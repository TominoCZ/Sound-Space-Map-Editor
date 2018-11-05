using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Blox_Saber_Editor
{
    public partial class Timeline : UserControl
    {
        public TimeSpan TotalTime { get; set; } = TimeSpan.FromMilliseconds(1);
        public TimeSpan CurrentTime { get; set; }

        public List<TimeStamp> _points = new List<TimeStamp>();

        public int BarWidth = 10;

        public Timeline()
        {
            InitializeComponent();
        }

        private void Timeline_Paint(object sender, PaintEventArgs e)
        {
            var progress = (float)(CurrentTime.TotalMilliseconds / TotalTime.TotalMilliseconds);

            var my = Height / 2;

            e.Graphics.FillRectangle(Brushes.Black, 0, my - BarWidth / 2 - 1, Width + 1, BarWidth + 2);
            e.Graphics.FillRectangle(Brushes.Red, 0, my - BarWidth / 2, Width * progress, BarWidth);

            e.Graphics.FillRectangle(Brushes.White, Width * progress, my - 7, 1, 14);

            lock (_points)
            {
                foreach (var point in _points)
                {
                    e.Graphics.FillRectangle(Brushes.Black, Width * (float)(point.Time / TotalTime.TotalMilliseconds), my + 7 + 3, 1, 8);

                    if (point.Dirty)
                        e.Graphics.FillRectangle(Brushes.LimeGreen, Width * (float)(point.Time / TotalTime.TotalMilliseconds), my + 7 + 3 + 8, 1, 3);
                }
            }
        }

        private void Timeline_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }

        public void AddPoint(TimeStamp point)
        {
            lock (_points)
            {
                _points.Add(point);
            }
        }

        public void Clear()
        {
            lock (_points)
            {
                _points.Clear();
            }
        }
    }

    public class TimeStamp
    {
        public int Time;

        public int X;
        public int Y;

        public bool Dirty;

        public TimeStamp(int time, int x, int y)
        {
            Time = time;

            X = x;
            Y = y;
        }
    }
}