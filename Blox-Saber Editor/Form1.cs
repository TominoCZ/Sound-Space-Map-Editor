using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NAudio.Utils;
using NAudio.Wave;

namespace Blox_Saber_Editor
{
    public partial class Form1 : Form
    {
        private static readonly string Folder = "./Assets/Sounds/";

        private long _loadedID = -1;

        private WaveStream _music;
        private WaveChannel32 _volumeStream;
        private WaveOutEvent _player = new WaveOutEvent();

        private List<Keys> _down = new List<Keys>();

        private Thread _updateThread;

        private Random r = new Random(); //TODO - temporary

        private TimeSpan _offset = TimeSpan.Zero;

        private bool _skipValueEvent;
        private bool _skipChangeEvent;

        public Form1()
        {
            InitializeComponent();

            Resize += (o, e) => { timeLine1.Invalidate(); };

            foreach (Control control in Controls)
            {
                control.KeyDown += Form1_KeyDown;

                control.KeyUp += Form1_KeyUp;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            _updateThread = new Thread(UpdateTimeLine) { IsBackground = true };
            _updateThread.Start();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (_down.Contains(e.KeyCode))
                return;

            _down.Add(e.KeyCode);

            if (_player.PlaybackState != PlaybackState.Playing)
                return;

            var x = r.Next(0, 3);
            var y = r.Next(0, 3);

            timeLine1.AddPoint(new TimeNode((int)_player.GetPositionTimeSpan().TotalMilliseconds, x, y));
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            _down.Remove(e.KeyCode);
        }

        private void indexSelector1_OnIndexChanged(object sender, EventArgs e)
        {
            if (timeLine1.GetActiveNode() is TimeNode node)
            {
                node.IndexX = indexSelector1.IndexX;
                node.IndexY = indexSelector1.IndexY;

                node.Tampered = true;
            }
        }

        private void timeLine1_OnActiveNodeChanged(object sender, EventArgs e)
        {
            if (timeLine1.GetActiveNode() is TimeNode node && !_skipChangeEvent)
            {
                indexSelector1.IndexX = node.IndexX;
                indexSelector1.IndexY = node.IndexY;

                _skipValueEvent = true;
                nudTime.Value = node.Time;
                _skipValueEvent = false;

                _skipChangeEvent = true;
                timeLine1.CurrentTime = TimeSpan.FromMilliseconds(node.Time);
                _skipChangeEvent = false;

                indexSelector1.Invalidate();
            }
        }

        private void btnLoadMusic_Click(object sender, EventArgs e)
        {
            if (!long.TryParse(tbID.Text = tbID.Text.Replace(" ", ""), out var id))
                return;

            if (LoadSound(id))
            {
                btnPlay.Enabled = true;
                _loadedID = id;
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            btnPlay.Enabled = false;
            btnStop.Enabled = true;
            btnPause.Enabled = true;

            ActiveControl = null;
            Focus();

            _player.Play();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnPlay.Enabled = true;
            btnStop.Enabled = false;
            btnPause.Enabled = false;

            pnlLoad.Enabled = true;

            _player.Stop();

            _music.CurrentTime = TimeSpan.Zero;
            timeLine1.CurrentTime = TimeSpan.Zero;

            _offset = TimeSpan.Zero;
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            btnPlay.Enabled = true;
            btnStop.Enabled = true;
            btnPause.Enabled = false;

            _player.Pause();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            timeLine1.ClearPoints();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Text file (*.txt)|*.txt"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                timeLine1.ClearPoints();

                var text = File.ReadAllText(ofd.FileName);

                var values = text.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                string id = "";

                foreach (var value in values)
                {
                    var split = value.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                    if (long.TryParse(split[0], out var x))
                    {
                        if (id == "")
                        {
                            id = value;
                            continue;
                        }

                        if (int.TryParse(split[1], out var y) && int.TryParse(split[2], out var time))
                        {
                            var point = new TimeNode(time, (int)x, y);

                            timeLine1.AddPoint(point);
                        }
                    }
                }

                tbID.Text = id;

                if (LoadSound(long.Parse(id)))
                {
                    btnPlay.Enabled = true;
                    _loadedID = long.Parse(id);
                }

                timeLine1.Invalidate();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var points = timeLine1.GetPoints();

                string text = _loadedID + ",";

                for (var index = 0; index < points.Count; index++)
                {
                    var point = points[index];

                    text += point.IndexX + "|";
                    text += point.IndexY + "|";
                    text += point.Time;

                    if (index < points.Count - 1)
                        text += ',';
                }

                File.WriteAllText(sfd.FileName, text);
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            if (_player == null)
                return;

            _player.Volume = tbVolume.Value / 100f;
        }

        private void UpdateTimeLine()
        {
            while (true)
            {
                timeLine1.BeginInvoke((MethodInvoker)(() =>
               {
                   if (_music == null || _player == null || !Visible || !IsHandleCreated || RecreatingHandle ||
                       !Created || Disposing || IsDisposed)
                       return;

                   timeLine1.Length = _music.TotalTime;

                   var time = _player.GetPositionTimeSpan() + _offset;

                   //timeLine1.CurrentTime = _player.PlaybackState == PlaybackState.Playing || _player.PlaybackState == PlaybackState.Paused ? (time > _music.TotalTime ? _music.TotalTime : time) : timeLine1.CurrentTime;

                   timeLine1.Invalidate();
               }));

                Thread.Sleep(16);
            }
        }

        private bool LoadSound(long id)
        {
            try
            {
                _music?.Dispose();
                _volumeStream?.Dispose();
                _player?.Dispose();

                if (!File.Exists(Folder + id + ".asset"))
                {
                    using (var wc = new MyWebClient())
                    {
                        wc.DownloadFile("https://assetgame.roblox.com/asset/?id=" + id, Folder + id + ".asset");
                    }
                }

                _music = new AudioFileReader(Folder + id + ".asset");
                _volumeStream = new WaveChannel32(_music);
                _player = new WaveOutEvent();

                _player.Init(_volumeStream);

                nudTime.Maximum = (int)_music.TotalTime.TotalMilliseconds;

                return true;
            }
            catch
            {
                MessageBox.Show($"Failed to download asset with id '{id}'", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return false;
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (!(timeLine1.GetPreviousNode() is TimeNode node))
                return;

            _offset = TimeSpan.FromMilliseconds(node.Time);

            btnPause.PerformClick();

            _player.Dispose();
            _player = new WaveOutEvent();

            _music.CurrentTime = timeLine1.CurrentTime = TimeSpan.FromMilliseconds(node.Time);
            _player.Init(_music);

            timeLine1.Invalidate();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (!(timeLine1.GetNextNode() is TimeNode node))
                return;

            _offset = TimeSpan.FromMilliseconds(node.Time);

            btnPause.PerformClick();

            _player.Dispose();
            _player = new WaveOutEvent();
            _music.CurrentTime = timeLine1.CurrentTime = TimeSpan.FromMilliseconds(node.Time);
            _player.Init(_music);

            timeLine1.Invalidate();
        }

        private void nudTime_ValueChanged(object sender, EventArgs e)
        {
            if (_skipValueEvent)
                return;

            if (!(timeLine1.GetActiveNode() is TimeNode node))
                return;
            
            node.Tampered = true;

            node.Time = (int)nudTime.Value;

            timeLine1.CurrentTime = TimeSpan.FromMilliseconds(node.Time);

            timeLine1.Invalidate();
        }
    }

    class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.UserAgent = "RobloxProxy";
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            return request;
        }
    }

    public class TimeNode
    {
        public int Time { get; set; }
        public int IndexX { get; set; }
        public int IndexY { get; set; }

        public bool Tampered { get; set; }

        public TimeNode(int time, int indexX, int indexY)
        {
            Time = time;
            IndexX = indexX;
            IndexY = indexY;
        }
    }
}
