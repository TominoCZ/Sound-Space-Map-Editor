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

        private WaveStream _waveStream;
        private WaveChannel32 _volume;
        private WaveOutEvent _player = new WaveOutEvent();

        private Thread _timelineThread;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _timelineThread = new Thread(UpdateTimeline) { IsBackground = true };
            _timelineThread.Start();
        }

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //TODO timeLine1.ClearPoints();

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
                    //_loadedID = long.Parse(id);
                }

                timeline1.Invalidate();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // TODO var points = timeLine1.GetPoints();

                // string text = _loadedID + ",";

                // for (var index = 0; index < points.Count; index++)
                {
                    // var point = points[index];

                    // text += point.IndexX + "|";
                    // text += point.IndexY + "|";
                    //  text += point.Time;

                    // if (index < points.Count - 1)
                    // text += ',';
                }

                //File.WriteAllText(sfd.FileName, text);
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
            Focus();

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

                    timeline1.TotalTime = _waveStream.TotalTime;

                    var time = _player.GetPositionTimeSpan();// + _offset; TODO

                    timeline1.CurrentTime = _player.PlaybackState == PlaybackState.Playing || _player.PlaybackState == PlaybackState.Paused ? (time > _waveStream.TotalTime ? _waveStream.TotalTime : time) : timeline1.CurrentTime;

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
