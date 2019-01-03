using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Blox_Saber_Editor
{
	class GuiTrack : Gui
	{
		private ColorSequence _cs = new ColorSequence();

		private float _mpb = 327.868852459016f;

		public Note MouseOverNote;

		public float ScreenX = 300;

		public GuiTrack(float y, float sy) : base(0, y, EditorWindow.Instance.ClientSize.Width, sy)
		{

		}

		public override void Render(float mouseX, float mouseY)
		{
			GL.Color3(0.1f, 0.1f, 0.1f);

			GLU.RenderQuad(ClientRectangle);

			var rect = ClientRectangle;

			var cellSize = rect.Height;
			var noteSize = cellSize * 0.65f;

			var gap = cellSize - noteSize;

			var audioTime = EditorWindow.Instance.MusicPlayer.CurrentTime.TotalMilliseconds;

			var cubeStep = EditorWindow.Instance.CubeStep;
			var posX = (float)audioTime / 1000 * cubeStep;
			var maxX = (float)EditorWindow.Instance.MusicPlayer.TotalTime.TotalMilliseconds / 1000 * cubeStep;

			var screenSeconds = rect.Width / cubeStep;

			var zoomLvl = (int)EditorWindow.Instance.Zoom;
			var lines = Math.Ceiling(screenSeconds + 1) * zoomLvl;
			var lineSpace = cubeStep / zoomLvl;

			GL.Color3(0.25f, 0.25f, 0.25f);
			for (int i = 0; i < lines; i++)
			{
				var x = i * lineSpace - (posX - ScreenX) % lineSpace;

				GL.Begin(PrimitiveType.Lines);
				GL.Vertex2((int)x + 0.5f, rect.Y);
				GL.Vertex2((int)x + 0.5f, rect.Y + rect.Height);
				GL.End();
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

			_cs.Reset();
			foreach (var note in EditorWindow.Instance.Notes)
			{
				note.Color = _cs.Next();

				var x = ScreenX - posX + note.Ms / 1000f * cubeStep;

				if (x < rect.X - noteSize || x > rect.Width)
					continue;

				var alphaMult = 1f;

				if (x <= ScreenX)
				{
					alphaMult = 0.25f;
				}

				var y = rect.Y + gap / 2;

				var noteRect = new RectangleF(x, y, noteSize, noteSize);

				if (!mouseOver)
				{
					MouseOverNote = null;
				}

				var b = !mouseOver && noteRect.Contains(mouseX, mouseY);

				if (b || EditorWindow.Instance.SelectedNote == note)
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

					GLU.RenderOutline(x - 4, y - 4, noteSize + 8, noteSize + 8);
				}

				GL.Color4(note.Color.R, note.Color.G, note.Color.B, alphaMult * 0.2f);
				GLU.RenderQuad(x, y, noteSize, noteSize);
				GL.Color4(note.Color.R, note.Color.G, note.Color.B, alphaMult * 1f);
				GLU.RenderOutline(x, y, noteSize, noteSize);

				//draw line
				GL.Color4(1f, 1f, 1f, alphaMult);
				GL.Begin(PrimitiveType.Lines);
				GL.Vertex2((int)x + 0.5f, rect.Y + rect.Height - 4);
				GL.Vertex2((int)x + 0.5f, rect.Y + rect.Height + 4);
				GL.End();
			}

			GL.Color3(1f, 1, 1);
			GL.Begin(PrimitiveType.Lines);
			GL.Vertex2(rect.X + ScreenX + 0.5f, rect.Y + 4);
			GL.Vertex2(rect.X + ScreenX + 0.5f, rect.Y + rect.Height - 4);
			GL.End();

			//GL.Color3(1, 1, 1f);
			//FontRenderer.Print("HELLO", 0, rect.Y + rect.Height + 8);
		}

		public void OnResize(Size size)
		{
			ClientRectangle = new RectangleF(0, ClientRectangle.Y, size.Width, ClientRectangle.Height);

			ScreenX = ClientRectangle.Width / 2.5f;
		}
	}
}