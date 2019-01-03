using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Blox_Saber_Editor
{
	class GuiGrid : Gui
	{
		public List<GuiButton> Buttons = new List<GuiButton>();

		public Note MouseOverNote;

		public GuiGrid(float sx, float sy) : base(EditorWindow.Instance.ClientSize.Width / 2f - sx / 2, EditorWindow.Instance.ClientSize.Height / 2f - sy / 2, sx, sy)
		{

		}

		public override void Render(float mouseX, float mouseY)
		{
			var rect = ClientRectangle;
			var mouseOver = false;

			GL.Color3(0.1f, 0.1f, 0.1f);
			GLU.RenderQuad(rect.X, rect.Y, rect.Width, rect.Height);

			var cellSize = rect.Width / 3f;
			var noteSize = cellSize * 0.75f;

			var gap = cellSize - noteSize;

			var audioTime = EditorWindow.Instance.MusicPlayer.CurrentTime.TotalMilliseconds;

			GL.Color3(0.2, 0.2, 0.2f);

			for (int y = 1; y <= 2; y++)
			{
				var ly = y * cellSize;

				GL.Begin(PrimitiveType.Lines);
				GL.Vertex2(rect.X + 0.5f, rect.Y + ly);
				GL.Vertex2(rect.X + rect.Width + 0.5f, rect.Y + ly);
				GL.End();
			}

			for (int x = 1; x <= 2; x++)
			{
				var lx = x * cellSize;

				GL.Begin(PrimitiveType.Lines);
				GL.Vertex2(rect.X + lx + 0.5f, rect.Y);
				GL.Vertex2(rect.X + lx + 0.5f, rect.Y + rect.Height);
				GL.End();
			}

			foreach (var note in EditorWindow.Instance.Notes)
			{
				var visible = audioTime < note.Ms && note.Ms - audioTime <= 750;

				if (!visible)
					continue;

				var x = rect.X + note.X * cellSize + gap / 2;
				var y = rect.Y + note.Y * cellSize + gap / 2;

				var progress = (float)(1 - Math.Min(1, (note.Ms - audioTime) / 750.0));

				var outlineSize = 4 + noteSize + noteSize * (1 - progress) * 2;

				var noteRect = new RectangleF(x, y, noteSize, noteSize);
				GL.Color4(note.Color.R, note.Color.G, note.Color.B, progress * 0.2f);
				GLU.RenderQuad(noteRect);
				GL.Color4(note.Color.R, note.Color.G, note.Color.B, progress);
				GLU.RenderOutline(noteRect);

				GLU.RenderOutline(x - outlineSize / 2 + noteSize / 2, y - outlineSize / 2 + noteSize / 2, outlineSize, outlineSize);

				if (!mouseOver)
				{
					MouseOverNote = null;
				}

				if (EditorWindow.Instance.SelectedNote is Note selected && selected == note)
				{
					outlineSize = noteSize + 8;

					GL.Color4(0, 0.5f, 1f, progress);
					GLU.RenderOutline(x - outlineSize / 2 + noteSize / 2, y - outlineSize / 2 + noteSize / 2, outlineSize, outlineSize);
				}

				if (!mouseOver && noteRect.Contains(mouseX, mouseY))
				{
					MouseOverNote = note;
					mouseOver = true;

					GL.Color3(0, 1, 0.25f);
					GLU.RenderOutline(x - 4, y - 4, noteSize + 8, noteSize + 8);
				}
			}
		}
	}
}