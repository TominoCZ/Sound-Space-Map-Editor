using System;
using OpenTK.Graphics.OpenGL;

namespace Blox_Saber_Editor.Gui
{
	class GuiCheckBox : GuiButton
	{
		public bool Toggle { get; private set; }

		private float _alpha;

		public GuiCheckBox(int id, float x, float y, float sx, float sy, bool toggle) : base(id, x, y, sx, sy)
		{
			ID = id;

			Toggle = toggle;
		}

		public GuiCheckBox(int id, string text, float x, float y, float sx, float sy, bool toggle) : this(id, x, y, sx, sy, toggle)
		{
			Text = text;
		}

		public override void Render(float delta, float mouseX, float mouseY)
		{
			var rect = ClientRectangle;

			IsMouseOver = rect.Contains(mouseX, mouseY);

			GL.Color3(0.05f, 0.05f, 0.05f);
			GLU.RenderQuad(rect);
			GL.Color3(0.2f, 0.2f, 0.2f);
			GLU.RenderOutline(rect);

			_alpha = Toggle ? Math.Min(1, _alpha + delta * 8) : Math.Max(0, _alpha - delta * 8);

			var checkSize = rect.Height * 0.75f * _alpha;
			var gap = (rect.Height - checkSize) / 2;

			if (checkSize > 0)
			{
				GL.Color4(0, 1f, 0.1f, Math.Pow(_alpha, 4));
				GLU.RenderQuad(rect.X + gap, rect.Y + gap, checkSize, checkSize);
			}

			var fr = EditorWindow.Instance.FontRenderer;
			//var width = fr.GetWidth(Text, 24);
			var height = fr.GetHeight(32);

			GL.Color4(1f, 1, 1, 1);
			fr.Render(Text, (int)(rect.Right + rect.Height / 4), (int)(rect.Y + rect.Height / 2 - height / 2f), 32);
		}

		public override void OnMouseClick(float x, float y)
		{
			Toggle = !Toggle;
		}
	}
}