using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;

namespace Blox_Saber_Editor
{
	class GuiScreenLoadCreate : GuiScreen
	{
		private readonly GuiButton _createButton;
		private readonly GuiButton _loadButton;
		private readonly GuiButton _pasteButton;

		private readonly int _textureId;

		public GuiScreenLoadCreate() : base(0, 0, EditorWindow.Instance.ClientSize.Width, EditorWindow.Instance.ClientSize.Height)
		{
			using (var img = Properties.Resources.BloxSaber)
			{
				_textureId = TextureManager.GetOrRegister("logo", img);
			}

			_createButton = new GuiButton(0, 0, 0, 192, 64, "CREATE MAP");
			_loadButton = new GuiButton(1, 0, 0, 192, 64, "LOAD MAP");
			_pasteButton = new GuiButton(2, 0, 0, 192, 64, "PASTE MAP");

			OnResize(EditorWindow.Instance.ClientSize);

			Buttons.Add(_createButton);
			Buttons.Add(_loadButton);
			Buttons.Add(_pasteButton);
		}

		public override void Render(float delta, float mouseX, float mouseY)
		{
			var rect = ClientRectangle;

			GL.Color3(1, 1, 1f);
			GLU.RenderTexturedQuad(rect.X + rect.Width / 2 - 256, 0, 512, 512, 0, 0, 1, 1, _textureId);

			base.Render(delta, mouseX, mouseY);
		}

		protected override void OnButtonClicked(int id)
		{
			switch (id)
			{
				case 0:
					EditorWindow.Instance.OpenGuiScreen(new GuiScreenCreate());
					break;
				case 1:
					var ofd = new OpenFileDialog
					{
						Title = "Load map",
						Filter = "Text Documents (*.txt)|*.txt"
					};

					var result = ofd.ShowDialog();

					if (result == DialogResult.OK)
					{
						EditorWindow.Instance.LoadFile(ofd.FileName);
					}
					break;
				case 2:
					var clipboard = Clipboard.GetText();

					if (!string.IsNullOrWhiteSpace(clipboard))
					{
						EditorWindow.Instance.LoadMap(clipboard);
					}
					break;
			}
		}

		public override void OnResize(Size size)
		{
			ClientRectangle = new RectangleF(0, 0, size.Width, size.Height);

			_createButton.ClientRectangle.Location =
				new PointF((int)(size.Width / 2f - _createButton.ClientRectangle.Width - 10), 512);
			_loadButton.ClientRectangle.Location = new PointF((int)(size.Width / 2f + 10), 512);
			_pasteButton.ClientRectangle.Location = new PointF((int)(size.Width / 2f - 192 / 2f), 512 - 64 - 20);
		}
	}
}
