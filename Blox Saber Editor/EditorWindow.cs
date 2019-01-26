using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Blox_Saber_Editor.Gui;
using Blox_Saber_Editor.Properties;
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

		public readonly Dictionary<Key, Tuple<int, int>> KeyMapping = new Dictionary<Key, Tuple<int, int>>();

		//private readonly GuiScreenEditor _screenEditor;

		private WindowState _lastWindowState;

		public readonly UndoRedo UndoRedo = new UndoRedo();

		public bool IsDraggingNoteOnTimeLine => _draggingNoteTimeline && _draggedNotes.Count > 0 && _draggedNotes[0].DragStartMs != _draggedNotes[0].Ms;
		public List<Note> SelectedNotes = new List<Note>();
		private List<Note> _draggedNotes = new List<Note>();
		private Note _draggedNote;
		private Note _lastPlayedNote;

		private DateTime _lastTempoChange = DateTime.Now;

		private Point _clickedMouse;
		private Point _lastMouse;

		private float _brigthness;

		private int _dragStartX;
		private long _dragStartMs;
		private long _dragNoteStartMs;
		private long _dragNoteDragStartMs;

		private int _dragStartIndexX;
		private int _dragStartIndexY;

		private bool _saved;
		private bool _rightDown;
		private bool _controlDown;
		//private bool _draggingCursor;
		private bool _draggingNoteTimeline;
		private bool _draggingNoteGrid;
		private bool _draggingTimeline;

		private bool _wasPlaying;

		private string _file;

		private long _soundID = -1;

		public NoteList Notes = new NoteList();

		private float _zoom = 1;

		public float Zoom
		{
			get => _zoom;
			set => _zoom = Math.Max(1, Math.Min(4, value));
		}

		public float CubeStep => 50 * 10 * Zoom;

		public EditorWindow() : base(800, 600, new GraphicsMode(32, 8, 0, 4), "Blox Saber Map Editor Beta")
		{
			Instance = this;

			Icon = Resources.Blox_Saber;
			VSync = VSyncMode.Off;

			_lastWindowState = WindowState;

			TargetRenderFrequency = 240;

			MusicPlayer = new MusicPlayer { Volume = 0.25f };
			SoundPlayer = new SoundPlayer();

			FontRenderer = new FontRenderer("main");

			OpenGuiScreen(new GuiScreenLoadCreate());

			SoundPlayer.Init();

			SoundPlayer.Cache("hit");
			SoundPlayer.Cache("click");

			KeyMapping.Add(Key.Q, new Tuple<int, int>(0, 0));
			KeyMapping.Add(Key.W, new Tuple<int, int>(1, 0));
			KeyMapping.Add(Key.E, new Tuple<int, int>(2, 0));

			KeyMapping.Add(Key.A, new Tuple<int, int>(0, 1));
			KeyMapping.Add(Key.S, new Tuple<int, int>(1, 1));
			KeyMapping.Add(Key.D, new Tuple<int, int>(2, 1));

			KeyMapping.Add(Key.Z, new Tuple<int, int>(0, 2));
			KeyMapping.Add(Key.X, new Tuple<int, int>(1, 2));
			KeyMapping.Add(Key.C, new Tuple<int, int>(2, 2));
		}

		protected override void OnLoad(EventArgs e)
		{
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
			GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);

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
			{
				gse.Timeline.Progress = (float)MusicPlayer.Progress;

				if (MusicPlayer.IsPlaying)
				{
					var closest = Notes.LastOrDefault(n => n.Ms <= (int)MusicPlayer.CurrentTime.TotalMilliseconds);

					if (_lastPlayedNote != closest)
					{
						_lastPlayedNote = closest;

						if (closest != null)
						{
							SoundPlayer.Play("hit", gse.SFXVolume.Value / (float)gse.SFXVolume.MaxValue);
							_brigthness = 0.2f;
						}
					}
				}
			}

			GL.ClearColor(_brigthness, 0, _brigthness, 1);
			_brigthness = (float)Math.Max(0, _brigthness - e.Time);

			GuiScreen?.Render((float)e.Time, _lastMouse.X, _lastMouse.Y);

			if (_draggingNoteTimeline && GuiScreen is GuiScreenEditor editor)
			{
				var rect = editor.Track.ClientRectangle;

				foreach (var draggedNote in _draggedNotes)
				{
					var posX = (float)MusicPlayer.CurrentTime.TotalSeconds * CubeStep;
					var noteX = editor.Track.ScreenX - posX + draggedNote.DragStartMs / 1000f * CubeStep;

					GL.Color3(0.75f, 0.75f, 0.75f);
					GLU.RenderQuad((int)noteX, (int)rect.Y, 1, rect.Height);
				}
			}

			if (_rightDown && GuiScreen is GuiScreenEditor g)
			{
				if (g.Track.ClientRectangle.Contains(_clickedMouse))
				{
					var x = Math.Min(_lastMouse.X, _clickedMouse.X);
					var y = Math.Min(_lastMouse.Y, _clickedMouse.Y);

					var w = Math.Max(_lastMouse.X, _clickedMouse.X) - x;
					var h = Math.Min((int)g.Track.ClientRectangle.Height, Math.Max(_lastMouse.Y, _clickedMouse.Y)) - y;

					GL.Color4(0, 1, 0.2f, 0.2f);
					GLU.RenderQuad(x, y, w, h);
					GL.Color4(0, 1, 0.2f, 1);
					GLU.RenderOutline(x, y, w, h);
				}
			}

			GL.PopMatrix();
			SwapBuffers();
		}

		protected override void OnResize(EventArgs e)
		{
			if (ClientSize.Width < 800 || ClientSize.Height < 600)
			{
				ClientSize = new Size(800, 600);
			}

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

			if (_rightDown && GuiScreen is GuiScreenEditor g)
			{
				var x = Math.Min(_lastMouse.X, _clickedMouse.X);
				var y = Math.Min(_lastMouse.Y, _clickedMouse.Y);

				var w = Math.Max(_lastMouse.X, _clickedMouse.X) - x;
				var h = Math.Min((int)g.Track.ClientRectangle.Height, Math.Max(_lastMouse.Y, _clickedMouse.Y)) - y;

				var rect = new Rectangle(x, y, w, h);

				var list = g.Track.GetNotesInRect(rect);

				SelectedNotes = list;
				_draggedNotes = new List<Note>(list);
			}

			if (GuiScreen is GuiScreenEditor editor)
			{
				if (_draggingNoteTimeline)
				{
					OnDraggingTimelineNotes(Math.Abs(e.X - _dragStartX) >= 5 ? e.X : _dragStartX);
				}

				if (editor.Timeline.Dragging)
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
				if (editor.MasterVolume.Dragging)
				{
					var rect = editor.MasterVolume.ClientRectangle;
					var lineSize = rect.Height - rect.Width;
					var step = lineSize / editor.MasterVolume.MaxValue;

					var tick = MathHelper.Clamp(Math.Round((lineSize - (e.Y - rect.Y - rect.Width / 2)) / step), 0, editor.MasterVolume.MaxValue);

					editor.MasterVolume.Value = (int)tick;

					MusicPlayer.Volume = Math.Max(0,
						Math.Min(1, (float)tick / editor.MasterVolume.MaxValue));
				}
				if (editor.SFXVolume.Dragging)
				{
					var rect = editor.SFXVolume.ClientRectangle;
					var lineSize = rect.Height - rect.Width;
					var step = lineSize / editor.SFXVolume.MaxValue;

					var tick = MathHelper.Clamp(Math.Round((lineSize - (e.Y - rect.Y - rect.Width / 2)) / step), 0, editor.SFXVolume.MaxValue);

					editor.SFXVolume.Value = (int)tick;
				}
				if (editor.BeatSnapDivisor.Dragging)
				{
					var rect = editor.BeatSnapDivisor.ClientRectangle;
					var step = (rect.Width - rect.Height) / editor.BeatSnapDivisor.MaxValue;

					var tick = (int)MathHelper.Clamp(Math.Round((e.X - rect.X - rect.Height / 2) / step), 0, editor.BeatSnapDivisor.MaxValue);

					editor.BeatSnapDivisor.Value = tick;
					editor.Track.BeatDivisor = tick + 1;
				}
				if (editor.Tempo.Dragging)
				{
					var rect = editor.Tempo.ClientRectangle;
					var step = (rect.Width - rect.Height) / editor.Tempo.MaxValue;

					var tick = (int)MathHelper.Clamp(Math.Round((e.X - rect.X - rect.Height / 2) / step), 0, editor.Tempo.MaxValue);

					editor.Tempo.Value = tick;

					var tempo = 0.2f + tick * 0.1f;

					TrySetTempo(tempo);
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

		protected override void OnMouseLeave(EventArgs e)
		{
			_rightDown = false;
		}

		protected override void OnFocusedChanged(EventArgs e)
		{
			if (!Focused)
			{
				OnMouseLeave(null);
				OnMouseUp(new MouseButtonEventArgs(_lastMouse.X, _lastMouse.Y, MouseButton.Left, false));
			}
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			_clickedMouse = e.Position;

			if (e.Button == MouseButton.Right)
				_rightDown = true;

			GuiScreen?.OnMouseClick(e.X, e.Y);

			if (GuiScreen is GuiScreenEditor editor && !_rightDown)
			{
				if (editor.Track.MouseOverNote is Note tn)
				{
					MusicPlayer.Pause();

					_draggingNoteTimeline = true;

					_dragStartX = e.X;
					tn.DragStartMs = tn.Ms;

					_draggedNote = tn;

					if (!_draggedNotes.Contains(tn))
					{
						if (_draggedNotes.Count == 1 || !SelectedNotes.Contains(tn))
							_draggedNotes.Clear();

						_draggedNotes.Add(tn);
					}

					if (!SelectedNotes.Contains(tn))
					{
						SelectedNotes.Clear();

						SelectedNotes.Add(tn);
					}

					foreach (var note in _draggedNotes)
					{
						note.DragStartMs = note.Ms;
					}

					_dragNoteStartMs = tn.Ms;
				}
				else if (editor.Grid.MouseOverNote is Note gn)
				{
					MusicPlayer.Pause();

					_draggingNoteGrid = true;

					_dragStartIndexX = gn.X;
					_dragStartIndexY = gn.Y;

					if (!_draggedNotes.Contains(gn))
					{
						if (_draggedNotes.Count == 1 || !SelectedNotes.Contains(gn))
							_draggedNotes.Clear();

						_draggedNotes.Add(gn);
					}

					if (!SelectedNotes.Contains(gn))
					{
						SelectedNotes.Clear();

						SelectedNotes.Add(gn);
					}
				}
				else if (editor.Track.ClientRectangle.Contains(e.Position))
				{
					_wasPlaying = MusicPlayer.IsPlaying;

					MusicPlayer.Pause();

					_draggingTimeline = true;
					_dragStartX = e.X;
					_dragStartMs = (int)MusicPlayer.CurrentTime.TotalMilliseconds;

				}
				else if (editor.MasterVolume.ClientRectangle.Contains(e.Position))
				{
					editor.MasterVolume.Dragging = true;
					OnMouseMove(new MouseMoveEventArgs(e.X, e.Y, 0, 0));
				}
				else if (editor.SFXVolume.ClientRectangle.Contains(e.Position))
				{
					editor.SFXVolume.Dragging = true;
					OnMouseMove(new MouseMoveEventArgs(e.X, e.Y, 0, 0));
				}
				else if (editor.BeatSnapDivisor.ClientRectangle.Contains(e.Position))
				{
					editor.BeatSnapDivisor.Dragging = true;
					OnMouseMove(new MouseMoveEventArgs(e.X, e.Y, 0, 0));
				}
				else if (editor.Tempo.ClientRectangle.Contains(e.Position))
				{
					editor.Tempo.Dragging = true;
					OnMouseMove(new MouseMoveEventArgs(e.X, e.Y, 0, 0));
				}
				else
				{
					SelectedNotes.Clear();
				}

				if (editor.ClientRectangle.Contains(e.Position))
				{
					MusicPlayer.Pause();
					editor.Timeline.Dragging = true;

					OnMouseMove(new MouseMoveEventArgs(e.X, e.Y, 0, 0));
				}
			}
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (_draggingNoteTimeline)
			{
				MusicPlayer.Pause();

				_lastPlayedNote = Notes.LastOrDefault(n =>
					n.Ms <= Math.Floor(MusicPlayer.CurrentTime.TotalMilliseconds));

				if (_draggedNotes.Count > 0)
				{
					var notes = new List<Note>(_draggedNotes);

					var note = notes[0];
					var start = note.DragStartMs;
					var diff = note.Ms - start;

					if (diff != 0)
					{
						Notes.Sort();

						long[] startMs = new long[notes.Count];

						for (var i = 0; i < startMs.Length; i++)
						{
							startMs[i] = notes[i].DragStartMs;
						}

						var saveState = _saved;

						UndoRedo.AddUndoRedo("MOVE NOTE" + (_draggedNotes.Count > 1 ? "S" : ""), () =>
						{
							for (var index = 0; index < notes.Count; index++)
							{
								var note1 = notes[index];

								start = startMs[index];
								note1.Ms = start;
							}

							Notes.Sort();

							_saved = saveState;
						}, () =>
						{
							for (var index = 0; index < notes.Count; index++)
							{
								var note1 = notes[index];

								start = startMs[index];
								note1.Ms = start + diff;
							}

							Notes.Sort();

							_saved = false;
						});

						_saved = false;
					}
				}
			}

			if (_draggingNoteGrid)
			{
				MusicPlayer.Pause();
				OnDraggingGridNote(_lastMouse);

				var note = _draggedNotes[0];
				var startX = _dragStartIndexX;
				var startY = _dragStartIndexY;
				var newX = note.X;
				var newY = note.Y;

				if (note.X != _dragStartIndexX || note.Y != _dragStartIndexY)
				{
					var saveState = _saved;

					UndoRedo.AddUndoRedo("REPOSITION NOTE", () =>
					 {
						 note.X = startX;
						 note.Y = startY;

						 _saved = saveState;
					 }, () =>
					 {
						 note.X = newX;
						 note.Y = newY;

						 _saved = false;
					 });

					_saved = false;
				}
			}

			if (_draggingTimeline)
			{
				MusicPlayer.Stop();
				OnDraggingTimeline(e.X);

				_lastPlayedNote = Notes.LastOrDefault(n =>
					n.Ms <= Math.Floor(MusicPlayer.CurrentTime.TotalMilliseconds));

				if (_wasPlaying)
					MusicPlayer.Play();
			}

			if (e.Button == MouseButton.Right)
				_rightDown = false;

			if (GuiScreen is GuiScreenEditor gui)
			{
				if (gui.MasterVolume.Dragging || gui.SFXVolume.Dragging)
				{
					Settings.Default.MasterVolume = (decimal)gui.MasterVolume.Value / gui.MasterVolume.MaxValue;
					Settings.Default.SFXVolume = (decimal)gui.SFXVolume.Value / gui.SFXVolume.MaxValue;

					Settings.Default.Save();
				}

				gui.BeatSnapDivisor.Dragging = false;
				gui.MasterVolume.Dragging = false;
				gui.SFXVolume.Dragging = false;
				gui.Timeline.Dragging = false;
				gui.Tempo.Dragging = false;
			}

			_draggingNoteTimeline = false;
			_draggingNoteGrid = false;
			_draggingTimeline = false;
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			GuiScreen.OnKeyTyped(e.KeyChar);
		}

		protected override void OnKeyUp(KeyboardKeyEventArgs e)
		{
			_controlDown = e.Control || Keyboard.GetState().IsKeyDown(Key.ControlLeft) ||
									   Keyboard.GetState().IsKeyDown(Key.LControl);
		}

		protected override void OnKeyDown(KeyboardKeyEventArgs e)
		{
			_controlDown = e.Control || Keyboard.GetState().IsKeyDown(Key.ControlLeft) ||
						   Keyboard.GetState().IsKeyDown(Key.LControl);

			if (e.Key == Key.F11)
			{
				if (WindowState != WindowState.Fullscreen)
				{
					_lastWindowState = WindowState;
					WindowState = WindowState.Fullscreen;
				}
				else if (WindowState == WindowState.Fullscreen)
				{
					WindowState = _lastWindowState;
				}

				return;
			}

			GuiScreen.OnKeyDown(e.Key, e.Control);

			if (GuiScreen is GuiScreen gs && !gs.AllowInput())
				return;

			if (e.Key == Key.A && e.Control)
			{
				Notes.Sort();

				SelectedNotes = Notes.ToList();
				_draggedNotes = Notes.ToList();

				return;
			}

			if (GuiScreen is GuiScreenEditor editor)
			{
				if (e.Key == Key.S && e.Control)
				{
					if (e.Shift || !_saved && _file == null)
					{
						var wasPlaying = MusicPlayer.IsPlaying;

						MusicPlayer.Pause();

						if (PromptSave())
						{
							_saved = true;

							editor.ShowToast("SAVED", Color.Chartreuse);
						}

						if (wasPlaying)
							MusicPlayer.Play();
					}
					else
					{
						if (WriteFile(_file))
						{
							_saved = true;

							editor.ShowToast("SAVED", Color.Chartreuse);
						}
					}

					return;
				}

				if (e.Control)
				{
					if (e.Key == Key.Z)
					{
						if (UndoRedo.CanUndo)
						{
							MusicPlayer.Pause();

							UndoRedo.Undo();
						}
					}
					else if (e.Key == Key.Y)
					{
						if (UndoRedo.CanRedo)
						{
							MusicPlayer.Pause();

							UndoRedo.Redo();
						}
					}
				}

				//make sure to not register input while we're typing into a text box

				if (!MusicPlayer.IsPlaying && SelectedNotes.Count > 0 && _draggingNoteTimeline)
				{
					if (e.Key == Key.Left)
					{
						_saved = false;
						foreach (var node in SelectedNotes)
						{
							node.Ms--;
						}

						Notes.Sort();
					}
					if (e.Key == Key.Right)
					{
						_saved = false;
						foreach (var node in SelectedNotes)
						{
							node.Ms++;
						}

						Notes.Sort();
					}
				}

				if (e.Key == Key.Space)
				{
					if (!_draggingTimeline && !_draggingNoteTimeline && !editor.Timeline.Dragging)
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
					if (KeyMapping.TryGetValue(e.Key, out var tuple))
					{
						var note = new Note(tuple.Item1, tuple.Item2,
							(int)MusicPlayer.CurrentTime.TotalMilliseconds);

						Notes.Add(note);

						var saveState = _saved;
						UndoRedo.AddUndoRedo("ADD NOTE", () =>
						{
							Notes.Remove(note);

							_saved = saveState;
						}, () =>
						{
							Notes.Add(note);
							_saved = false;
						});

						_saved = false;
					}

					if (e.Key == Key.Delete && SelectedNotes.Count > 0)
					{
						var result = MessageBox.Show($"Are you sure you want to delete {(SelectedNotes.Count > 1 ? "these notes" : "this note")}?", $"Delete {(SelectedNotes.Count > 1 ? "notes" : "note")}?",
							MessageBoxButtons.YesNo, MessageBoxIcon.Question);

						if (result == DialogResult.Yes)
						{
							var toRemove = new List<Note>(SelectedNotes);

							Notes.RemoveAll(toRemove);

							var saveState = _saved;
							UndoRedo.AddUndoRedo("DELETE NOTE" + (toRemove.Count > 1 ? "S" : ""), () =>
							{
								Notes.AddAll(toRemove);

								_saved = saveState;
							}, () =>
							{
								Notes.RemoveAll(toRemove);

								_saved = false;
							});

							_saved = false;

							SelectedNotes.Clear();
						}
					}
				}
			}
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			var dist = e.DeltaPrecise / 10;

			//if (GuiScreen is GuiScreenEditor editor)
			//{
				if (_controlDown)
					Zoom += dist;
				else
				{
					MusicPlayer.Pause();
					var time = MusicPlayer.CurrentTime.TotalSeconds;
					MusicPlayer.Stop();

					time = Math.Max(0, Math.Min(MusicPlayer.TotalTime.TotalMilliseconds, time + dist / Zoom * 0.5));

					MusicPlayer.CurrentTime = TimeSpan.FromSeconds(time);
				}
			//}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = !WillClose();

			if (!e.Cancel)
			{
				MusicPlayer.Dispose();
				SoundPlayer.Dispose();
			}
		}

		private bool TrySetTempo(float tempo)
		{
			if ((DateTime.Now - _lastTempoChange).TotalMilliseconds >= 35)
			{
				_lastTempoChange = DateTime.Now;
				var newSpeed = Math.Max(0.2f, Math.Min(1, tempo));

				if (newSpeed != MusicPlayer.Speed)
				{
					MusicPlayer.Speed = newSpeed;
					return true;
				}
			}

			return false;
		}

		public bool WillClose()
		{
			if (!_saved && _soundID != -1)
			{
				var wasPlaying = MusicPlayer.IsPlaying;

				MusicPlayer.Pause();

				var r = MessageBox.Show("You have unsaved progress.\nDo you wish to save it before closing?", "WillClose", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

				if (r == DialogResult.Yes)
					PromptSave();

				if (r == DialogResult.Cancel)
				{
					if (wasPlaying)
					{
						MusicPlayer.Play();
					}

					return false;
				}
			}

			return true;
		}

		private long GetClosestBeat(Note note)
		{
			var lastDiffMs = long.MaxValue;
			var closestMs = long.MaxValue;

			List<long> beats = new List<long>();

			if (GuiScreen is GuiScreenEditor gui)
			{
				var rect = gui.Track.ClientRectangle;

				var audioTime = MusicPlayer.CurrentTime.TotalMilliseconds;
				var posX = (float)audioTime / 1000 * CubeStep;

				var screenX = gui.Track.ScreenX;

				var bpm = gui.Track.BPM;
				var bpmOffset = gui.Track.BPMOffset;
				var beatDivisor = gui.Track.BeatDivisor;

				var lineSpace = 60 / bpm * CubeStep;
				var stepSmall = lineSpace / beatDivisor;

				var lineX = screenX - posX + bpmOffset / 1000f * CubeStep;
				if (lineX < 0)
					lineX %= lineSpace;


				while (lineSpace > 0 && lineX < rect.Width)
				{
					//bpm line
					var timelineMs = (long)((lineX - screenX + posX) / CubeStep * 1000);

					if (timelineMs != long.MaxValue && timelineMs != long.MinValue)
						beats.Add(timelineMs);

					for (int j = 1; j <= beatDivisor; j++)
					{
						var xo = lineX + j * stepSmall;

						if (j < beatDivisor)
						{
							//divided bpm line
							timelineMs = (long)((xo - screenX + posX) / CubeStep * 1000);

							if (timelineMs != long.MaxValue && timelineMs != long.MinValue)
								beats.Add(timelineMs);
						}
					}

					lineX += lineSpace;
				}
			}

			foreach (var lineMs in beats)
			{
				var diffMs = Math.Abs(lineMs - note.Ms);

				if (diffMs < lastDiffMs)
				{
					lastDiffMs = diffMs;

					closestMs = lineMs;
				}
			}

			return closestMs;
		}

		private void OnDraggingTimelineNotes(int mouseX)
		{
			var pixels = mouseX - _dragStartX;
			var msDiff = pixels / CubeStep * 1000;

			var audioTime = MusicPlayer.CurrentTime.TotalMilliseconds;

			if (GuiScreen is GuiScreenEditor gui && _draggedNote != null)
			{
				var clickMs = (int)(Math.Max(0, _clickedMouse.X - gui.Track.ScreenX + (float)audioTime / 1000 * CubeStep) / CubeStep * 1000);
				var clickOff = clickMs - _dragNoteStartMs;
				var cursorMs = (int)(Math.Max(0, mouseX - gui.Track.ScreenX + (float)audioTime / 1000 * CubeStep) / CubeStep * 1000) - clickOff;

				if (_draggedNotes.Count > 0)
				{
					var snappedMs = GetClosestBeat(_draggedNote);

					if (Math.Abs(snappedMs - cursorMs) / 1000f * CubeStep <= 8) //8 pixels
						msDiff = -(_draggedNote.DragStartMs - snappedMs);
				}

				foreach (var note in _draggedNotes)
				{
					var time = note.DragStartMs + (int)msDiff;

					time = (int)Math.Max(0, Math.Min(MusicPlayer.TotalTime.TotalMilliseconds, time));

					note.Ms = time;
				}
				/*
				if (_draggedNotes.Count > 0)
				{
					var snappedMs = GetClosestBeat(_draggedNote);
	
					if (Math.Abs(snappedMs - _draggedNote.Ms) / 1000f * CubeStep <= 5)
					{
						var diff = snappedMs - _draggedNote.Ms;
	
						//_draggedNote.Ms = snappedMs;
	
						foreach (var note in _draggedNotes)
						{
							note.Ms = MathHelper.Clamp(note.Ms + diff, 0, totalTime);
						}
						
						//Console.WriteLine("snap");
					}
				}*/

				Notes.Sort();
			}
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

				_draggedNotes[0].X = newX;
				_draggedNotes[0].Y = newY;
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

		public void LoadFile(string file)
		{
			var data = File.ReadAllText(file);

			if (LoadMap(data))
			{
				_file = file;
				_saved = true;
			}
		}

		public bool LoadMap(string data)
		{
			Notes.Clear();

			SelectedNotes.Clear();
			_draggedNotes.Clear();

			_draggedNote = null;
			_lastPlayedNote = null;

			var splits = Regex.Matches(data, "([^,]+)");

			try
			{
				var ID = splits[0];

				for (int i = 1; i < splits.Count; i++)
				{
					var chunk = splits[i];

					var chunkSplit = Regex.Matches(chunk.Value, "([^|]+)");

					var x = 2 - int.Parse(chunkSplit[0].Value);
					var y = 2 - int.Parse(chunkSplit[1].Value);
					var ms = long.Parse(chunkSplit[2].Value);

					Notes.Add(new Note(x, y, ms));
				}
				if (long.TryParse(ID.Value, out _soundID) && LoadSound(_soundID))
				{
					MusicPlayer.Load("assets/cached/" + _soundID + ".asset");
					OpenGuiScreen(new GuiScreenEditor());
				}
				else
					_soundID = -1;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace);
				return false;
			}

			return _soundID != -1;
		}

		public void CreateMap(long id)
		{
			LoadMap(id.ToString());
		}

		private bool PromptSave()
		{
			SaveFileDialog sfd = new SaveFileDialog
			{
				Title = "Save map",
				Filter = "Text Documents (*.txt)|*.txt"
			};

			if (_file != null)
			{
				sfd.InitialDirectory = Path.GetDirectoryName(_file);

				sfd.FileName = Path.GetFileNameWithoutExtension(_file);
			}

			var result = sfd.ShowDialog();

			if (result == DialogResult.OK)
			{
				WriteFile(sfd.FileName);

				_file = sfd.FileName;

				return true;
			}

			return false;
		}

		private bool WriteFile(string file)
		{
			if (file == null)
				return false;

			try
			{
				var sb = new StringBuilder();

				sb.Append(_soundID.ToString());

				for (int i = 0; i < Notes.Count; i++)
				{
					Note note = Notes[i];

					var gridX = 2 - note.X;
					var gridY = 2 - note.Y;

					sb.Append($",{gridX}|{gridY}|{note.Ms}");
				}

				File.WriteAllText(file, sb.ToString(), Encoding.UTF8);
			}
			catch { return false; }

			return true;
		}

		private bool LoadSound(long id)
		{
			try
			{
				if (!Directory.Exists("assets/cached"))
					Directory.CreateDirectory("assets/cached");

				if (!File.Exists("assets/cached/" + id + ".asset"))
				{
					using (var wc = new SecureWebClient())
					{
						wc.DownloadFile("https://assetgame.roblox.com/asset/?id=" + id, "assets/cached/" + id + ".asset");
					}
				}

				return true;
			}
			catch (Exception e)
			{
				MessageBox.Show($"Failed to download asset with id '{id}':\n\n{e.Message}", "Error", MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}

			return false;
		}

		public void OpenGuiScreen(GuiScreen s)
		{
			GuiScreen?.OnClosing();

			IsPaused = s != null && s.Pauses;

			GuiScreen = s;
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

	class NoteList
	{
		private List<Note> _notes = new List<Note>();

		public int Count => _notes.Count;

		public void Add(Note note)
		{
			_notes.Add(note);

			Sort();
		}

		public void Remove(Note note)
		{
			_notes.Remove(note);

			Sort();
		}

		public void Clear()
		{
			_notes.Clear();
		}

		public void Sort()
		{
			_notes = new List<Note>(_notes.OrderBy(n => n.Ms));
		}

		public Note LastOrDefault(Func<Note, bool> predicate)
		{
			return _notes.LastOrDefault(predicate);
		}

		public void RemoveAll(List<Note> notes)
		{
			foreach (var note in notes)
			{
				_notes.Remove(note);
			}

			Sort();
		}

		public void AddAll(List<Note> notes)
		{
			_notes.AddRange(notes);

			Sort();
		}

		public Note this[int index]
		{
			get => _notes[index];
			set => _notes[index] = value;
		}

		public List<Note> ToList()
		{
			return new List<Note>(_notes);
		}
	}
}