using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Blox_Saber_Editor
{
	class GuiScreenMain : GuiScreen
	{
		public double Progress;

		public readonly GuiGrid Grid = new GuiGrid(300, 300);
		public readonly GuiTrack Track = new GuiTrack(0, 64);
		public readonly GuiTempo Tempo = new GuiTempo(512, 64);
		public readonly GuiVolume Volume = new GuiVolume(64, 256);

		public GuiScreenMain() : base(0, EditorWindow.Instance.ClientSize.Height - 64, EditorWindow.Instance.ClientSize.Width - 512 - 64, 64)
		{
			Buttons.Add(new GuiButtonPlayPause(0, EditorWindow.Instance.ClientSize.Width - 512 - 64, EditorWindow.Instance.ClientSize.Height - 64, 64, 64));
		}

		public override void Render(float mouseX, float mouseY)
		{
			Grid.Render(mouseX, mouseY);

			Track.Render(mouseX, mouseY);
			Tempo.Render(mouseX, mouseY);
			Volume.Render(mouseX, mouseY);

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
			Tempo.OnResize(size);
			Volume.OnResize(size);
			Grid.ClientRectangle = new RectangleF((int)(size.Width / 2f - Grid.ClientRectangle.Width / 2), (int)((size.Height + Track.ClientRectangle.Height - 64) / 2 - Grid.ClientRectangle.Height / 2), Grid.ClientRectangle.Width, Grid.ClientRectangle.Height);
		}
	}
}