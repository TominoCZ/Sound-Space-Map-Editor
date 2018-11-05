using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Utils;
using NAudio.Wave;

namespace Blox_Saber_Editor
{
    public partial class Form1 : Form
    {
        private static readonly string Folder = "./Assets/Sounds/";

        private long _loadedId = -1;

        private WaveStream _waveStream;
        private WaveChannel32 _volume;
        private WaveOutEvent _player = new WaveOutEvent();

        private Thread _timelineThread;

        private List<Keys> _down = new List<Keys>();

        private TimeSpan _playOffset = TimeSpan.Zero;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _timelineThread = new Thread(UpdateTimeline) { IsBackground = true };
            _timelineThread.Start();

            void AssingEvents(Control c)
            {
                c.Click += (o, evt) => { this.ActiveControl = null; };
                c.MouseClick += (o, evt) => { this.ActiveControl = null; };
                c.MouseLeave += (o, evt) => { this.ActiveControl = null; };
                c.MouseUp += (o, evt) => { this.ActiveControl = null; };
            }

            foreach (Control c in Controls)
            {
                if (c is Panel p)
                {
                    foreach (Control pc in p.Controls)
                    {
                        if (pc is Button)
                        {
                            AssingEvents(pc);
                        }
                    }
                }
                else if (c is Button)
                    c.Click += (o, evt) => { this.ActiveControl = null; };
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (_down.Contains(e.KeyCode))
                return;

            _down.Add(e.KeyCode);

            if (_player == null || _player.PlaybackState != PlaybackState.Playing)
                return;

            var timeStamp = new TimeStamp((int)_player.GetPositionTimeSpan().TotalMilliseconds, 1, 1);

            timeline1.AddPoint(timeStamp);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            _down.Remove(e.KeyCode);
        }

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                timeline1.Clear();

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
                            var point = new TimeStamp(time, (int)x, y);

                            timeline1.AddPoint(point);
                        }
                    }
                }

                tbID.Text = id;

                if (LoadSound(long.Parse(id)))
                {
                    btnPlay.Enabled = true;
                    _loadedId = long.Parse(id);
                }

                timeline1.Invalidate();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var points = timeline1.GetPoints();

                string text = _loadedId + ",";

                for (var index = 0; index < points.Count; index++)
                {
                    var point = points[index];

                    text += point.X + "|";
                    text += point.Y + "|";
                    text += point.Time;

                    if (index < points.Count - 1)
                        text += ',';
                }

                File.WriteAllText(sfd.FileName, text);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            timeline1.Clear();
        }

        private void btnLoadSong_Click(object sender, EventArgs e)
        {
            if (long.TryParse(tbID.Text, out var id))
            {
                if (LoadSound(id))
                {
                    btnPlay.Enabled = true;

                    nudTimeStamp.Maximum = (int)_waveStream.TotalTime.TotalMilliseconds;
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid asset ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            btnPlay.Enabled = false;
            btnStop.Enabled = true;
            btnPause.Enabled = true;

            ActiveControl = null;

            _player.Stop();

            var current = timeline1.GetCurrentTimeStamp();

            _playOffset = TimeSpan.FromMilliseconds(current?.Time ?? 0);
            
            _waveStream.CurrentTime = _playOffset;

            _player = new WaveOutEvent();
            _player.Init(_waveStream);

            _player.Play();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnPlay.Enabled = true;
            btnStop.Enabled = false;
            btnPause.Enabled = false;

            _player.Stop();

            _waveStream.CurrentTime = TimeSpan.Zero;
            timeline1.CurrentTime = TimeSpan.Zero;

            //TODO_offset = TimeSpan.Zero;
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            btnPlay.Enabled = true;
            btnStop.Enabled = true;
            btnPause.Enabled = false;

            _player.Pause();
        }

        private void UpdateTimeline()
        {
            while (true)
            {
                timeline1.BeginInvoke((MethodInvoker)(() =>
                {
                    if (_waveStream == null || _player == null || !Visible || !IsHandleCreated || RecreatingHandle ||
                        !Created || Disposing || IsDisposed)
                        return;

                    _player.Volume = trackBar1.Value / 100f;

                    timeline1.TotalTime = _waveStream.TotalTime;

                    var time = _player.GetPositionTimeSpan() + _playOffset;

                    if (_player.PlaybackState == PlaybackState.Playing)
                    {
                        if (time > _waveStream.TotalTime)
                            timeline1.CurrentTime = _waveStream.TotalTime;
                        else
                            timeline1.CurrentTime = time;
                    }

                    timeline1.Invalidate();
                }));

                Thread.Sleep(16);
            }
        }

        private bool LoadSound(long id)
        {
            try
            {
                _waveStream?.Dispose();
                _volume?.Dispose();
                _player?.Dispose();

                if (!Directory.Exists(Folder))
                    Directory.CreateDirectory(Folder);

                if (!File.Exists(Folder + id + ".asset"))
                {
                    using (var wc = new SecureWebClient())
                    {
                        wc.DownloadFile("https://assetgame.roblox.com/asset/?id=" + id, Folder + id + ".asset");
                    }
                }

                _waveStream = new AudioFileReader(Folder + id + ".asset");
                _volume = new WaveChannel32(_waveStream);
                _player = new WaveOutEvent();

                _player.Init(_volume);

                return true;
            }
            catch
            {
                MessageBox.Show($"Failed to download asset with id '{id}'", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            return false;
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            btnPause.PerformClick();

            var ts = timeline1.GetPreviousTimeStamp();

            if (ts == null)
                return;

            timeline1.CurrentTime = TimeSpan.FromMilliseconds(ts.Time);

            nudTimeStamp.Value = (int)timeline1.CurrentTime.TotalMilliseconds;

            timeline1.Invalidate();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            btnPause.PerformClick();

            var ts = timeline1.GetNextTimeStamp();

            if (ts == null)
                return;

            timeline1.CurrentTime = TimeSpan.FromMilliseconds(ts.Time);

            nudTimeStamp.Value = (int)timeline1.CurrentTime.TotalMilliseconds;

            timeline1.Invalidate();
        }

        private void nudTimeStamp_ValueChanged(object sender, EventArgs e)
        {
            //TODO avoid calls from button events
            if (_player.PlaybackState != PlaybackState.Playing)
            {
                var ts = timeline1.GetCurrentTimeStamp();

                var newTs = new TimeStamp((int)nudTimeStamp.Value, ts.X, ts.Y);

                timeline1.Replace(ts, newTs);

                timeline1.Invalidate();
            }
        }

        class SecureWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
                request.UserAgent = "RobloxProxy";
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                return request;
            }
        }
    }
}
