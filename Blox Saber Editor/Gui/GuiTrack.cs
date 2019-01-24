using System.Collections.Generic;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Blox_Saber_Editor.Gui
{
	class GuiTrack : Gui
	{
		private readonly ColorSequence _cs = new ColorSequence();

		public Note MouseOverNote;

		public float ScreenX = 300;

		public float BPM = 0;//150;
		public int BPMOffset = 0; //in ms
		public int BeatDivisor = 8;

		public GuiTrack(float y, float sy) : base(0, y, EditorWindow.Instance.ClientSize.Width, sy)
		{

		}

		public override void Render(float delta, float mouseX, float mouseY)
		{
			GL.Color3(0.1f, 0.1f, 0.1f);

			var rect = ClientRectangle;

			GLU.RenderQuad(rect);
			GL.Color3(0.2f, 0.2f, 0.2f);
			GLU.RenderQuad((int)rect.X, (int)rect.Y + rect.Height, (int)rect.Width, 1);

			var fr = EditorWindow.Instance.FontRenderer;

			var cellSize = rect.Height;
			var noteSize = cellSize * 0.65f;

			var gap = cellSize - noteSize;

			var audioTime = EditorWindow.Instance.MusicPlayer.CurrentTime.TotalMilliseconds;

			var cubeStep = EditorWindow.Instance.CubeStep;
			var posX = (float)audioTime / 1000 * cubeStep;
			var maxX = (float)EditorWindow.Instance.MusicPlayer.TotalTime.TotalMilliseconds / 1000 * cubeStep;

			//var screenSeconds = rect.Width / cubeStep;

			var zoomLvl = (int)EditorWindow.Instance.Zoom;
			//var lines = Math.Ceiling(screenSeconds + 1) * zoomLvl;
			var lineSpace = cubeStep / zoomLvl;
			//var stepSmall = lineSpace / 4;

			//var stepSmallTime = (int)(stepSmall / cubeStep * 1000 * 100) / 100f;
			//var stepText = stepSmallTime.ToString("#,##.##") + "ms";
			//var stepTextW = fr.GetWidth(stepText, 16);
			//var stepTextH = fr.GetHeight(16);

			var lineX = ScreenX - posX;
			if (lineX < 0)
				lineX %= lineSpace;

			//render quarters of a second depending on zoom level
			while (lineSpace > 0 && lineX < rect.Width)
			{
				GL.Color3(0.65f, 0.65f, 0.65f);
				GL.Begin(PrimitiveType.Lines);
				GL.Vertex2((int)lineX + 0.5f, rect.Y);
				GL.Vertex2((int)lineX + 0.5f, rect.Y + rect.Height + 60);
				GL.End();
				/*
				for (int j = 1; j <= 4; j++)
				{
					var xo = lineX + j * stepSmall;

					GL.Color4(0, 0.75f, 1, 0.75f);
					fr.Render(stepText, (int)(xo - stepSmall / 2 - stepTextW / 2f), (int)(rect.Y + rect.Height + 50 - stepTextH), 16);

					if (j < 4)
					{
						GL.Color3(0.3f, 0.3f, 0.3f);
						GL.Begin(PrimitiveType.Lines);
						GL.Vertex2((int)xo + 0.5f, rect.Y + rect.Height / 2);
						GL.Vertex2((int)xo + 0.5f, rect.Y + rect.Height + 50);
						GL.End();
					}
				}
				*/
				lineX += lineSpace;
			}

			var mouseOver = false;

			//draw start line
			GL.Color4(0f, 1f, 0f, 1);
			GL.Begin(PrimitiveType.Lines);
			GL.Vertex2((int)(ScreenX - posX) + 0.5f, rect.Y);
			GL.Vertex2((int)(ScreenX - posX) + 0.5f, rect.Y + rect.Height + 8);
			GL.End();
			//draw end line
			GL.Color4(1f, 0f, 0f, 1);
			GL.Begin(PrimitiveType.Lines);
			GL.Vertex2((int)(ScreenX - posX + maxX) + 0.5f, rect.Y);
			GL.Vertex2((int)(ScreenX - posX + maxX) + 0.5f, rect.Y + rect.Height + 8);
			GL.End();

			MouseOverNote = null;

			_cs.Reset();
			for (int i = 0; i < EditorWindow.Instance.Notes.Count; i++)
			{
				Note note = EditorWindow.Instance.Notes[i];
				note.Color = _cs.Next();

				var x = ScreenX - posX + note.Ms / 1000f * cubeStep;

				if (x < rect.X - noteSize || x > rect.Width)
					continue;

				var alphaMult = 1f;

				if (x <= ScreenX)
				{
					alphaMult = 0.35f;
				}

				var y = rect.Y + gap / 2;

				var noteRect = new RectangleF(x, y, noteSize, noteSize);

				var b = MouseOverNote == null && !mouseOver && noteRect.Contains(mouseX, mouseY);

				if ((b || EditorWindow.Instance.SelectedNotes.Contains(note)) && !EditorWindow.Instance.IsDraggingNoteOnTimeLine)
				{
					if (b)
					{
						MouseOverNote = note;
						mouseOver = true;
						GL.Color3(0, 1, 0.25f);
					}
					else
					{
						GL.Color3(0, 0.5f, 1);
					}

					GLU.RenderOutline((int)(x - 4), (int)(y - 4), (int)(noteSize + 8), (int)(noteSize + 8));
				}

				GL.Color4(note.Color.R, note.Color.G, note.Color.B, alphaMult * 0.2f);
				GLU.RenderQuad((int)x, (int)y, (int)noteSize, (int)noteSize);
				GL.Color4(note.Color.R, note.Color.G, note.Color.B, alphaMult * 1f);
				GLU.RenderOutline((int)x, (int)y, (int)noteSize, (int)noteSize);

				var gridGap = 2;
				for (int j = 0; j < 9; j++)
				{
					var indexX = 2 - j % 3;
					var indexY = 2 - j / 3;

					var gridX = (int)x + indexX * (9 + gridGap) + 5;
					var gridY = (int)y + indexY * (9 + gridGap) + 5;

					if (note.X == indexX && note.Y == indexY)
						GLU.RenderQuad(gridX, gridY, 9, 9);
					else
						GLU.RenderOutline(gridX, gridY, 9, 9);
				}

				var numText = $"{(i + 1):#,##}";

				GL.Color3(0, 0.75f, 1f);
				fr.Render(numText, (int)x + 3, (int)(rect.Y + rect.Height) + 3, 16);

				GL.Color3(0, 1f, 0.5f);
				fr.Render($"{note.Ms:#,##}ms", (int)x + 3, (int)(rect.Y + rect.Height + fr.GetHeight(16)) + 3 + 2, 16);

				//draw line
				GL.Color4(1f, 1f, 1f, alphaMult);
				GL.Begin(PrimitiveType.Lines);
				GL.Vertex2((int)x + 0.5f, rect.Y + rect.Height - 4);
				GL.Vertex2((int)x + 0.5f, rect.Y + rect.Height + 30);
				GL.End();
			}

			if (BPM > 33)
			{
				lineSpace = 60 / BPM * cubeStep;
				var stepSmall = lineSpace / BeatDivisor;

				lineX = ScreenX - posX + BPMOffset / 1000f * cubeStep;
				if (lineX < 0)
					lineX %= lineSpace;

				//render BPM lines
				while (lineSpace > 0 && lineX < rect.Width)
				{
					GL.Color3(0, 1f, 0);
					GL.Begin(PrimitiveType.Lines);
					GL.Vertex2((int) lineX + 0.5f, rect.Bottom);
					GL.Vertex2((int) lineX + 0.5f, rect.Bottom - 8);
					GL.End();

					for (int j = 1; j <= BeatDivisor; j++)
					{
						var xo = lineX + j * stepSmall;

						if (j < BeatDivisor)
						{
							GL.Color3(0, 0.5f, 0);
							GL.Begin(PrimitiveType.Lines);
							GL.Vertex2((int) xo + 0.5f, rect.Bottom - 5);
							GL.Vertex2((int) xo + 0.5f, rect.Bottom);
							GL.End();
						}
					}

					lineX += lineSpace;
				}
			}
			//draw screen line
			GL.Color3(1f, 0.5f, 0);
			GL.Begin(PrimitiveType.Lines);
			GL.Vertex2(rect.X + ScreenX + 0.5f, rect.Y + 4);
			GL.Vertex2(rect.X + ScreenX + 0.5f, rect.Y + rect.Height - 4);
			GL.End();

			//GL.Color3(1, 1, 1f);
			//FontRenderer.Print("HELLO", 0, rect.Y + rect.Height + 8);
		}

		public override void OnResize(Size size)
		{
			ClientRectangle = new RectangleF(0, ClientRectangle.Y, size.Width, ClientRectangle.Height);

			ScreenX = ClientRectangle.Width / 2.5f;
		}

		public List<Note> GetNotesInRect(RectangleF selectionRect)
		{
			var notes = new List<Note>();

			var rect = ClientRectangle;

			var cellSize = rect.Height;
			var noteSize = cellSize * 0.65f;

			var gap = cellSize - noteSize;

			var audioTime = EditorWindow.Instance.MusicPlayer.CurrentTime.TotalMilliseconds;

			var cubeStep = EditorWindow.Instance.CubeStep;
			var posX = (float)audioTime / 1000 * cubeStep;

			for (int i = 0; i < EditorWindow.Instance.Notes.Count; i++)
			{
				Note note = EditorWindow.Instance.Notes[i];
				note.Color = _cs.Next();

				var x = ScreenX - posX + note.Ms / 1000f * cubeStep;

				if (x < rect.X - noteSize || x > rect.Width)
					continue;

				var y = rect.Y + gap / 2;

				var noteRect = new RectangleF(x, y, noteSize, noteSize);

				if (selectionRect.IntersectsWith(noteRect))
					notes.Add(note);
			}

			return notes;
		}
	}
}