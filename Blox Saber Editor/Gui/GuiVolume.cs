using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Blox_Saber_Editor
{
	class GuiVolume : Gui
	{
		public GuiVolume(float sx, float sy) : base(EditorWindow.Instance.ClientSize.Width - sx, EditorWindow.Instance.ClientSize.Height - sy - 64, sx, sy)
		{
		}

		public override void Render(float mouseX, float mouseY)
		{
			var rect = ClientRectangle;
			var maxY = rect.Height - rect.Width;
			var pos = (1 - EditorWindow.Instance.MusicPlayer.Volume) * maxY;
			var y = rect.Y + rect.Width / 2;

			GL.Color3(0.15f, 0.15f, 0.15f);
			GLU.RenderQuad(rect);

			GL.Color3(0.5f, 0.5f, 0.5f);
			GLU.RenderQuad(rect.X + rect.Width / 2 - 1, y, 2, maxY);

			GL.Color3(0.75f, 0.75f, 0.75f);
			GLU.RenderQuad(rect.X + rect.Width / 2 - 10, y + pos - 2.5f, 20, 5);
		}

		public void OnResize(Size size)
		{
			ClientRectangle = new RectangleF(size.Width - ClientRectangle.Size.Width, size.Height - ClientRectangle.Size.Height - 64, ClientRectangle.Size.Width, ClientRectangle.Size.Height);
		}
	}
}