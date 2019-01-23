using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using OpenTK.Graphics.OpenGL;

namespace Blox_Saber_Editor
{
	class GuiVolume : Gui
	{
		public GuiVolume(float sx, float sy) : base(EditorWindow.Instance.ClientSize.Width - sx, EditorWindow.Instance.ClientSize.Height - sy - 64, sx, sy)
		{
		}

		public override void Render(float delta, float mouseX, float mouseY)
		{
			var rect = ClientRectangle;
			var maxY = rect.Height - rect.Width;
			var volume = EditorWindow.Instance.MusicPlayer.Volume;
			var pos = (1 - EditorWindow.Instance.MusicPlayer.Volume) * maxY;
			var y = rect.Y + rect.Width / 2;

			GL.Color3(0.1f, 0.1f, 0.1f);
			GLU.RenderQuad(rect);
			GL.Color3(0.35f, 0.35f, 0.35f);
			GLU.RenderOutline(rect);

			GL.Color3(0.5f, 0.5f, 0.5f);
			GLU.RenderQuad(rect.X + rect.Width / 2 - 1, y, 2, maxY);
			GL.Color3(1f, 1, 1);
			GLU.RenderQuad(rect.X + rect.Width / 2 - 10, y + pos - 2.5f, 20, 5);

			GL.Color3(0, 0.75f, 1);
			var fr = EditorWindow.Instance.FontRenderer;

			var text = "MASTER";

			var w = fr.GetWidth(text, 14);
			var h = fr.GetHeight(14);

			fr.Render(text, (int)(rect.X + rect.Width / 2 - w / 2f), (int)(rect.Y + h - 3), 14);
		}

		public override void OnResize(Size size)
		{
			ClientRectangle = new RectangleF(size.Width - ClientRectangle.Size.Width, size.Height - ClientRectangle.Size.Height - 64, ClientRectangle.Size.Width, ClientRectangle.Size.Height);
		}
	}
}