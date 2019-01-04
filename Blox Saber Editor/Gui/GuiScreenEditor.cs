using System.Drawing;
using System.Security.AccessControl;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Blox_Saber_Editor
{
	class GuiScreenEditor : GuiScreen
	{
		public double Progress;

		public readonly GuiGrid Grid = new GuiGrid(300, 300);
		public readonly GuiTrack Track = new GuiTrack(0, 64);
		public readonly GuiTempo Tempo = new GuiTempo(512, 64);
		public readonly GuiVolume Volume = new GuiVolume(64, 256);

		public readonly GuiTextBox TextBox = new GuiTextBox(0, 0, 256, 64);

		public GuiScreenEditor() : base(0, EditorWindow.Instance.ClientSize.Height - 64, EditorWindow.Instance.ClientSize.Width - 512 - 64, 64)
		{
			Buttons.Add(new GuiButtonPlayPause(0, EditorWindow.Instance.ClientSize.Width - 512 - 64, EditorWindow.Instance.ClientSize.Height - 64, 64, 64));
		}

		public override void Render(float delta, float mouseX, float mouseY)
		{
			Grid.Render(delta, mouseX, mouseY);

			Track.Render(delta, mouseX, mouseY);
			Tempo.Render(delta, mouseX, mouseY);
			Volume.Render(delta, mouseX, mouseY);
			//TextBox.Render(delta, mouseX, mouseY);

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
			GL.Color3(1f, 1, 1);
			GLU.RenderQuad(timelinePos.X + cursorPos - 2.5f + pos.X, timelinePos.Y - size.Y * 0.5f / 2 + pos.Y, 5, size.Y * 0.5f);

			base.Render(delta, mouseX, mouseY);
		}

		public void OnKeyTyped(char key)
		{
			TextBox.OnKeyTyped(key);
		}

		public void OnKeyDown(Key key, bool control)
		{
			TextBox.OnKeyDown(key, control);
		}

		public override void OnMouseClick(float x, float y)
		{
			TextBox.OnMouseClick(x, y);

			base.OnMouseClick(x, y);
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

		public override void OnResize(Size size)
		{
			Buttons[0].ClientRectangle = new RectangleF(size.Width - 512 - 64, size.Height - 64, 64, 64);

			ClientRectangle = new RectangleF(0, size.Height - 64, size.Width - 512 - 64, 64);

			TextBox.OnResize(size);
			Track.OnResize(size);
			Tempo.OnResize(size);
			Volume.OnResize(size);
			Grid.ClientRectangle = new RectangleF((int)(size.Width / 2f - Grid.ClientRectangle.Width / 2), (int)((size.Height + Track.ClientRectangle.Height - 64) / 2 - Grid.ClientRectangle.Height / 2), Grid.ClientRectangle.Width, Grid.ClientRectangle.Height);
		}
	}
}