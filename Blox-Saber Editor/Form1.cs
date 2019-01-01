using Blox_Saber_Editor.Properties;
using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using VarispeedDemo.SoundTouch;

namespace Blox_Saber_Editor
{
	public partial class Form1 : Form
	{
		private static readonly string Folder = "./Assets/Sounds/";

		private long _loadedId = -1;

		private AudioPlayer _audioPlayer = new AudioPlayer();

		private Stopwatch _frameTimer = new Stopwatch();

		private List<Keys> _down = new List<Keys>();

		private Button[,] _grid = new Button[3, 3];
		private Button _mappedButton;

		private Dictionary<Keys, Tuple<int, int>> _mapping = new Dictionary<Keys, Tuple<int, int>>();

		private Random _r = new Random();

		private bool _bypassEvent;
		private bool _bypassSet;

		public static bool Saved = true;

		public Form1()
		{
			InitializeComponent();

			_grid[0, 0] = button1;
			_grid[1, 0] = button2;
			_grid[2, 0] = button3;

			_grid[0, 1] = button4;
			_grid[1, 1] = button5;
			_grid[2, 1] = button6;

			_grid[0, 2] = button7;
			_grid[1, 2] = button8;
			_grid[2, 2] = button9;

			_mapping.Add(Keys.Q, new Tuple<int, int>(0, 0));
			_mapping.Add(Keys.W, new Tuple<int, int>(1, 0));
			_mapping.Add(Keys.E, new Tuple<int, int>(2, 0));

			_mapping.Add(Keys.A, new Tuple<int, int>(0, 1));
			_mapping.Add(Keys.S, new Tuple<int, int>(1, 1));
			_mapping.Add(Keys.D, new Tuple<int, int>(2, 1));

			_mapping.Add(Keys.Z, new Tuple<int, int>(0, 2));
			_mapping.Add(Keys.X, new Tuple<int, int>(1, 2));
			_mapping.Add(Keys.C, new Tuple<int, int>(2, 2));

			Application.Idle += (o, e) =>
			{
				UpdateTimeline(_frameTimer.Elapsed.TotalSeconds);
				_frameTimer.Restart();
			};

			LoadMappings();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			timeline1.OnDrag += timeline1_OnDrag;
			timeline1.OnDragBegin += timeline1_OnDragBegin;
			timeline1.OnDragEnd += timeline1_OnDragEnd;

			GotFocus += (o, evt) =>
			{
				ActiveControl = null;
			};
			LostFocus += (o, evt) =>
			{
				ActiveControl = null;
			};

			chbSmooth.Checked = Settings.Default.smooth;
			tbVolume.Value = Settings.Default.volume;
			tbID.Text = Settings.Default.id;

			void AssingEvents(Control c)
			{
				c.Click += (o, evt) => { ActiveControl = null; };
				c.MouseClick += (o, evt) => { ActiveControl = null; };
				c.MouseLeave += (o, evt) => { ActiveControl = null; };
				c.MouseEnter += (o, evt) => { ActiveControl = null; };
				c.MouseUp += (o, evt) => { ActiveControl = null; };
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
					c.Click += (o, evt) => { ActiveControl = null; };
			}
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{

			if (_down.Contains(e.KeyCode) && e.KeyCode != Keys.R && e.KeyCode != Keys.T)
				return;
			else
				_down.Add(e.KeyCode);

			if (pnlMap.Visible)
			{
				if (_mappedButton != null && e.KeyCode != Keys.Back && e.KeyCode != Keys.Escape && e.KeyCode != Keys.ShiftKey)
				{
					var xy = GetGridButtonIndex(_mappedButton);

					_mappedButton.Text = e.KeyCode.ToString();

					gridButton_Click(_mappedButton, null);

					MapKey(e.KeyCode, xy);

					SaveMappings();
				}

				if (e.KeyCode != Keys.ShiftKey)
				{
					_mappedButton = null;
					pnlMap.Visible = false;
				}
				return;
			}
			else
			{
				if (_mapping.TryGetValue(e.KeyCode, out var xy))
				{
					//var btn = _grid[xy.Item1, xy.Item2];

					//btn.PerformClick();

					if (_audioPlayer.IsPlaying)
					{
						var timeStamp = new TimeStamp((int)_audioPlayer.CurrentTime.TotalMilliseconds, xy.Item1, 2 - xy.Item2);

						timeline1.Add(timeStamp);

						return;
					}
				}
			}

			if (e.KeyCode == Keys.R)
			{
				btnPrev.PerformClick();
			}
			else if (e.KeyCode == Keys.T)
			{
				btnNext.PerformClick();
			}
			else if (e.KeyCode == Keys.Delete)
			{
				var ts = timeline1.GetCurrentTimeStamp();

				if (ts != null)
				{
					var d = MessageBox.Show("Delete this note?", "Delte?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

					if (d == DialogResult.Yes)
					{
						timeline1.Remove(ts);
						timeline1.Invalidate();
					}
				}
			}

			if (_audioPlayer.IsPlaying && e.KeyCode == Keys.Space)
			{
				var timeStamp = new TimeStamp((int)_audioPlayer.CurrentTime.TotalMilliseconds, _r.Next(0, 3), _r.Next(0, 3));

				timeline1.Add(timeStamp);
			}
		}

		private void Form1_KeyUp(object sender, KeyEventArgs e)
		{
			_down.Remove(e.KeyCode);
		}

		private void btnLoadSong_Click(object sender, EventArgs e)
		{
			if (long.TryParse(tbID.Text, out var id))
			{
				if (LoadSound(id))
				{
					btnPlay.Enabled = true;

					_loadedId = id;
				}
			}
			else
			{
				MessageBox.Show("Please enter a valid asset ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			if (sfd.ShowDialog() == DialogResult.OK)
			{
				File.WriteAllText(sfd.FileName, GetParsedText());

				Saved = true;
			}
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			var d = MessageBox.Show("Are you sure you want to clear the whole timeline?", "Clear", MessageBoxButtons.YesNo);

			if (d == DialogResult.Yes)
			{
				timeline1.Clear();
			}
		}

		private void btnCopy_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(GetParsedText());

			MessageBox.Show("Map copied to clipboard.", "Copy", MessageBoxButtons.OK);
		}

		private void btnPlay_Click(object sender, EventArgs e)
		{
			btnPlay.Enabled = false;
			btnStop.Enabled = true;
			btnPause.Enabled = true;

			ActiveControl = null;

			_audioPlayer.Play();
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			btnPlay.Enabled = true;
			btnStop.Enabled = false;
			btnPause.Enabled = false;

			_audioPlayer.Stop();
		}

		private void btnPause_Click(object sender, EventArgs e)
		{
			btnPlay.Enabled = true;
			btnStop.Enabled = true;
			btnPause.Enabled = false;

			_audioPlayer.Pause();
		}

		private void btnLoadFile_Click(object sender, EventArgs e)
		{
			if (!Saved)
			{
				var d = MessageBox.Show("You have unsaved work.\nContinue?", "Load a Map file", MessageBoxButtons.YesNo);

				if (d != DialogResult.Yes)
					return;
			}

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
							var point = new TimeStamp(time, 2 - (int)x, y);

							timeline1.Add(point);
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

		private void btnPrev_Click(object sender, EventArgs e)
		{
			btnPause.PerformClick();

			lock (_grid)
			{
				var ts = timeline1.GetPreviousTimeStamp();

				timeline1.CurrentTime = TimeSpan.FromMilliseconds(ts?.Time ?? 0);

				_wasPlaying = _audioPlayer.IsPlaying;
				_audioPlayer.Stop();
				_audioPlayer.CurrentTime = timeline1.CurrentTime;

				if (_wasPlaying)
					_audioPlayer.Play();

				_bypassSet = true;
				nudTimeStamp.Value = (int)timeline1.CurrentTime.TotalMilliseconds;
				_bypassSet = false;

				if (ts != null)
				{
					SetActiveGridButton(_grid[ts.X, 2 - ts.Y]);
				}
				else
				{
					SetActiveGridButton(null);
				}

				timeline1.Invalidate();
			}
		}

		private void btnNext_Click(object sender, EventArgs e)
		{
			btnPause.PerformClick();

			lock (_grid)
			{
				var ts = timeline1.GetNextTimeStamp();

				timeline1.CurrentTime = TimeSpan.FromMilliseconds(ts?.Time ?? timeline1.TotalTime.TotalMilliseconds);

				_wasPlaying = _audioPlayer.IsPlaying;
				_audioPlayer.Stop();
				_audioPlayer.CurrentTime = timeline1.CurrentTime;

				if (_wasPlaying)
					_audioPlayer.Play();

				_bypassSet = true;
				nudTimeStamp.Value = (int)timeline1.CurrentTime.TotalMilliseconds;
				_bypassSet = false;

				if (ts != null)
				{
					SetActiveGridButton(_grid[ts.X, 2 - ts.Y]);
				}
				else
				{
					SetActiveGridButton(null);
				}

				timeline1.Invalidate();
			}
		}

		private void gridButton_Click(object sender, EventArgs e)
		{
			if (_down.Contains(Keys.ShiftKey))
			{
				pnlMap.Visible = true;

				_mappedButton = (Button)sender;

				return;
			}

			SetActiveGridButton((Button)sender);

			var xy = GetGridButtonIndex((Button)sender);

			var btn = _grid[xy.Item1, xy.Item2];

			if (!_audioPlayer.IsPlaying)
			{
				var ts = timeline1.GetCurrentTimeStamp();

				if (ts != null)
				{
					if (ts.X != xy.Item1 || ts.Y != 2 - xy.Item2)
					{
						ts.X = xy.Item1;
						ts.Y = 2 - xy.Item2;

						ts.Dirty = true;

						Saved = false;
					}
				}
			}
		}

		private void nudTimeStamp_ValueChanged(object sender, EventArgs e)
		{
			if (!_audioPlayer.IsPlaying && !_bypassSet)
			{
				var ts = timeline1.GetCurrentTimeStamp();

				if (ts == null)
					return;

				ts.Time = (int)nudTimeStamp.Value;

				if (!_bypassEvent)
				{
					timeline1.CurrentTime = TimeSpan.FromMilliseconds(ts.Time);
					ts.Dirty = true;
					Saved = false;
				}

				timeline1.Sort();

				timeline1.Invalidate();
			}

			ActiveControl = null;
		}

		private void chbSmooth_CheckedChanged(object sender, EventArgs e)
		{
			Settings.Default.smooth = timeline1.Smooth = chbSmooth.Checked;
			Settings.Default.Save();

			ActiveControl = null;
		}

		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			Settings.Default.volume = tbVolume.Value;
			Settings.Default.Save();
		}

		private void tbTempo_MouseUp(object sender, MouseEventArgs e)
		{
			_audioPlayer.Speed = tbTempo.Value / 100f;

			if (_wasPlaying)
				_audioPlayer.Play();
		}

		private bool _wasPlaying;

		private void timeline1_OnDragBegin(object sender, EventArgs e)
		{
			_wasPlaying = _audioPlayer.IsPlaying;
		}

		private void timeline1_OnDrag(object sender, float progress)
		{
			_wasPlaying = _audioPlayer.IsPlaying;
			_audioPlayer.Stop();
			timeline1.CurrentTime = TimeSpan.FromMilliseconds(timeline1.TotalTime.TotalMilliseconds * progress);
			_audioPlayer.CurrentTime = timeline1.CurrentTime;
			if (_wasPlaying)
				_audioPlayer.Play();

			btnPause.PerformClick();

			//update
			var ts = timeline1.GetPreviousTimeStamp();

			if (ts != null)
			{
				_bypassSet = true;
				nudTimeStamp.Value = ts.Time;
				_bypassSet = false;

				SetActiveGridButton(_grid[ts.X, 2 - ts.Y]);
			}
			else
			{
				SetActiveGridButton(null);
			}

			timeline1.Invalidate();
		}

		private void timeline1_OnDragEnd(object sender, EventArgs e)
		{
			if (_wasPlaying)
			{
				btnPlay.PerformClick();
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (Saved)
				return;

			var d = MessageBox.Show("You have unsaved work.\nAre you sure?", "Exit", MessageBoxButtons.YesNo);

			if (d != DialogResult.Yes)
				e.Cancel = true;
		}

		private void MapKey(Keys key, Tuple<int, int> xy)
		{
			List<Keys> keys = new List<Keys>();

			foreach (var p in _mapping)
			{
				if (p.Value.Item1 == xy.Item1 && p.Value.Item2 == xy.Item2)
					keys.Add(p.Key);
			}

			foreach (var k in keys)
			{
				_mapping.Remove(k);
			}

			_mapping.Add(key, xy);
		}

		private void LoadMappings()
		{
			var file = "blox_saber_editor.cfg";

			if (File.Exists(file))
			{
				var lines = File.ReadAllLines(file);

				foreach (var line in lines)
				{
					var trimmed = line.Replace(" ", "");

					if (trimmed.Length < 4)
						continue;

					try
					{
						var split = trimmed.Split('=');

						var key = (Keys)int.Parse(split[0]);

						var pos = split[1].Split(',');

						var x = 2 - int.Parse(pos[0]);
						var y = 2 - int.Parse(pos[1]);

						MapKey(key, new Tuple<int, int>(x, y));

						_grid[x, y].Text = key.ToString();
					}
					catch
					{

					}
				}
			}
		}

		private void SaveMappings()
		{
			var file = "blox_saber_editor.cfg";

			var sb = new StringBuilder();

			for (int x = 0; x < 3; x++)
			{
				for (int y = 0; y < 3; y++)
				{
					var btn = _grid[x, y];

					if (Enum.TryParse(btn.Text, out Keys k))
					{
						sb.AppendLine($"{(int)k}={2 - x},{2 - y}");
					}
				}
			}

			File.WriteAllText(file, sb.ToString());
		}

		private void UpdateTimeline(double delta)
		{
			_audioPlayer.Volume = tbVolume.Value / 100f;

			_audioPlayer.Update(_frameTimer.Elapsed.TotalSeconds);

			var time = _audioPlayer.CurrentTime;
			timeline1.CurrentTime = time;

			var ts = timeline1.GetCurrentTimeStamp();

			if (_audioPlayer.IsPlaying)
			{
				if (ts != null)
				{
					var btn = _grid[ts.X, 2 - ts.Y];

					SetActiveGridButton(btn);
					btn.Invalidate();

					_bypassEvent = true;
					nudTimeStamp.Value = ts.Time;
					_bypassEvent = false;
				}
			}

			var total = timeline1.GetCount();
			var number = timeline1.GetNumber(ts);

			lblNote.Text = number == 0 ? "Note" : $"Note #{number}/{total}";

			if (_audioPlayer.IsDonePlaying)
			{
				btnPlay.Enabled = true;
				btnStop.Enabled = false;
				btnPause.Enabled = false;
			}

			timeline1.Invalidate();
		}

		private bool LoadSound(long id)
		{
			try
			{
				if (!Directory.Exists(Folder))
					Directory.CreateDirectory(Folder);

				if (!File.Exists(Folder + id + ".asset"))
				{
					using (var wc = new SecureWebClient())
					{
						wc.DownloadFile("https://assetgame.roblox.com/asset/?id=" + id, Folder + id + ".asset");
					}
				}

				_audioPlayer.Load(Folder + id + ".asset");

				nudTimeStamp.Maximum = (int)_audioPlayer.TotalTime.TotalMilliseconds;
				timeline1.TotalTime = _audioPlayer.TotalTime;

				timeline1.CurrentTime = TimeSpan.Zero;

				timeline1.Invalidate();

				Settings.Default.id = id.ToString();
				Settings.Default.Save();

				Saved = true;

				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				MessageBox.Show($"Failed to download asset with id '{id}'", "Error", MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}

			return false;
		}

		private void SetActiveGridButton(Button b)
		{
			for (int x = 0; x < 3; x++)
			{
				for (int y = 0; y < 3; y++)
				{
					var btn = _grid[x, y];

					btn.BackColor = btn == b ? Color.DeepPink : SystemColors.ControlLight;
				}
			}
		}

		private Tuple<int, int> GetGridButtonIndex(Button b)
		{
			for (int x = 0; x < 3; x++)
			{
				for (int y = 0; y < 3; y++)
				{
					var btn = _grid[x, y];

					if (btn == b)
						return new Tuple<int, int>(x, y);
				}
			}

			return null;
		}

		private string GetParsedText()
		{
			var points = timeline1.GetPoints();

			string text = _loadedId + ",";

			for (var index = 0; index < points.Count; index++)
			{
				var point = points[index];

				text += (2 - point.X) + "|";
				text += point.Y + "|";
				text += point.Time;

				if (index < points.Count - 1)
					text += ',';
			}

			return text;
		}
	}

	class AudioPlayer : IDisposable
	{
		private WaveStream _music;
		private WaveChannel32 _volumeStream;
		private WaveOutEvent _player;
		private VarispeedSampleProvider _speedControl;

		private Timer _time = new Timer();

		public AudioPlayer()
		{
			_player = new WaveOutEvent();
		}

		public void Load(string file)
		{
			_music?.Dispose();
			_volumeStream?.Dispose();
			_player?.Dispose();
			_speedControl?.Dispose();

			var reader = new AudioFileReader(file);
			_music = reader;
			_volumeStream = new WaveChannel32(_music);
			_player = new WaveOutEvent();

			_speedControl = new VarispeedSampleProvider(reader, 100, new SoundTouchProfile(true, true));

			Init();

			_time.Reset();
		}

		public void Init() => _player.Init(_speedControl);
		public void Play()
		{
			if (TotalTime == CurrentTime)
			{
				CurrentTime = TimeSpan.Zero;
			}

			if (_player == null)
				return;

			_player.Play();
			_time.Start();
		}
		public void Pause()
		{
			_time.Stop();
			_player.Pause();
		}
		public void Stop()
		{
			_time.Reset();
			_player.Stop();

			if (_music == null)
				return;

			_music.Position = 0;
			_music.CurrentTime = TimeSpan.Zero;
		}

		public float Speed
		{
			get => _speedControl == null ? 1 : _speedControl.PlaybackRate;

			set
			{
				if (_speedControl == null)
					return;

				var wasPlaying = IsPlaying;

				Pause();
				var time = _time.Elapsed;
				Stop();

				_speedControl.PlaybackRate = value;

				CurrentTime = time;

				Init();

				if (wasPlaying)
					Play();
			}
		}

		public float Volume
		{
			get => _player.Volume;

			set => _player.Volume = value;
		}

		public bool IsPlaying => _player.PlaybackState == PlaybackState.Playing;
		public bool IsPaused => _player.PlaybackState == PlaybackState.Paused;

		public bool IsDonePlaying => TotalTime == CurrentTime;

		public TimeSpan TotalTime => _music == null ? TimeSpan.Zero : _music.TotalTime;

		public TimeSpan CurrentTime
		{
			get
			{
				if (_music == null)
				{
					return TimeSpan.Zero;
				}

				var time = _time.Elapsed;

				time = time >= _music.TotalTime ? _music.TotalTime : time;

				return time;
			}
			set
			{
				if (_music == null)
					return;

				_music.CurrentTime = value;
				_time.Elapsed = value;

				_speedControl.Reposition();
			}
		}

		public double Progress => TotalTime == TimeSpan.Zero ? 0 : Math.Min(1, CurrentTime.TotalMilliseconds / TotalTime.TotalMilliseconds);

		public void Update(double delta)
		{
			_time.Update(delta * Speed);
		}

		public void Dispose()
		{
			_player.Dispose();
			_speedControl.Dispose();
			_music.Dispose();
			_volumeStream.Dispose();
		}
	}

	class Timer
	{
		private double _elapsed;

		public bool IsRunning { get; private set; }

		public TimeSpan Elapsed
		{
			get => TimeSpan.FromSeconds(_elapsed);
			set => _elapsed = value.TotalSeconds;
		}

		public void Start() => IsRunning = true;

		public void Stop() => IsRunning = false;

		public void Reset()
		{
			IsRunning = false;
			_elapsed = 0;
		}

		public void Update(double delta)
		{
			if (IsRunning)
				_elapsed += delta;
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
