using System;
using System.Drawing;
using System.Security.AccessControl;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Blox_Saber_Editor
{
	class GuiScreenEditor : GuiScreen
	{
		public double Progress;

		public readonly GuiGrid Grid = new GuiGrid(300, 300);
		public readonly GuiTrack Track = new GuiTrack(0, 64);
		public readonly GuiTempo Tempo = new GuiTempo(512, 64);
		public readonly GuiVolume Volume = new GuiVolume(64, 256);

		private GuiLabel _toast;
		private float _toastTime;

		public GuiScreenEditor() : base(0, EditorWindow.Instance.ClientSize.Height - 64, EditorWindow.Instance.ClientSize.Width - 512 - 64, 64)
		{
			_toast = new GuiLabel(0, 0, "") { Centered = true, FontSize = 36 };

			Buttons.Add(new GuiButtonPlayPause(0, EditorWindow.Instance.ClientSize.Width - 512 - 64, EditorWindow.Instance.ClientSize.Height - 64, 64, 64));

			OnResize(EditorWindow.Instance.ClientSize);
		}

		public override void Render(float delta, float mouseX, float mouseY)
		{
			_toastTime = Math.Min(2, _toastTime + delta);

			var toastOffY = 1f;

			if (_toastTime <= 0.5)
			{
				toastOffY = (float)Math.Sin(Math.Min(0.5, _toastTime) / 0.5 * MathHelper.PiOver2);
			}
			else if (_toastTime >= 1.75)
			{
				toastOffY = (float)Math.Cos(Math.Min(0.25, _toastTime - 1.75) / 0.25 * MathHelper.PiOver2);
			}

			var size = EditorWindow.Instance.ClientSize;

			var h = EditorWindow.Instance.FontRenderer.GetHeight(_toast.FontSize);

			_toast.ClientRectangle.Y = size.Height - Tempo.ClientRectangle.Height + h - toastOffY * h * 2;
			//_toast.ClientRectangle.Y = 200;
			Grid.Render(delta, mouseX, mouseY);

			_toast.Render(delta, mouseX, mouseY);

			Track.Render(delta, mouseX, mouseY);
			Tempo.Render(delta, mouseX, mouseY);
			Volume.Render(delta, mouseX, mouseY);
			//TextBox.Render(delta, mouseX, mouseY);

			var rect = ClientRectangle;

			var timelinePos = new Vector2(rect.Height / 2f, rect.Height / 2f - 1);
			var timelineSize = new Vector2(rect.Width - rect.Height, 2);

			//background
			GL.Color3(0.1f, 0.1f, 0.1f);
			GLU.RenderQuad((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

			//timeline
			GL.Color3(0.5f, 0.5f, 0.5f);
			GLU.RenderQuad(timelinePos.X + rect.X, timelinePos.Y + rect.Y, timelineSize.X, timelineSize.Y);

			var cursorPos = timelineSize.X * Progress;

			//cursor
			GL.Color3(1f, 1, 1);
			GLU.RenderQuad(timelinePos.X + cursorPos - 2.5f + rect.X, timelinePos.Y - rect.Height * 0.5f / 2 + rect.Y, 5, rect.Height * 0.5f);

			base.Render(delta, mouseX, mouseY);
		}

		protected override void OnButtonClicked(int id)
		{
			if (id == 0)
			{
				if (EditorWindow.Instance.MusicPlayer.IsPlaying)
					EditorWindow.Instance.MusicPlayer.Pause();
				else
					EditorWindow.Instance.MusicPlayer.Play();
			}
		}

		public override void OnResize(Size size)
		{
			Buttons[0].ClientRectangle = new RectangleF(size.Width - 512 - 64, size.Height - 64, 64, 64);

			ClientRectangle = new RectangleF(0, size.Height - 64, size.Width - 512 - 64, 64);

			Track.OnResize(size);
			Tempo.OnResize(size);
			Volume.OnResize(size);

			Grid.ClientRectangle = new RectangleF((int)(size.Width / 2f - Grid.ClientRectangle.Width / 2), (int)((size.Height + Track.ClientRectangle.Height - 64) / 2 - Grid.ClientRectangle.Height / 2), Grid.ClientRectangle.Width, Grid.ClientRectangle.Height);

			_toast.ClientRectangle.X = size.Width / 2f;
		}

		public void ShowToast(string text, Color color)
		{
			_toastTime = 0;

			_toast.Text = text;
			_toast.Color = color;
		}
	}
}