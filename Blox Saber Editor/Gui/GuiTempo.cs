using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Blox_Saber_Editor
{
	class GuiTempo : Gui
	{
		private readonly int _textureId;

		public GuiTempo(float sx, float sy) : base(EditorWindow.Instance.ClientSize.Width - sx, EditorWindow.Instance.ClientSize.Height - sy, sx, sy)
		{
			_textureId = TextureManager.GetOrRegister("tempo");
		}

		public override void Render(float delta, float mouseX, float mouseY)
		{
			GL.Color3(1, 1, 1f);
			GLU.RenderTexturedQuad(ClientRectangle, 0, 0, 1, 1, _textureId);

			var tempo = (EditorWindow.Instance.MusicPlayer.Speed - 0.2f) / 0.8f;
			var y = ClientRectangle.Y + 44;

			GL.Color3(1, 0, 0.5f);
			GLU.RenderQuad(ClientRectangle.X + 32 + tempo * (512 - 64) - 2f, y - 15 + 1, 4, 15);
		}

		public override void OnResize(Size size)
		{
			ClientRectangle = new RectangleF(size.Width - ClientRectangle.Width, size.Height - ClientRectangle.Height, ClientRectangle.Width, ClientRectangle.Height);
		}
	}
}