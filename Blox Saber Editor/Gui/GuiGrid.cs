using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Blox_Saber_Editor.Gui
{
	class GuiGrid : Gui
	{
		public Note MouseOverNote;

		public GuiGrid(float sx, float sy) : base(EditorWindow.Instance.ClientSize.Width / 2f - sx / 2, EditorWindow.Instance.ClientSize.Height / 2f - sy / 2, sx, sy)
		{

		}

		public override void Render(float delta, float mouseX, float mouseY)
		{
			var editor = (GuiScreenEditor) EditorWindow.Instance.GuiScreen;

			var rect = ClientRectangle;
			var mouseOver = false;

			GL.Color3(0.1f, 0.1f, 0.1f);
			Glu.RenderQuad(rect.X, rect.Y, rect.Width, rect.Height);

			var cellSize = rect.Width / 3f;
			var noteSize = cellSize * 0.75f;

			var gap = cellSize - noteSize;

			var audioTime = EditorWindow.Instance.MusicPlayer.CurrentTime.TotalMilliseconds;

			GL.Color3(0.2, 0.2, 0.2f);

			for (int y = 0; y <= 3; y++)
			{
				var ly = y * cellSize;

				Glu.RenderQuad((int)(rect.X), (int)(rect.Y + ly), rect.Width + 1, 1);
			}

			for (int x = 0; x <= 3; x++)
			{
				var lx = x * cellSize;

				Glu.RenderQuad((int)(rect.X + lx), (int)(rect.Y), 1, rect.Height + 1);
			}

			var fr = EditorWindow.Instance.FontRenderer;

			GL.Color3(0.2f, 0.2f, 0.2f);
			foreach (var pair in EditorWindow.Instance.KeyMapping)
			{
				var letter = pair.Key.ToString();
				var tuple = pair.Value;

				var x = rect.X + tuple.Item1 * cellSize + cellSize / 2;
				var y = rect.Y + tuple.Item2 * cellSize + cellSize / 2;

				var width = fr.GetWidth(letter, 38);
				var height = fr.GetHeight(38);

				fr.Render(letter, (int)(x - width / 2f), (int)(y - height / 2), 38);
			}

			for (var index = 0; index < EditorWindow.Instance.Notes.Count; index++)
			{
				var note = EditorWindow.Instance.Notes[index];
				var visible = audioTime < note.Ms && note.Ms - audioTime <= 750;

				if (!visible)
					continue;

				var x = rect.X + note.X * cellSize + gap / 2;
				var y = rect.Y + note.Y * cellSize + gap / 2;

				var progress = (float)Math.Pow(1 - Math.Min(1, (note.Ms - audioTime) / 750.0), 2);
				
				var noteRect = new RectangleF(x, y, noteSize, noteSize);
				GL.Color4(note.Color.R, note.Color.G, note.Color.B, progress * 0.15f);
				Glu.RenderQuad(noteRect);
				GL.Color4(note.Color.R, note.Color.G, note.Color.B, progress);
				Glu.RenderOutline(noteRect);

				if (editor.ApproachSquares.Toggle)
				{
					var outlineSize = 4 + noteSize + noteSize * (1 - progress) * 2;
					Glu.RenderOutline(x - outlineSize / 2 + noteSize / 2, y - outlineSize / 2 + noteSize / 2,
						outlineSize,
						outlineSize);
				}

				if (editor.GridNumbers.Toggle)
				{
					GL.Color4(1, 1, 1, progress);
					var s = $"{(index + 1):#,##}";
					var w = fr.GetWidth(s, 24);
					var h = fr.GetHeight(24);

					fr.Render(s, (int) (noteRect.X + noteRect.Width / 2 - w / 2f),
						(int) (noteRect.Y + noteRect.Height / 2 - h / 2f), 24);
				}

				if (!mouseOver)
				{
					MouseOverNote = null;
				}

				if (EditorWindow.Instance.SelectedNotes.Contains(note))
				{
					var outlineSize = noteSize + 8;

					GL.Color4(0, 0.5f, 1f, progress);
					Glu.RenderOutline(x - outlineSize / 2 + noteSize / 2, y - outlineSize / 2 + noteSize / 2,
						outlineSize, outlineSize);
				}

				if (!mouseOver && noteRect.Contains(mouseX, mouseY))
				{
					MouseOverNote = note;
					mouseOver = true;

					GL.Color3(0, 1, 0.25f);
					Glu.RenderOutline(x - 4, y - 4, noteSize + 8, noteSize + 8);
				}
			}
		}
	}
}