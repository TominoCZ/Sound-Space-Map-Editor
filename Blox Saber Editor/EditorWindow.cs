using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Blox_Saber_Editor.Properties;
using FreeType;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using KeyPressEventArgs = OpenTK.KeyPressEventArgs;

namespace Blox_Saber_Editor
{
	class EditorWindow : GameWindow
	{
		public static EditorWindow Instance;
		public FontRenderer FontRenderer;
		public bool IsPaused { get; private set; }

		public GuiScreen GuiScreen;

		public MusicPlayer MusicPlayer;
		public SoundPlayer SoundPlayer;

		private readonly Dictionary<Key, Tuple<int, int>> _mapping = new Dictionary<Key, Tuple<int, int>>();

		//private readonly GuiScreenEditor _screenEditor;

		private readonly UndoRedo _undoRedo = new UndoRedo();

		private Note _draggedNote;
		private Note _lastPlayedNote;
		public Note SelectedNote;

		private DateTime _lastTempoChange = DateTime.Now;

		private Point _lastMouse;

		private float _brigthness;

		private int _dragStartX;
		private int _dragStartMs;

		private int _dragStartIndexX;
		private int _dragStartIndexY;

		private bool _mouseDown;
		private bool _draggingCursor;
		private bool _draggingNoteTimeline;
		private bool _draggingNoteGrid;
		private bool _draggingTimeline;
		private bool _draggingVolume;

		private bool _wasPlaying;

		private long _soundID = -1;

		public List<Note> Notes = new List<Note>();

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

			Icon = Resources.Blox_Saber;
			VSync = VSyncMode.Off;

			TargetRenderFrequency = 240;

			MusicPlayer = new MusicPlayer { Volume = 0.25f };
			SoundPlayer = new SoundPlayer();

			FontRenderer = new FontRenderer("main");
			//_screenEditor = new GuiScreenEditor();

			OpenGuiScreen(new GuiScreenEditor()); //_screenEditor);
												  //OpenGuiScreen(new GuiScreenLoadCreate());

			SoundPlayer.Init();

			LoadMap("map.txt");

			MusicPlayer.Load("song.mp3");
			MusicPlayer.Play();

			SoundPlayer.Cache("hit");

			_mapping.Add(Key.Q, new Tuple<int, int>(0, 0));
			_mapping.Add(Key.W, new Tuple<int, int>(1, 0));
			_mapping.Add(Key.E, new Tuple<int, int>(2, 0));

			_mapping.Add(Key.A, new Tuple<int, int>(0, 1));
			_mapping.Add(Key.S, new Tuple<int, int>(1, 1));
			_mapping.Add(Key.D, new Tuple<int, int>(2, 1));

			_mapping.Add(Key.Z, new Tuple<int, int>(0, 2));
			_mapping.Add(Key.X, new Tuple<int, int>(1, 2));
			_mapping.Add(Key.C, new Tuple<int, int>(2, 2));
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
			//MusicPlayer.Update();
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			//MusicPlayer.Update();

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.PushMatrix();

			if (GuiScreen is GuiScreenEditor gse)
				gse.Progress = MusicPlayer.Progress;

			if (MusicPlayer.IsPlaying)
			{
				var closest = Notes.OrderBy(n => n.Ms)
					.LastOrDefault(n => n.Ms <= (int)MusicPlayer.CurrentTime.TotalMilliseconds);

				if (_lastPlayedNote != closest)
				{
					_lastPlayedNote = closest;

					if (closest != null)
					{
						SoundPlayer.Play("hit");
						_brigthness = 0.2f;
					}
				}
			}

			GL.ClearColor(_brigthness, 0, _brigthness, 1);
			_brigthness = (float)Math.Max(0, _brigthness - e.Time);

			GuiScreen?.Render((float)e.Time, _lastMouse.X, _lastMouse.Y);

			if (_draggingNoteTimeline && GuiScreen is GuiScreenEditor editor)
			{
				var rect = editor.Track.ClientRectangle;
				var posX = (float)MusicPlayer.CurrentTime.TotalSeconds * CubeStep;
				var noteX = editor.Track.ScreenX - posX + _dragStartMs / 1000f * CubeStep;

				GL.Color3(0.75f, 0.75f, 0.75f);
				GLU.RenderQuad(noteX, rect.Y, 1, rect.Height);

				if (_lastMouse.X != _dragStartX)
				{
					noteX = editor.Track.ScreenX - posX + _draggedNote.Ms / 1000f * CubeStep;

					GLU.RenderQuad(noteX, rect.Y, 1, rect.Height);
				}
			}

			GL.PopMatrix();
			SwapBuffers();
		}

		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(ClientRectangle);

			GL.MatrixMode(MatrixMode.Projection);
			var m = Matrix4.CreateOrthographicOffCenter(0, Width, Height, 0, 0, 1);
			GL.LoadMatrix(ref m);

			GuiScreen?.OnResize(ClientSize);

			OnRenderFrame(new FrameEventArgs());
		}

		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			_lastMouse = e.Position;

			GuiScreen?.OnMouseMove(e.X, e.Y);

			if (GuiScreen is GuiScreenEditor editor)
			{
				if (_draggingCursor)
				{
					var wasPlaying = MusicPlayer.IsPlaying;

					MusicPlayer.Pause();

					var progress = (e.X - editor.ClientRectangle.Height / 2f) /
								   (editor.ClientRectangle.Width - editor.ClientRectangle.Height);

					progress = Math.Max(0, Math.Min(1, progress));

					MusicPlayer.Stop();
					MusicPlayer.CurrentTime =
						TimeSpan.FromMilliseconds(MusicPlayer.TotalTime.TotalMilliseconds * progress);

					MusicPlayer.Pause();

					if (wasPlaying)
					{
						MusicPlayer.Play();
					}
				}

				if (_draggingNoteTimeline)
				{
					OnDraggingTimelineNote(Math.Abs(e.X - _dragStartX) >= 5 ? e.X : _dragStartX);
				}

				if (_draggingVolume)
				{
					var rect = editor.Volume.ClientRectangle;

					MusicPlayer.Volume = Math.Max(0,
						Math.Min(1, 1 - (e.Y - rect.Y - rect.Width / 2) / (rect.Height - rect.Width)));
				}

				if (_draggingNoteGrid)
				{
					OnDraggingGridNote(e.Position);
				}

				if (_draggingTimeline)
				{
					OnDraggingTimeline(e.X);
				}
			}
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			_mouseDown = true;

			GuiScreen?.OnMouseClick(e.X, e.Y);

			SelectedNote = null;

			if (GuiScreen is GuiScreenEditor editor)
			{
				if (editor.Track.MouseOverNote is Note tn)
				{
					MusicPlayer.Pause();

					_draggingNoteTimeline = true;

					_dragStartX = e.X;
					_dragStartMs = tn.Ms;

					_draggedNote = tn;

					SelectedNote = tn;

					//_noteDragStartMs = note.Ms;
				}
				else if (editor.Grid.MouseOverNote is Note gn)
				{
					MusicPlayer.Pause();

					_draggingNoteGrid = true;

					_dragStartIndexX = gn.X;
					_dragStartIndexY = gn.Y;

					_draggedNote = gn;

					SelectedNote = gn;
				}
				else if (editor.Track.ClientRectangle.Contains(e.Position))
				{
					_wasPlaying = MusicPlayer.IsPlaying;

					MusicPlayer.Pause();

					_draggingTimeline = true;
					_dragStartX = e.X;
					_dragStartMs = (int)MusicPlayer.CurrentTime.TotalMilliseconds;
				}
				else if (editor.Volume.ClientRectangle.Contains(e.Position))
				{
					_draggingVolume = true;
				}

				if (editor.ClientRectangle.Contains(e.Position))
				{
					MusicPlayer.Pause();
					_draggingCursor = true;

					OnMouseMove(new MouseMoveEventArgs(e.X, e.Y, 0, 0));
				}
			}
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (_draggingNoteTimeline)
			{
				MusicPlayer.Pause();
				OnDraggingTimelineNote(Math.Abs(e.X - _dragStartX) >= 5 ? e.X : _dragStartX);

				_lastPlayedNote = Notes.OrderBy(n => n.Ms).LastOrDefault(n =>
					n.Ms <= Math.Floor(MusicPlayer.CurrentTime.TotalMilliseconds));

				var note = _draggedNote;
				var start = _dragStartMs;
				var diff = note.Ms - start;

				if (diff != 0)
				{
					_undoRedo.AddUndoRedo(() => { note.Ms = start; }, () => { note.Ms = start + diff; });
				}
			}

			if (_draggingNoteGrid)
			{
				MusicPlayer.Pause();
				OnDraggingGridNote(_lastMouse);

				var note = _draggedNote;
				var startX = _dragStartIndexX;
				var startY = _dragStartIndexY;
				var newX = note.X;
				var newY = note.Y;

				if (note.X != _dragStartIndexX || note.Y != _dragStartIndexY)
				{
					_undoRedo.AddUndoRedo(() =>
					{
						note.X = startX;
						note.Y = startY;
					}, () =>
					{
						note.X = newX;
						note.Y = newY;
					});
				}
			}

			if (_draggingTimeline)
			{
				MusicPlayer.Stop();
				OnDraggingTimeline(e.X);

				_lastPlayedNote = Notes.OrderBy(n => n.Ms).LastOrDefault(n =>
					n.Ms <= Math.Floor(MusicPlayer.CurrentTime.TotalMilliseconds));

				if (_wasPlaying)
					MusicPlayer.Play();
			}

			_draggingVolume = false;
			_draggingNoteTimeline = false;
			_draggingNoteGrid = false;
			_draggingTimeline = false;
			_draggingCursor = false;
			_mouseDown = false;
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if (GuiScreen is GuiScreenEditor editor)
			{
				editor.OnKeyTyped(e.KeyChar);
			}
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

			if (GuiScreen is GuiScreenEditor editor)
			{
				editor.OnKeyDown(e.Key, e.Control);

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

				//make sure to not register input while we're typing into a text box
				if (!editor.TextBox.Focused)
				{
					if (e.Key == Key.Space)
					{
						if (!_draggingTimeline && !_draggingNoteTimeline && !_draggingCursor)
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

					if (!e.Control)
					{
						if (_mapping.TryGetValue(e.Key, out var tuple))
						{
							var note = new Note(tuple.Item1, tuple.Item2,
								(int)MusicPlayer.CurrentTime.TotalMilliseconds);

							Notes.Add(note);
						}

						if (e.Key == Key.Delete && SelectedNote != null)
						{
							var result = MessageBox.Show("Are you sure you want to delete this note?", "Delete note?",
								MessageBoxButtons.YesNo, MessageBoxIcon.Question);

							if (result == DialogResult.Yes)
							{
								Notes.Remove(SelectedNote);

								SelectedNote = null;
							}
						}
					}
				}
			}
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			var dist = e.DeltaPrecise / 10;

			if (GuiScreen is GuiScreenEditor editor)
			{
				if (editor.Tempo.ClientRectangle.Contains(e.Position))
				{
					if ((DateTime.Now - _lastTempoChange).TotalMilliseconds >= 30)
					{
						_lastTempoChange = DateTime.Now;
						var newSpeed = Math.Max(0.2f, Math.Min(1, MusicPlayer.Speed + e.Delta * 0.1f));

						if (newSpeed != MusicPlayer.Speed)
							MusicPlayer.Speed = newSpeed;
					}
				}
				else if (editor.Volume.ClientRectangle.Contains(e.Position))
				{
					MusicPlayer.Volume = Math.Max(0, Math.Min(1, MusicPlayer.Volume + e.Delta * 0.1f));
				}
				else
				{
					var state = Keyboard.GetState();
					state = Keyboard.GetState();
					if (state.IsKeyDown(Key.LControl) || state.IsKeyDown(Key.ControlLeft))
						Zoom += dist;
					else
					{
						MusicPlayer.Pause();
						var time = MusicPlayer.CurrentTime.TotalSeconds;
						MusicPlayer.Stop();

						time = Math.Max(0, Math.Min(MusicPlayer.TotalTime.TotalMilliseconds, time + dist / Zoom * 0.5));

						MusicPlayer.CurrentTime = TimeSpan.FromSeconds(time);
					}
				}
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			MusicPlayer.Dispose();
			SoundPlayer.Dispose();

			SaveMap();
		}

		private void OnDraggingTimelineNote(int mouseX)
		{
			var pixels = mouseX - _dragStartX;
			var msDiff = pixels / CubeStep * 1000;

			var time = _dragStartMs + (int)msDiff;

			time = (int)Math.Max(0, Math.Min(MusicPlayer.TotalTime.TotalMilliseconds, time));

			_draggedNote.Ms = time;
		}

		private void OnDraggingGridNote(Point pos)
		{
			if (GuiScreen is GuiScreenEditor editor)
			{
				var rect = editor.Grid.ClientRectangle;
				var newX = (int)Math.Floor((pos.X - rect.X) / rect.Width * 3);
				var newY = (int)Math.Floor((pos.Y - rect.Y) / rect.Height * 3);

				if (newX < 0 || newX > 2 || newY < 0 || newY > 2)
				{
					return;
				}

				_draggedNote.X = newX;
				_draggedNote.Y = newY;
			}
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

				Notes.Add(new Note(x, y, ms));
			}

			_soundID = long.Parse(ID.Value);
		}

		private void SaveMap()
		{
			SaveFileDialog sfd = new SaveFileDialog
			{
				Filter = ".TXT|*.TXT"
			};

			var result = sfd.ShowDialog();

			if (result == DialogResult.OK)
			{
				using (var fs = File.OpenWrite(sfd.FileName))
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
}