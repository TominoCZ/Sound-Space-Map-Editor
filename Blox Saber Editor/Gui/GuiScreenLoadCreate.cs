using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using Color = System.Drawing.Color;

namespace Blox_Saber_Editor.Gui
{
	class GuiScreenLoadCreate : GuiScreen
	{
		private readonly Random _r = new Random();

		private readonly GuiButton _createButton;
		private readonly GuiButton _loadButton;
		private readonly GuiButton _pasteButton;

		private readonly List<Particle> _particles = new List<Particle>();

		private readonly int _textureId;

		private float _timer;

		public GuiScreenLoadCreate() : base(0, 0, EditorWindow.Instance.ClientSize.Width, EditorWindow.Instance.ClientSize.Height)
		{
			using (var img = Properties.Resources.BloxSaber)
			{
				_textureId = TextureManager.GetOrRegister("logo", img);
			}

			_createButton = new GuiButton(0, 0, 0, 192, 64, "CREATE MAP");
			_loadButton = new GuiButton(1, 0, 0, 192, 64, "LOAD MAP");
			_pasteButton = new GuiButton(2, 0, 0, 192, 64, "PASTE MAP");

			Buttons.Add(_createButton);
			Buttons.Add(_loadButton);
			Buttons.Add(_pasteButton);

			OnResize(EditorWindow.Instance.ClientSize);
		}

		public override void Render(float delta, float mouseX, float mouseY)
		{
			_timer += delta;

			if (_timer >= 0.08)
			{
				_timer = 0;

				for (int i = 0; i < 3; i++)
				{
					var s = 15 + (float)_r.NextDouble() * 20;
					var x = (float)_r.NextDouble() * ClientRectangle.Width;
					var y = ClientRectangle.Height + s;

					var mx = -0.5f + (float)_r.NextDouble();
					var my = -(0.35f + (float)_r.NextDouble() * 0.75f);

					_particles.Add(new Particle(x, y, mx, my, s));
				}
			}

			var rect = ClientRectangle;

			GL.Color3(1, 1, 1f);
			Glu.RenderTexturedQuad(rect.X + rect.Width / 2 - 256, 0, 512, 512, 0, 0, 1, 1, _textureId);

			for (var index = _particles.Count - 1; index >= 0; index--)
			{
				var particle = _particles[index];

				particle.Render(delta);

				if (particle.IsDead)
					_particles.Remove(particle);
			}

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

					var wasFullscreen = EditorWindow.Instance.IsFullscreen;

					if (EditorWindow.Instance.IsFullscreen)
					{
						EditorWindow.Instance.ToggleFullscreen();
					}

					var result = ofd.ShowDialog();

					if (wasFullscreen)
					{
						EditorWindow.Instance.ToggleFullscreen();
					}

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

		public override bool AllowInput()
		{
			return false;
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

	class Particle
	{
		public float X, Y, Mx, My, Size, Age, MaxAge, Angle, AngleStep;

		public int RotationOrientation = 1;

		public bool IsDead;

		private static Random _r = new Random();

		public Particle(float x, float y, float mx, float my, float size)
		{
			X = x;
			Y = y;

			Mx = mx;
			My = my;

			Size = size;

			MaxAge = 0.75f + (float)_r.NextDouble() * 2f;
			Angle = (float)_r.NextDouble() * 45;
			AngleStep = 10 + (float)_r.NextDouble() * 50;

			if (_r.NextDouble() >= 0.45)
				RotationOrientation = -1;
		}

		public void Render(float delta)
		{
			if (IsDead)
				return;

			var speedMult = Math.Max(0.001f, (MaxAge - Age) / MaxAge);

			if (speedMult <= 0.001f)
				IsDead = true;

			var squareMult = (float)Math.Pow(speedMult, 2);

			X += Mx * squareMult;
			Y += My * squareMult;

			Age += delta;
			Angle += delta * RotationOrientation * 360 * speedMult;

			var size = Size * speedMult;

			var alpha = (int)(255 * squareMult);

			GL.Color4(RotationOrientation == 1 ? Color.FromArgb((int)(alpha * 0.2f), 255, 0, 0) : Color.FromArgb((int)(alpha * 0.2f), 0, 255, 255));
			GL.Translate(X, Y, 0);
			GL.Rotate(Angle, 0, 0, 1);
			Glu.RenderQuad(-size / 2, -size / 2, size, size);
			GL.Color4(RotationOrientation == 1 ? Color.FromArgb(alpha, 255, 0, 0) : Color.FromArgb(alpha, 0, 255, 255));
			Glu.RenderOutline(-size / 2, -size / 2, size, size);
			GL.Rotate(-Angle, 0, 0, 1);
			GL.Translate(-X, -Y, 0);
		}
	}
}
