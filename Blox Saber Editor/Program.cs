using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NAudio.Wave;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using VarispeedDemo.SoundTouch;
using PlaybackState = NAudio.Wave.PlaybackState;

namespace Blox_Saber_Editor
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			using (var w = new EditorWindow())
			{
				w.Run();
			}
		}
	}

	class EditorWindow : GameWindow
	{
		public static EditorWindow Instance;

		public bool IsPaused { get; private set; }

		public GuiRenderer GuiRenderer;

		public GuiScreen GuiScreen;

		public MusicPlayer MusicPlayer;
		public SoundPlayer SoundPlayer;

		private readonly GuiScreenMain _screenMain;

		private readonly UndoRedo _undoRedo = new UndoRedo();

		private Note _draggedNote;
		private Note _lastPlayedNote;

		private PointF _lastMouse;

		private int _dragStartX;
		private int _dragStartMs;

		private bool _mouseDown;
		private bool _draggingCursor;
		private bool _draggingNote;
		private bool _draggingTimeline;

		private bool _wasPlaying;

		private readonly int _soundID = -1;

		public List<Note> Notes = new List<Note>();

		public ColorSequence Colors = new ColorSequence();

		private float _zoom = 1;
		public float Zoom
		{
			get => _zoom;
			set => _zoom = Math.Max(1, Math.Min(4, value));
		}

		public float CubeStep => 50 * 10 * Zoom;

		public EditorWindow() : base(800, 600, new GraphicsMode(32, 8, 0, 4), "Blox Saber Editor")
		{
			Instance = this;

			Icon = Properties.Resources.Blox_Saber;
			VSync = VSyncMode.Off;

			TargetRenderFrequency = 240;

			MusicPlayer = new MusicPlayer();
			SoundPlayer = new SoundPlayer();

			GuiRenderer = new GuiRenderer();
			_screenMain = new GuiScreenMain();

			OpenGuiScreen(_screenMain);

			SoundPlayer.Init();

			LoadMap("map.txt");

			MusicPlayer.Load("song.mp3");
			MusicPlayer.Play();

			SoundPlayer.Cache("hit", "assets/sounds/hit.wav");
		}

		protected override void OnLoad(EventArgs e)
		{
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			GL.Enable(EnableCap.Texture2D);
			GL.ActiveTexture(TextureUnit.Texture0);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			MusicPlayer.Update();
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			GL.MatrixMode(MatrixMode.Projection);
			var m = Matrix4.CreateOrthographicOffCenter(0, Width, Height, 0, 0, 1);
			GL.LoadMatrix(ref m);

			_screenMain.Progress = MusicPlayer.Progress;

			if (MusicPlayer.IsPlaying)
			{
				var closest = Notes.OrderBy(n => n.Ms).LastOrDefault(n => n.Ms <= (int)MusicPlayer.CurrentTime.TotalMilliseconds);

				if (_lastPlayedNote != closest)
				{
					_lastPlayedNote = closest;

					if (closest != null)
					{
						SoundPlayer.Play("hit", 0.8f);
					}
				}
			}

			#region old

			//_bd.update();
			//var beat = _bd.getLastBeat();

			//b = (float)Math.Max(0, b - e.Time * 8);

			//if (_ts != beat)
			//{
			//Beat Occured
			//b = 1;
			//Console.WriteLine(DateTime.Now.Ticks);

			//Update localLastBeat
			//_ts = beat;
			//}

			//GL.ClearColor(b, b, b, 1);

			#endregion

			GuiRenderer.Render(_lastMouse.X, _lastMouse.Y);

			GuiScreen?.Render(_lastMouse.X, _lastMouse.Y);

			SwapBuffers();
		}

		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(ClientRectangle);

			_screenMain.OnResize(ClientSize);
		}

		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			_lastMouse = new PointF(e.X, e.Y);

			GuiScreen?.OnMouseMove(e.X, e.Y);

			if (_draggingCursor)
			{
				var wasPlaying = MusicPlayer.IsPlaying;

				MusicPlayer.Pause();

				var progress = (e.X - _screenMain.ClientRectangle.Height / 2f) /
							   (_screenMain.ClientRectangle.Width - _screenMain.ClientRectangle.Height);

				progress = Math.Max(0, Math.Min(1, progress));

				MusicPlayer.Stop();
				MusicPlayer.CurrentTime = TimeSpan.FromMilliseconds(MusicPlayer.TotalTime.TotalMilliseconds * progress);

				MusicPlayer.Pause();

				if (wasPlaying)
				{
					MusicPlayer.Play();
				}
			}

			if (_draggingNote)
			{
				OnDraggingNote(e.X);
			}
			if (_draggingTimeline)
			{
				OnDraggingTimeline(e.X);
			}
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			_mouseDown = true;

			GuiScreen?.OnMouseClick(e.X, e.Y);

			if (_screenMain.Track.MouseOverNote is Note note)
			{
				MusicPlayer.Pause();

				_draggingNote = true;
				_dragStartX = e.X;
				_dragStartMs = note.Ms;

				_draggedNote = note;
			}
			else if (_screenMain.Track.ClientRectangle.Contains(e.X, e.Y))
			{
				_wasPlaying = MusicPlayer.IsPlaying;

				MusicPlayer.Pause();

				_draggingTimeline = true;
				_dragStartX = e.X;
				_dragStartMs = (int)MusicPlayer.CurrentTime.TotalMilliseconds;
			}

			if (_screenMain.ClientRectangle.Contains(e.Position))
			{
				MusicPlayer.Pause();
				_draggingCursor = true;

				OnMouseMove(new MouseMoveEventArgs(e.X, e.Y, 0, 0));
			}
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (_draggingNote)
			{
				MusicPlayer.Pause();
				OnDraggingNote(e.X);

				_lastPlayedNote = Notes.OrderBy(n => n.Ms).LastOrDefault(n => n.Ms <= Math.Floor(MusicPlayer.CurrentTime.TotalMilliseconds));

				var note = _draggedNote;
				var start = _dragStartMs;
				var diff = note.Ms - start;

				_undoRedo.AddUndoRedo(() => { note.Ms = start; }, () => { note.Ms = start + diff; });
			}
			if (_draggingTimeline)
			{
				MusicPlayer.Stop();
				OnDraggingTimeline(e.X);

				_lastPlayedNote = Notes.OrderBy(n => n.Ms).LastOrDefault(n => n.Ms <= Math.Floor(MusicPlayer.CurrentTime.TotalMilliseconds));

				if (_wasPlaying)
					MusicPlayer.Play();
			}

			_draggingNote = false;
			_draggingTimeline = false;
			_draggingCursor = false;
			_mouseDown = false;
		}

		protected override void OnKeyDown(KeyboardKeyEventArgs e)
		{
			if (e.Key == Key.F11)
			{
				if (WindowState == WindowState.Normal)
					WindowState = WindowState.Fullscreen;
				else if (WindowState == WindowState.Fullscreen)
					WindowState = WindowState.Normal;
			}

			if (e.Control)
			{
				if (e.Key == Key.Z)
				{
					MusicPlayer.Pause();

					_undoRedo.Undo();
				}
				else if (e.Key == Key.Y)
				{
					MusicPlayer.Pause();

					_undoRedo.Redo();
				}
			}

			if (e.Key == Key.Space)
			{
				if (!_draggingTimeline && !_draggingNote && !_draggingCursor)
				{
					if (MusicPlayer.IsPlaying)
					{
						MusicPlayer.Pause();
					}
					else
					{
						MusicPlayer.Play();
					}
				}
			}
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			var dist = e.DeltaPrecise / 10;

			if (Keyboard.GetState().IsKeyDown(Key.LControl))
				Zoom += dist;
			else if (Keyboard.GetState().IsKeyDown(Key.ShiftLeft)) //TODO - temporary
			{
				var newSpeed = Math.Max(0.2f, Math.Min(1, MusicPlayer.Speed + (e.Delta > 0 ? 1 : -1) * 0.1f));

				if (newSpeed != MusicPlayer.Speed)
					MusicPlayer.Speed = newSpeed;
			}
			else
			{
				MusicPlayer.Pause();
				var time = MusicPlayer.CurrentTime.TotalSeconds;
				MusicPlayer.Stop();

				time = Math.Max(0, Math.Min(MusicPlayer.TotalTime.TotalMilliseconds, time + dist / Zoom * 0.5));

				MusicPlayer.CurrentTime = TimeSpan.FromSeconds(time);
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			MusicPlayer.Dispose();
			SoundPlayer.Dispose();
		}

		private void OnDraggingNote(int mouseX)
		{
			var pixels = mouseX - _dragStartX;
			var msDiff = pixels / CubeStep * 1000;

			var time = _dragStartMs + (int)msDiff;

			time = (int)Math.Max(0, Math.Min(MusicPlayer.TotalTime.TotalMilliseconds, time));

			_draggedNote.Ms = time;
		}

		private void OnDraggingTimeline(int mouseX)
		{
			var pixels = mouseX - _dragStartX;
			var msDiff = pixels / CubeStep * 1000;

			var time = _dragStartMs - (int)msDiff;

			time = (int)Math.Max(0, Math.Min(MusicPlayer.TotalTime.TotalMilliseconds, time));

			MusicPlayer.CurrentTime = TimeSpan.FromMilliseconds(time);
		}

		private void LoadMap(string file)
		{
			Colors.Reset();
			Notes.Clear();

			var text = File.ReadAllText(file);

			var splits = Regex.Matches(text, "([^,]+)");

			var ID = splits[0];

			for (int i = 1; i < splits.Count; i++)
			{
				var chunk = splits[i];

				var chunkSplit = Regex.Matches(chunk.Value, "([^|]+)");

				var x = 2 - int.Parse(chunkSplit[0].Value);
				var y = 2 - int.Parse(chunkSplit[1].Value);
				var ms = int.Parse(chunkSplit[2].Value);

				Notes.Add(new Note(x, y, ms) { Color = Colors.Next() });
			}
		}

		private void SaveMap(string file)
		{
			OpenFileDialog ofd = new OpenFileDialog
			{
				Filter = ".TXT|*.TXT"
			};

			var result = ofd.ShowDialog();

			if (result == DialogResult.OK)
			{
				using (var fs = File.OpenWrite(ofd.FileName))
				{
					using (var sw = new StreamWriter(fs))
					{
						sw.Write(_soundID.ToString());

						for (int i = 0; i < Notes.Count; i++)
						{
							Note note = Notes[i];

							var gridX = 2 - note.X;
							var gridY = 2 - note.Y;

							sw.Write($",{gridX}|{gridY}|{note.Ms}");
						}
					}
				}
			}
		}

		public void OpenGuiScreen(GuiScreen s)
		{
			GuiScreen?.OnClosing();

			IsPaused = s != null && s.Pauses;

			GuiScreen = s;
		}
	}

	class SoundPlayer : IDisposable
	{
		private AudioContext _context;

		private readonly Dictionary<string, Tuple<int, int>> _sounds = new Dictionary<string, Tuple<int, int>>();

		private string _lastId;

		public void Init()
		{
			_context = new AudioContext();

			AL.Listener(ALListenerf.Gain, 1);
			AL.Listener(ALListener3f.Position, 0, 0, 0);
			AL.Listener(ALListener3f.Velocity, 0, 0, 0);
		}

		public void Cache(string id, string file)
		{
			//create a buffer
			byte[] data;
			WaveFormat format;

			using (var afr = new AudioFileReader(file))
			{
				data = new byte[afr.Length];

				var provider = afr.ToSampleProvider().ToStereo().ToWaveProvider16();
				provider.Read(data, 0, data.Length);

				format = provider.WaveFormat;
			}

			var buffer = AL.GenBuffer();
			AL.BufferData(buffer, ALFormat.Stereo16, data, data.Length, format.SampleRate);

			//create audio source
			var source = AL.GenSource();
			AL.Source(source, ALSourcef.Gain, 0f);
			AL.Source(source, ALSourcef.Pitch, 1);
			AL.Source(source, ALSource3f.Position, 0, 0, 0);

			AL.BindBufferToSource(source, buffer);

			_sounds.Add(id, new Tuple<int, int>(source, buffer));
		}

		public void Play(string id, float volume = 1)
		{
			if (_sounds.TryGetValue(id, out var sound))
			{
				if (id != _lastId)
				{
					_lastId = id;

					AL.Source(sound.Item1, ALSourcei.Buffer, sound.Item2);
				}

				AL.Source(sound.Item1, ALSourcef.Gain, volume);
				AL.SourcePlay(sound.Item1);
			}
		}

		public void Dispose()
		{
			foreach (var tuple in _sounds.Values)
			{
				AL.DeleteSource(tuple.Item1);
				AL.DeleteBuffer(tuple.Item2);
			}

			_context.Dispose();
		}
	}

	class UndoRedo
	{
		private readonly List<UndoRedoAction> _actions = new List<UndoRedoAction>();

		public void AddUndoRedo(Action undo, Action redo)
		{
			Console.WriteLine("done");
			_actions.RemoveAll(a => a.Undone);
			_actions.Add(new UndoRedoAction(undo, redo));
		}

		public void Undo()
		{
			var action = _actions.LastOrDefault(a => !a.Undone);

			if (action == null)
				return;

			Console.WriteLine("undone");

			action.Undo?.Invoke();
			action.Undone = true;
		}

		public void Redo()
		{
			var action = _actions.LastOrDefault(a => a.Undone);

			if (action == null)
				return;

			Console.WriteLine("redone");

			action.Redo?.Invoke();
			action.Undone = false;
		}
	}

	class UndoRedoAction
	{
		public Action Undo;
		public Action Redo;

		public bool Undone;

		public UndoRedoAction(Action undo, Action redo)
		{
			Undo = undo;
			Redo = redo;
		}
	}

	class MusicPlayer : IDisposable
	{
		private WaveStream _music;
		private WaveChannel32 _volumeStream;
		private WaveOutEvent _player;
		private VarispeedSampleProvider _speedControl;

		private readonly Timer _time = new Timer();

		public MusicPlayer()
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

			_speedControl = new VarispeedSampleProvider(reader, 1000, new SoundTouchProfile(true, true));

			Init();

			Reset();
		}

		public void Init() => _player.Init(_speedControl);
		public void Play()
		{
			if (TotalTime == CurrentTime)
			{
				CurrentTime = TimeSpan.Zero;
			}

			_time.Start();
			_player.Play();
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
		}

		public float Speed
		{
			get => _speedControl.PlaybackRate;

			set
			{
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
			get => _volumeStream.Volume;

			set => _volumeStream.Volume = value;
		}

		public void Reset()
		{
			Stop();

			_music.CurrentTime = TimeSpan.Zero;
		}

		public void Update()
		{
			_time.Update(Speed);
		}

		public bool IsPlaying => _player.PlaybackState == PlaybackState.Playing;
		public bool IsPaused => _player.PlaybackState == PlaybackState.Paused;

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

				time = time > _music.TotalTime ? _music.TotalTime : time;

				return time;
			}
			set
			{
				_music.CurrentTime = value;
				_time.Elapsed = value;

				_speedControl.Reposition();
			}
		}

		public double Progress => TotalTime == TimeSpan.Zero ? 0 : Math.Min(1, CurrentTime.TotalMilliseconds / TotalTime.TotalMilliseconds);

		public void Dispose()
		{
			_player.Dispose();
			_speedControl.Dispose();
			_music.Dispose();
			_volumeStream.Dispose();
		}
	}

	class GuiRenderer
	{
		public List<Gui> Guis = new List<Gui>();

		public void Render(float mouseX, float mouseY)
		{
			foreach (var gui in Guis)
			{
				gui.Render(mouseX, mouseY);
			}
		}
	}

	class GuiScreen : Gui
	{
		protected List<GuiButton> Buttons = new List<GuiButton>();

		public bool Pauses { get; }

		protected GuiScreen(float x, float y, float sx, float sy) : base(x, y, sx, sy)
		{
			Pauses = true;
		}

		public override void Render(float mouseX, float mouseY)
		{
			foreach (var button in Buttons)
			{
				button.Render(mouseX, mouseY);
			}
		}

		public virtual void OnMouseMove(float x, float y)
		{

		}

		public virtual void OnMouseClick(float x, float y)
		{
			foreach (var button in Buttons)
			{
				if (button.IsMouseOver)
				{
					OnButtonClicked(button.ID);
					break;
				}
			}
		}

		protected virtual void OnButtonClicked(int id)
		{

		}

		public virtual void OnClosing()
		{

		}
	}

	class GuiScreenMain : GuiScreen
	{
		public double Progress;

		private readonly GuiTempo _tempo = new GuiTempo(512, 64);
		private readonly GuiGrid _grid = new GuiGrid(300, 300);

		public GuiTrack Track = new GuiTrack(0, 64);

		public GuiScreenMain() : base(0, EditorWindow.Instance.ClientSize.Height - 64, EditorWindow.Instance.ClientSize.Width - 512 - 64, 64)
		{
			Buttons.Add(new GuiButtonPlayPause(0, EditorWindow.Instance.ClientSize.Width - 512 - 64, EditorWindow.Instance.ClientSize.Height - 64, 64, 64));
		}

		public override void Render(float mouseX, float mouseY)
		{
			_grid.Render(mouseX, mouseY);
			_tempo.Render(mouseX, mouseY);

			Track.Render(mouseX, mouseY);

			var rect = ClientRectangle;

			var pos = new Vector2(rect.X, rect.Y);
			var size = new Vector2(rect.Width, rect.Height);
			var timelinePos = new Vector2(rect.Height / 2f, rect.Height / 2f - 1);
			var timelineSize = new Vector2(rect.Width - rect.Height, 2);

			//background
			GL.Color3(0.1f, 0.1f, 0.1f);
			GLU.RenderQuad(pos.X, pos.Y, size.X, size.Y);

			//timeline
			GL.Color3(0.5f, 0.5f, 0.5f);
			GLU.RenderQuad(timelinePos.X + pos.X, timelinePos.Y + pos.Y, timelineSize.X, timelineSize.Y);

			var cursorPos = timelineSize.X * Progress;

			//cursor
			GL.Color3(0.75f, 0.75f, 0.75f);
			GLU.RenderQuad(timelinePos.X + cursorPos - 2.5f + pos.X, timelinePos.Y - size.Y * 0.5f / 2 + pos.Y, 5, size.Y * 0.5f);

			base.Render(mouseX, mouseY);
		}

		protected override void OnButtonClicked(int id)
		{
			if (id == 0)
			{
				if (EditorWindow.Instance.MusicPlayer.IsPlaying)
					EditorWindow.Instance.MusicPlayer.Pause();
				else
					EditorWindow.Instance.MusicPlayer.Play();
			}
		}

		public void OnResize(Size size)
		{
			Buttons[0].ClientRectangle = new RectangleF(size.Width - 512 - 64, size.Height - 64, 64, 64);

			ClientRectangle = new RectangleF(0, size.Height - 64, size.Width - 512 - 64, 64);

			Track.OnResize(size);
			_tempo.OnResize(size);
			_grid.ClientRectangle = new RectangleF((int)(size.Width / 2f - _grid.ClientRectangle.Width / 2), (int)((size.Height + Track.ClientRectangle.Height - 64) / 2 - _grid.ClientRectangle.Height / 2), _grid.ClientRectangle.Width, _grid.ClientRectangle.Height);
		}
	}

	class GuiButtonPlayPause : GuiButton
	{
		public GuiButtonPlayPause(int id, float x, float y, float sx, float sy) : base(id, x, y, sx, sy)
		{
			Texture = TextureManager.GetOrRegister("widgets");
		}

		public override void Render(float mouseX, float mouseY)
		{
			var rect = ClientRectangle;
			var b = EditorWindow.Instance.MusicPlayer.IsPlaying;

			float us = b ? 0.5f : 0;

			GL.Color3(0.08f, 0.08f, 0.08f);
			GLU.RenderQuad(rect.X, rect.Y, rect.Width, rect.Height);
			GL.Color3(1, 1, 1f);
			GLU.RenderTexturedQuad(rect.X, rect.Y, rect.Width, rect.Height, us, 0, us + 0.5f, 0.5f, Texture);

			base.Render(mouseX, mouseY);
		}
	}

	class GuiButton : Gui
	{
		public bool IsMouseOver { get; protected set; }
		public int ID;

		protected int Texture;

		protected GuiButton(int id, float x, float y, float sx, float sy) : base(x, y, sx, sy)
		{
			ID = id;
		}

		protected GuiButton(int id, float x, float y, float sx, float sy, int texture) : this(id, x, y, sx, sy)
		{
			Texture = texture;
		}

		public override void Render(float mouseX, float mouseY)
		{
			IsMouseOver = ClientRectangle.Contains(mouseX, mouseY);
		}
	}

	class GuiTextBox : Gui
	{
		protected GuiTextBox(float x, float y, float sx, float sy) : base(x, y, sx, sy)
		{
		}
	}

	class GuiTempo : Gui
	{
		private readonly int _textureId;

		public GuiTempo(float sx, float sy) : base(EditorWindow.Instance.ClientSize.Width - sx, EditorWindow.Instance.ClientSize.Height - sy, sx, sy)
		{
			_textureId = TextureManager.GetOrRegister("tempo");
		}

		public override void Render(float mouseX, float mouseY)
		{
			GL.Color3(1, 1, 1f);
			GLU.RenderTexturedQuad(ClientRectangle, 0, 0, 1, 1, _textureId);

			var tempo = (EditorWindow.Instance.MusicPlayer.Speed - 0.2f) / 0.8f;
			var y = ClientRectangle.Y + 44;

			GL.Color3(0, 0.75f, 1f);
			GLU.RenderQuad(ClientRectangle.X + 32 + tempo * (512 - 64) - 2f, y - 15 + 1, 4, 15);
		}

		public void OnResize(Size size)
		{
			ClientRectangle = new RectangleF(size.Width - ClientRectangle.Width, size.Height - ClientRectangle.Height, ClientRectangle.Width, ClientRectangle.Height);
		}
	}

	class GuiGrid : Gui
	{
		public List<GuiButton> Buttons = new List<GuiButton>();

		public GuiGrid(float sx, float sy) : base(EditorWindow.Instance.ClientSize.Width / 2f - sx / 2, EditorWindow.Instance.ClientSize.Height / 2f - sy / 2, sx, sy)
		{

		}

		public override void Render(float mouseX, float mouseY)
		{
			var rect = ClientRectangle;

			GL.Color3(0.1f, 0.1f, 0.1f);
			GLU.RenderQuad(rect.X, rect.Y, rect.Width, rect.Height);

			var cellSize = rect.Width / 3f;
			var noteSize = cellSize * 0.75f;

			var gap = cellSize - noteSize;

			var audioTime = EditorWindow.Instance.MusicPlayer.CurrentTime.TotalMilliseconds;

			GL.Color3(0.2, 0.2, 0.2f);

			for (int y = 1; y <= 2; y++)
			{
				var ly = y * cellSize;

				GL.Begin(PrimitiveType.Lines);
				GL.Vertex2(rect.X + 0.5f, rect.Y + ly);
				GL.Vertex2(rect.X + rect.Width + 0.5f, rect.Y + ly);
				GL.End();
			}

			for (int x = 1; x <= 2; x++)
			{
				var lx = x * cellSize;

				GL.Begin(PrimitiveType.Lines);
				GL.Vertex2(rect.X + lx + 0.5f, rect.Y);
				GL.Vertex2(rect.X + lx + 0.5f, rect.Y + rect.Height);
				GL.End();
			}

			foreach (var note in EditorWindow.Instance.Notes)
			{
				var visible = audioTime < note.Ms && note.Ms - audioTime <= 750;

				if (!visible)
					continue;

				var x = rect.X + note.X * cellSize + gap / 2;
				var y = rect.Y + note.Y * cellSize + gap / 2;

				var progress = (float)(1 - Math.Min(1, (note.Ms - audioTime) / 750.0));

				var outlineSize = 4 + noteSize + noteSize * (1 - progress) * 2;

				GL.Color4(note.Color.R, note.Color.G, note.Color.B, progress * 0.2f);
				GLU.RenderQuad(x, y, noteSize, noteSize);
				GL.Color4(note.Color.R, note.Color.G, note.Color.B, progress);
				GLU.RenderOutline(x, y, noteSize, noteSize);

				GLU.RenderOutline(x - outlineSize / 2 + noteSize / 2, y - outlineSize / 2 + noteSize / 2, outlineSize, outlineSize);
			}
		}
	}

	class GuiTrack : Gui
	{
		public float ScreenX = 300;

		public Note MouseOverNote;

		public GuiTrack(float y, float sy) : base(0, y, EditorWindow.Instance.ClientSize.Width, sy)
		{

		}

		public override void Render(float mouseX, float mouseY)
		{
			GL.Color3(0.1f, 0.1f, 0.1f);

			GLU.RenderQuad(ClientRectangle);

			var rect = ClientRectangle;

			var cellSize = rect.Height;
			var noteSize = cellSize * 0.65f;

			var gap = cellSize - noteSize;

			var audioTime = EditorWindow.Instance.MusicPlayer.CurrentTime.TotalMilliseconds;

			var cubeStep = EditorWindow.Instance.CubeStep;
			var posX = (float)audioTime / 1000 * cubeStep;
			var maxX = (float)EditorWindow.Instance.MusicPlayer.TotalTime.TotalMilliseconds / 1000 * cubeStep;

			var mouseOver = false;

			//draw start line
			GL.Color4(0f, 1f, 0f, 1);
			GL.Begin(PrimitiveType.Lines);
			GL.Vertex2((int)(ScreenX - posX) + 0.5f, rect.Y);
			GL.Vertex2((int)(ScreenX - posX) + 0.5f, rect.Y + rect.Height + 8);
			GL.End();
			//draw end line
			GL.Color4(1f, 0f, 0f, 1);
			GL.Begin(PrimitiveType.Lines);
			GL.Vertex2((int)(ScreenX - posX + maxX) + 0.5f, rect.Y);
			GL.Vertex2((int)(ScreenX - posX + maxX) + 0.5f, rect.Y + rect.Height + 8);
			GL.End();

			foreach (var note in EditorWindow.Instance.Notes)
			{
				var x = ScreenX - posX + note.Ms / 1000f * cubeStep;

				if (x < rect.X - noteSize || x > rect.Width)
					continue;

				var alphaMult = 1f;

				if (x <= ScreenX)
				{
					alphaMult = 0.25f;
				}

				var y = rect.Y + gap / 2;

				var noteRect = new RectangleF(x, y, noteSize, noteSize);

				if (!mouseOver)
				{
					MouseOverNote = null;
				}

				if (!mouseOver && noteRect.Contains(mouseX, mouseY))
				{
					MouseOverNote = note;
					mouseOver = true;
				}

				GL.Color4(note.Color.R, note.Color.G, note.Color.B, alphaMult * 0.2f);
				GLU.RenderQuad(x, y, noteSize, noteSize);
				GL.Color4(note.Color.R, note.Color.G, note.Color.B, alphaMult * 1f);
				GLU.RenderOutline(x, y, noteSize, noteSize);

				//draw line
				GL.Color4(1f, 1f, 1f, alphaMult);
				GL.Begin(PrimitiveType.Lines);
				GL.Vertex2((int)x + 0.5f, rect.Y + rect.Height - 4);
				GL.Vertex2((int)x + 0.5f, rect.Y + rect.Height + 4);
				GL.End();
			}

			GL.Color3(1f, 1, 1);
			GL.Begin(PrimitiveType.Lines);
			GL.Vertex2(rect.X + ScreenX + 0.5f, rect.Y + 4);
			GL.Vertex2(rect.X + ScreenX + 0.5f, rect.Y + rect.Height - 4);
			GL.End();

			//GL.Color3(1, 1, 1f);
			//FontRenderer.Print("HELLO", 0, rect.Y + rect.Height + 8);
		}

		public void OnResize(Size size)
		{
			ClientRectangle = new RectangleF(0, ClientRectangle.Y, size.Width, ClientRectangle.Height);

			ScreenX = ClientRectangle.Width / 2.5f;
		}
	}

	class Gui
	{
		public RectangleF ClientRectangle;

		protected Gui(float x, float y, float sx, float sy)
		{
			ClientRectangle = new RectangleF(x, y, sx, sy);
		}

		public virtual void Render(float mouseX, float mouseY)
		{

		}
	}

	class ColorSequence
	{
		private readonly Color[] _colors;
		private int _index;

		public ColorSequence()
		{
			_colors = new[] { Color.Red, Color.Cyan };
		}

		public Color Next()
		{
			var color = _colors[_index];

			_index = (_index + 1) % _colors.Length;

			return color;
		}

		public void Reset()
		{
			_index = 0;
		}
	}

	class Note
	{
		public int X;
		public int Y;
		public int Ms;

		public Color Color;

		public Note(int x, int y, int ms)
		{
			X = x;
			Y = y;

			Ms = ms;
		}
	}

	//class Timer
	//{
	//	private double _elapsed;

	//	public bool IsRunning { get; private set; }

	//	public TimeSpan Elapsed
	//	{
	//		get => TimeSpan.FromSeconds(_elapsed);
	//		set => _elapsed = value.TotalSeconds;
	//	}

	//	public void Start() => IsRunning = true;

	//	public void Stop() => IsRunning = false;

	//	public void Reset()
	//	{
	//		IsRunning = false;
	//		_elapsed = 0;
	//	}

	//	public void Update(double delta)
	//	{
	//		if (IsRunning)
	//			_elapsed += delta;
	//	}
	//}

	class Timer
	{
		private readonly Stopwatch _sw = new Stopwatch();

		private double _last;

		private double _elapsed;

		public TimeSpan Elapsed
		{
			get => TimeSpan.FromSeconds(_elapsed);

			set
			{
				if (_sw.IsRunning)
					_sw.Restart();
				else
					_sw.Reset();

				_elapsed = value.TotalSeconds;
				_last = 0;
			}
		}

		public void Start() => _sw.Start();

		public void Stop() => _sw.Stop();

		public void Reset()
		{
			_sw.Reset();

			_elapsed = 0;
			_last = 0;
		}

		public void Update(double speed)
		{
			var elapsed = _sw.Elapsed.TotalSeconds;

			_elapsed += (elapsed - _last) * speed;

			_last = elapsed;
		}
	}

	/*class Timer : Stopwatch
	{
		private TimeSpan _offset = TimeSpan.Zero;

		public new TimeSpan Elapsed
		{
			get => base.Elapsed + _offset;
			set => _offset = value - base.Elapsed;
		}

		public new void Reset()
		{
			base.Reset();

			_offset = TimeSpan.Zero;
		}

		public new void Restart()
		{
			base.Restart();

			_offset = TimeSpan.Zero;
		}
	}*/

	static class GLU
	{
		public static void RenderQuad(double x, double y, double sx, double sy)
		{
			GL.Translate(x, y, 0);
			GL.Scale(sx, sy, 1);
			GL.Begin(PrimitiveType.Quads);
			GL.Vertex2(0, 0);
			GL.Vertex2(0, 1);
			GL.Vertex2(1, 1);
			GL.Vertex2(1, 0);
			GL.End();
			GL.Scale(1f / sx, 1f / sy, 1);
			GL.Translate(-x, -y, 0);
		}

		public static void RenderQuad(RectangleF rect)
		{
			RenderQuad(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static void RenderTexturedQuad(float x, float y, float sx, float sy, float us, float vs, float ue, float ve, int texture)
		{
			GL.BindTexture(TextureTarget.Texture2D, texture);
			GL.Translate(x, y, 0);
			GL.Scale(sx, sy, 1);
			GL.Begin(PrimitiveType.Quads);
			GL.TexCoord2(us, vs);
			GL.Vertex2(0, 0);
			GL.TexCoord2(us, ve);
			GL.Vertex2(0, 1);
			GL.TexCoord2(ue, ve);
			GL.Vertex2(1, 1);
			GL.TexCoord2(ue, vs);
			GL.Vertex2(1, 0);
			GL.End();
			GL.Scale(1f / sx, 1f / sy, 1);
			GL.Translate(-x, -y, 0);
			GL.BindTexture(TextureTarget.Texture2D, 0);
		}

		public static void RenderTexturedQuad(RectangleF rect, float us, float vs, float ue, float ve, int texture)
		{
			RenderTexturedQuad(rect.X, rect.Y, rect.Width, rect.Height, us, vs, ue, ve, texture);
		}

		public static void RenderOutline(float x, float y, float sx, float sy)
		{
			GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);

			GL.Translate(x, y, 0);
			GL.Scale(sx, sy, 1);
			GL.Begin(PrimitiveType.Polygon);
			GL.Vertex2(0, 0);
			GL.Vertex2(0, 1);
			GL.Vertex2(1, 1);
			GL.Vertex2(1, 0);
			GL.End();
			GL.Scale(1 / sx, 1 / sy, 1);
			GL.Translate(-x, -y, 0);

			GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
		}
	}
}
