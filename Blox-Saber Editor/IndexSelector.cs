using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.MediaFoundation;

namespace Blox_Saber_Editor
{
    public partial class IndexSelector : UserControl
    {
        public event EventHandler OnIndexChanged;
        
        public int IndexX = -1;
        public int IndexY = -1;
        
        private Point _mouse;

        public IndexSelector()
        {
            InitializeComponent();
        }

        private void IndexSelector_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingMode = CompositingMode.SourceOver;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            var thirdX = Width / 3;
            var thirdY = Height / 3;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var x = i * thirdX;
                    var y = j * thirdY;

                    e.Graphics.FillRectangle(Brushes.DodgerBlue, x, y, thirdX, thirdY);
                }
            }

            e.Graphics.FillRectangle(Brushes.Red, IndexX * thirdX + 1, (2 - IndexY) * thirdY + 1, thirdX, thirdY);

            var indexX = _mouse.X / thirdX;
            var indexY = _mouse.Y / thirdY;

            if (ClientRectangle.Contains(PointToClient(Cursor.Position)))
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(128, 255, 0, 0)), indexX * thirdX + 1,
                    indexY * thirdY + 1, thirdX - 1, thirdY - 1);
            }

            for (int i = 0; i < 4; i++)
            {
                var x = Math.Min(i * thirdX + 1, Width - 1);
                e.Graphics.DrawLine(Pens.Black, x, 0, x, Height);

                var y = Math.Min(i * thirdY + 1, Height - 1);
                e.Graphics.DrawLine(Pens.Black, 0, y, Width, y);
            }
        }

        private void IndexSelector_MouseMove(object sender, MouseEventArgs e)
        {
            _mouse = e.Location;

            Invalidate();
        }

        private void IndexSelector_MouseDown(object sender, MouseEventArgs e)
        {
            var thirdX = Width / 3;
            var thirdY = Height / 3;

            var indexX = _mouse.X / thirdX;
            var indexY = (Width - _mouse.Y) / thirdY;

            if (indexX != IndexX || indexY != IndexY)
            {
                IndexX = indexX;
                IndexY = indexY;

                OnIndexChanged?.Invoke(this, null);

                Invalidate();
            }
        }

        private void IndexSelector_MouseLeave(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
