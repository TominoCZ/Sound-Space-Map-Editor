using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAudio.Midi;

namespace Blox_Saber_Editor
{
    public partial class Timeline : UserControl
    {
        public TimeSpan TotalTime { get; set; } = TimeSpan.FromMilliseconds(1);
        public TimeSpan CurrentTime { get; set; }

        private List<TimeStamp> _points = new List<TimeStamp>();

        public int BarWidth = 10;

        private float channel = 0.5f;

        private TimeStamp _last;

        public Timeline()
        {
            InitializeComponent();
        }

        private void Timeline_Paint(object sender, PaintEventArgs e)
        {
            var progress = (float)(CurrentTime.TotalMilliseconds / TotalTime.TotalMilliseconds);

            var my = Height / 2;

            var r = (int)(channel * 255);

            var c = Color.FromArgb(r, r, r);

            e.Graphics.Clear(c);

            e.Graphics.FillRectangle(Brushes.Black, 0, my - BarWidth / 2 - 1, Width + 1, BarWidth + 2);
            e.Graphics.FillRectangle(Brushes.Red, 0, my - BarWidth / 2, Width * progress, BarWidth);

            e.Graphics.FillRectangle(Brushes.White, Width * progress, my - 7, 1, 14);

            var testPoint = GetCurrentTimeStamp();

            if (testPoint != null)
                e.Graphics.FillRectangle(Brushes.DarkOrange, Width * (float)(testPoint.Time / TotalTime.TotalMilliseconds), my + 7 + 3 + 8 + 3, 1, 3);

            lock (_points)
            {
                foreach (var point in _points)
                {
                    e.Graphics.FillRectangle(Brushes.Black, Width * (float)(point.Time / TotalTime.TotalMilliseconds), my + 7 + 3, 1, 8);

                    if (point.Dirty)
                        e.Graphics.FillRectangle(Brushes.LimeGreen, Width * (float)(point.Time / TotalTime.TotalMilliseconds), my + 7 + 3 + 8, 1, 3);
                }
            }

            var current = GetCurrentTimeStamp();

            channel = Math.Max(0.5f, channel * 0.875f);

            if (_last != current)
            {
                _last = current;
                channel = 0.92f;
            }
        }

        private void Timeline_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }

        public TimeStamp GetCurrentTimeStamp()
        {
            TimeStamp ts = null;

            lock (_points)
            {
                int currentTime = (int)CurrentTime.TotalMilliseconds;
                foreach (TimeStamp stamp in _points)
                {
                    if (stamp.Time <= currentTime)
                        ts = stamp;
                    else
                        break;
                }
            }

            return ts;
        }

        public TimeStamp GetPreviousTimeStamp()
        {
            TimeStamp ts = null;

            lock (_points)
            {
                int currentTime = (int)CurrentTime.TotalMilliseconds;

                foreach (TimeStamp stamp in _points)
                {
                    if (stamp.Time < currentTime)
                        ts = stamp;
                    else
                        break;
                }
            }

            return ts;
        }

        public TimeStamp GetNextTimeStamp()
        {
            lock (_points)
            {
                int currentTime = (int)CurrentTime.TotalMilliseconds;

                foreach (TimeStamp stamp in _points)
                {
                    if (stamp.Time > currentTime)
                        return stamp;
                }
            }

            return null;
        }

        public List<TimeStamp> GetPoints()
        {
            List<TimeStamp> stamps = new List<TimeStamp>();

            lock (_points)
            {
                stamps.AddRange(_points);
            }

            return stamps;
        }

        public void AddPoint(TimeStamp point)
        {
            lock (_points)
            {
                _points.Add(point);
                _points = _points.OrderBy(ts => ts.Time).ToList();
            }
        }

        public void Replace(TimeStamp which, TimeStamp with)
        {
            lock (_points)
            {
                var index = _points.IndexOf(which);

                _points.Remove(which);
                _points.Insert(index, with);

                _points = _points.OrderBy(p => p.Time).ToList();
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