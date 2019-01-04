using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Blox_Saber_Editor
{
	class GuiScreenLoadCreate : GuiScreen
	{
		private readonly GuiButton _createButton;
		private readonly GuiButton _loadButton;

		private readonly int _textureId;

		public GuiScreenLoadCreate() : base(0, 0, EditorWindow.Instance.ClientSize.Width, EditorWindow.Instance.ClientSize.Height)
		{
			using (var img = Properties.Resources.BloxSaber)
			{
				_textureId = TextureManager.GetOrRegister("logo", img);
			}

			_createButton = new GuiButton(0, 0, 0, 192, 64, "CREATE MAP");
			_loadButton = new GuiButton(0, 0, 0, 192, 64, "LOAD MAP");

			Buttons.Add(_createButton);
			Buttons.Add(_loadButton);
		}

		public override void Render(float delta, float mouseX, float mouseY)
		{
			var rect = ClientRectangle;

			GL.Color3(1, 1, 1f);
			GLU.RenderTexturedQuad(rect.X + rect.Width / 2 - 256, 0, 512, 512, 0, 0, 1, 1, _textureId);

			base.Render(delta, mouseX, mouseY);
		}

		public override void OnResize(Size size)
		{
			ClientRectangle = new RectangleF(0, 0, size.Width, size.Height);

			_createButton.ClientRectangle = new RectangleF((int)(size.Width / 2f - _createButton.ClientRectangle.Width - 20), 512, _createButton.ClientRectangle.Width, _createButton.ClientRectangle.Height);
			_loadButton.ClientRectangle = new RectangleF((int)(size.Width / 2f + 20), 512, _loadButton.ClientRectangle.Width, _loadButton.ClientRectangle.Height);
		}
	}
}
