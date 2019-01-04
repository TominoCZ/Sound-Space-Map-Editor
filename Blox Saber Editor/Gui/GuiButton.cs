using System;
using OpenTK.Graphics.OpenGL;

namespace Blox_Saber_Editor
{
	class GuiButton : Gui
	{
		public bool IsMouseOver { get; protected set; }
		public int ID;
		public string Text = "";

		protected int Texture;

		public GuiButton(int id, float x, float y, float sx, float sy) : base(x, y, sx, sy)
		{
			ID = id;
		}

		public GuiButton(int id, float x, float y, float sx, float sy, int texture) : this(id, x, y, sx, sy)
		{
			Texture = texture;
		}

		public GuiButton(int id, float x, float y, float sx, float sy, string text) : this(id, x, y, sx, sy)
		{
			Text = text;
		}

		public override void Render(float delta, float mouseX, float mouseY)
		{
			IsMouseOver = ClientRectangle.Contains(mouseX, mouseY);
			
			if (IsMouseOver)
				GL.Color3(0.75f, 0.75f, 0.75f);
			else
				GL.Color3(1f, 1, 1);

			GLU.RenderQuad(ClientRectangle);

			if (Texture > 0)
			{
				GLU.RenderTexturedQuad(ClientRectangle, 0, 0, 1, 1, Texture);
			}

			var fr = EditorWindow.Instance.FontRenderer;
			var width = fr.GetWidth(Text, 24);
			var height = fr.GetHeight(24);

			GL.Color3(0, 0, 0);
			fr.Render(Text, (int)(ClientRectangle.X + ClientRectangle.Width / 2 - width / 2f), (int)(ClientRectangle.Y + ClientRectangle.Height / 2 - height / 2f), 24);
		}
	}
}