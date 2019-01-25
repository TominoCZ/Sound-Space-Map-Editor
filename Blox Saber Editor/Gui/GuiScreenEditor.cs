using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Blox_Saber_Editor.Gui
{
	class GuiScreenEditor : GuiScreen
	{
		public double Progress;

		public readonly GuiGrid Grid = new GuiGrid(300, 300);
		public readonly GuiTrack Track = new GuiTrack(0, 64);
		public readonly GuiTempo Tempo = new GuiTempo(512, 64);
		public readonly GuiVolume Volume = new GuiVolume(64, 256);
		public readonly GuiTextBox BPM;
		public readonly GuiTextBox Offset;
		public readonly GuiCheckBox Reposition;

		private GuiLabel _toast;
		private float _toastTime;

		public GuiScreenEditor() : base(0, EditorWindow.Instance.ClientSize.Height - 64, EditorWindow.Instance.ClientSize.Width - 512 - 64, 64)
		{
			_toast = new GuiLabel(0, 0, "") { Centered = true, FontSize = 36 };

			var playPause = new GuiButtonPlayPause(0, EditorWindow.Instance.ClientSize.Width - 512 - 64,
				EditorWindow.Instance.ClientSize.Height - 64, 64, 64);
			Reposition = new GuiCheckBox(1, "Reposition", 10, 200, 64, 64, false);
			BPM = new GuiTextBox(10, 126, 64, 32) { Text = "0", Centered = true, Numeric = true };
			Offset = new GuiTextBox(10, BPM.ClientRectangle.Bottom + 5, 64, 32) { Text = "0", Centered = true, Numeric = true };
			var setOffset = new GuiButton(2, Offset.ClientRectangle.Right + 5, Offset.ClientRectangle.Y, 64, 32, "SET");

			BPM.Focused = true;
			Offset.Focused = true;
			BPM.OnKeyDown(Key.Right, false);
			Offset.OnKeyDown(Key.Right, false);
			BPM.Focused = false;
			Offset.Focused = false;

			Buttons.Add(playPause);
			Buttons.Add(Reposition);
			Buttons.Add(setOffset);

			OnResize(EditorWindow.Instance.ClientSize);
		}

		public override void Render(float delta, float mouseX, float mouseY)
		{
			_toastTime = Math.Min(2, _toastTime + delta);

			var toastOffY = 1f;

			if (_toastTime <= 0.5)
			{
				toastOffY = (float)Math.Sin(Math.Min(0.5, _toastTime) / 0.5 * MathHelper.PiOver2);
			}
			else if (_toastTime >= 1.75)
			{
				toastOffY = (float)Math.Cos(Math.Min(0.25, _toastTime - 1.75) / 0.25 * MathHelper.PiOver2);
			}

			var size = EditorWindow.Instance.ClientSize;

			var h = EditorWindow.Instance.FontRenderer.GetHeight(_toast.FontSize);

			_toast.ClientRectangle.Y = size.Height - Tempo.ClientRectangle.Height + h - toastOffY * h * 2;
			//_toast.ClientRectangle.Y = 200;
			Grid.Render(delta, mouseX, mouseY);

			_toast.Render(delta, mouseX, mouseY);

			Track.Render(delta, mouseX, mouseY);
			Tempo.Render(delta, mouseX, mouseY);
			Volume.Render(delta, mouseX, mouseY);
			BPM.Render(delta, mouseX, mouseY);
			Offset.Render(delta, mouseX, mouseY);

			var rect = ClientRectangle;

			var timelinePos = new Vector2(rect.Height / 2f, rect.Height / 2f - 1);
			var timelineSize = new Vector2(rect.Width - rect.Height, 2);

			//background
			GL.Color3(0.1f, 0.1f, 0.1f);
			GLU.RenderQuad((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

			//timeline
			GL.Color3(0.5f, 0.5f, 0.5f);
			GLU.RenderQuad(timelinePos.X + rect.X, timelinePos.Y + rect.Y, timelineSize.X, timelineSize.Y);

			var cursorPos = timelineSize.X * Progress;

			//cursor
			GL.Color3(1f, 1, 1);
			GLU.RenderQuad(timelinePos.X + cursorPos - 2.5f + rect.X, timelinePos.Y - rect.Height * 0.5f / 2 + rect.Y, 5, rect.Height * 0.5f);

			base.Render(delta, mouseX, mouseY);
		}

		public override bool AllowInput()
		{
			return !BPM.Focused && !Offset.Focused;
		}

		public override void OnKeyTyped(char key)
		{
			BPM.OnKeyTyped(key);
			Offset.OnKeyTyped(key);

			UpdateTrack();
		}

		public override void OnKeyDown(Key key, bool control)
		{
			BPM.OnKeyDown(key, control);
			Offset.OnKeyDown(key, control);

			UpdateTrack();
		}

		public override void OnMouseClick(float x, float y)
		{
			BPM.OnMouseClick(x, y);
			Offset.OnMouseClick(x, y);

			base.OnMouseClick(x, y);
		}

		protected override void OnButtonClicked(int id)
		{
			switch (id)
			{
				case 0:
					if (EditorWindow.Instance.MusicPlayer.IsPlaying)
						EditorWindow.Instance.MusicPlayer.Pause();
					else
						EditorWindow.Instance.MusicPlayer.Play();
					break;
				case 2:
					long oldOffset = Track.BPMOffset;
					long.TryParse(Offset.Text, out var newOffset);

					var toggle = Reposition.Toggle;

					void Redo()
					{
						if (toggle)
						{
							var list = EditorWindow.Instance.Notes.ToList();

							foreach (var note in list)
							{
								note.Ms += newOffset - oldOffset;
							}
						}

						Track.BPMOffset = newOffset;
					}

					Redo();

					EditorWindow.Instance.UndoRedo.AddUndoRedo(() =>
					{
						if (toggle)
						{
							var list = EditorWindow.Instance.Notes.ToList();

							foreach (var note in list)
							{
								note.Ms -= newOffset - oldOffset;
							}
						}

						Track.BPMOffset = oldOffset;
					}, Redo);

					break;
			}
		}

		public override void OnResize(Size size)
		{
			Buttons[0].ClientRectangle = new RectangleF(size.Width - 512 - 64, size.Height - 64, 64, 64);

			ClientRectangle = new RectangleF(0, size.Height - 64, size.Width - 512 - 64, 64);

			Track.OnResize(size);
			Tempo.OnResize(size);
			Volume.OnResize(size);

			Grid.ClientRectangle = new RectangleF((int)(size.Width / 2f - Grid.ClientRectangle.Width / 2), (int)((size.Height + Track.ClientRectangle.Height - 64) / 2 - Grid.ClientRectangle.Height / 2), Grid.ClientRectangle.Width, Grid.ClientRectangle.Height);

			_toast.ClientRectangle.X = size.Width / 2f;
		}

		private void UpdateTrack()
		{
			if (BPM.Focused)
			{
				long.TryParse(BPM.Text, out var bpm);

				Track.BPM = MathHelper.Clamp(bpm, 0, 400);

				if (Track.BPM > 0)
					BPM.Text = Track.BPM.ToString();
			}
			if (Offset.Focused)
			{
				long.TryParse(Offset.Text, out var offset);

				offset = Math.Max(0, offset);

				if (offset > 0)
					Offset.Text = offset.ToString();
			}
		}

		public void ShowToast(string text, Color color)
		{
			_toastTime = 0;

			_toast.Text = text;
			_toast.Color = color;
		}
	}
}