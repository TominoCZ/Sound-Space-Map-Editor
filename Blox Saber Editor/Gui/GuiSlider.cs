using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Blox_Saber_Editor.Gui
{
	class GuiSlider : Gui
	{
		public int MaxValue = 7;

		public int Value = 0;

		public float Progress;

		public bool Dragging;

		public bool Snap = true;

		private readonly bool _vertical;

		private float _alpha;

		public GuiSlider(float x, float y, float sx, float sy) : base(x, y, sx, sy)
		{
			_vertical = sx < sy;
		}

		public override void Render(float delta, float mouseX, float mouseY)
		{
			var rect = ClientRectangle;
			var lineSize = _vertical ? rect.Height - rect.Width : rect.Width - rect.Height;

			var step = lineSize / MaxValue;
			var pos = Snap ? step * Value : Progress * lineSize;
			var lineRect = new RectangleF(rect.X + (_vertical ? rect.Width / 2 - 1 : rect.Height / 2), rect.Y + (_vertical ? rect.Width / 2 : rect.Height / 2 - 1), _vertical ? 2 : lineSize, _vertical ? lineSize : 2);
			var cursorPos = new PointF(rect.X + (_vertical ? rect.Width / 2 : rect.Height / 2 + pos), _vertical ? rect.Bottom - rect.Width / 2 - pos : rect.Y + rect.Height / 2);

			var mouseClose = Dragging || Math.Sqrt(Math.Pow(mouseX - cursorPos.X, 2) + Math.Pow(mouseY - cursorPos.Y, 2)) <= 12;

			_alpha = MathHelper.Clamp(_alpha + (mouseClose ? 10 : -10) * delta, 0, 1);

			GL.Color4(0, 1f, 1f, 0.75f);
			Glu.RenderQuad(lineRect);
			GL.Color4(0, 1f, 1f, 1f);
			Glu.RenderOutline(lineRect);

			//GL.LineWidth(2);
			//cursor
			GL.Translate((int)cursorPos.X, (int)cursorPos.Y, 0);
			GL.Rotate(_alpha * 90, 0, 0, 1);

			GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
			GL.Color4(1f, 0, 0.25f, 0.2f * _alpha);
			RenderCircle(0, 0, 12 * _alpha);

			GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
			GL.Color4(1f, 0, 0.25f, 1f * _alpha);
			RenderCircle(0, 0, 12 * _alpha);

			GL.Rotate(-_alpha * 90, 0, 0, 1);
			GL.Translate(-(int)cursorPos.X, -(int)cursorPos.Y, 0);

			GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
			GL.Color4(1f, 0, 0.25f, 1f);
			RenderCircle((int)cursorPos.X, (int)cursorPos.Y, 4);
			//GL.LineWidth(1);
		}

		private void RenderCircle(float x, float y, float r)
		{
			GL.Begin(PrimitiveType.Polygon);

			for (int i = 0; i < 6; i++)
			{
				var a = i / 6f * MathHelper.TwoPi;

				var cx = Math.Cos(a) * r;
				var cy = -Math.Sin(a) * r;

				GL.Vertex2(x + cx, y + cy);
			}

			GL.End();
		}

		public override void OnResize(Size size)
		{
			ClientRectangle = new RectangleF(size.Width - ClientRectangle.Size.Width, size.Height - ClientRectangle.Size.Height - 64, ClientRectangle.Size.Width, ClientRectangle.Size.Height);
		}
	}
}