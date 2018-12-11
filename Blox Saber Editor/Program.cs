using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using ManagedBass;
using NAudio.Wave;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using MouseEventArgs = OpenTK.Input.MouseEventArgs;

namespace Blox_Saber_Editor
{
    static class Program
    {
        /// <summary>
        /// Hlavní vstupní bod aplikace.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var w = new EditorWindow())
            {
                w.Run();
            }
        }
    }

    class EditorWindow : GameWindow
    {
        public static EditorWindow Instance;

        public bool IsPaused { get; private set; }

        public GuiRenderer GuiRenderer = new GuiRenderer();

        public GuiScreen GuiScreen;

        private PointF _lastMouse;

        public EditorWindow() : base(640, 480, new GraphicsMode(32, 8, 0, 4), "Blox Saber Editor")
        {
            Instance = this;
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GuiScreen?.Render(_lastMouse.X, _lastMouse.Y);

            GuiRenderer.Render(_lastMouse.X, _lastMouse.Y);

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle);

            GL.MatrixMode(MatrixMode.Projection);
            var m = Matrix4.CreateOrthographicOffCenter(0, Width, Height, 0, 0, 1);
            GL.LoadMatrix(ref m);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            _lastMouse = new PointF(e.X, e.Y);

            GuiScreen?.OnMouseMove(e.X, e.Y);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            GuiScreen?.OnMouseClick(e.X, e.Y);
        }

        public void OpenGuiScreen(GuiScreen s)
        {
            GuiScreen?.OnClosing();

            IsPaused = s != null && s.Pauses;

            GuiScreen = s;
        }
    }

    class GuiRenderer
    {
        public List<Gui> Guis = new List<Gui>();

        public void Render(float mouseX, float mouseY)
        {
            foreach (var gui in Guis)
            {
                gui.Render(mouseX, mouseY);
            }
        }
    }

    class GuiScreen : Gui
    {
        public bool Pauses { get; }

        protected GuiScreen(float x, float y, float sx, float sy) : base(x, y, sx, sy)
        {
            Pauses = true;
        }

        public virtual void OnMouseMove(float x, float y)
        {

        }

        public virtual void OnMouseClick(float x, float y)
        {

        }

        public virtual void OnClosing()
        {

        }
    }

    class GuiButton : Gui
    {
        public bool IsMouseOver { get; protected set; }

        protected GuiButton(float x, float y, float sx, float sy) : base(x, y, sx, sy)
        {

        }

        public override void Render(float mouseX, float mouseY)
        {
            IsMouseOver = ClientRectangle.Contains(mouseX, mouseY);
        }
    }

    class Gui
    {
        public RectangleF ClientRectangle;

        protected Gui(float x, float y, float sx, float sy)
        {
            ClientRectangle = new RectangleF(x, y, sx, sy);
        }


        public virtual void Render(float mouseX, float mouseY)
        {

        }
    }
}
