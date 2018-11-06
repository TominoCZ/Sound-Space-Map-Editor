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
        public int SideRenderOffset = 10;

        public TimeSpan TotalTime { get; set; } = TimeSpan.FromMilliseconds(1);
        public TimeSpan CurrentTime { get; set; }

        public int BarWidth = 5;

        private float _channel = 0.5f;

        private TimeStamp _last;

        private List<TimeStamp> _points = new List<TimeStamp>();

        public Timeline()
        {
            InitializeComponent();
        }

        private void Timeline_Paint(object sender, PaintEventArgs e)
        {
            var progress = (float)(CurrentTime.TotalMilliseconds / TotalTime.TotalMilliseconds);

            var my = Height / 2;

            var r = (int)(_channel * 255);

            var c = Color.FromArgb(r, r, r);

            e.Graphics.Clear(c);

            e.Graphics.FillRectangle(Brushes.Black, SideRenderOffset, my - BarWidth / 2 - 1, (Width - SideRenderOffset * 2) + 1, BarWidth + 2);
            e.Graphics.FillRectangle(Brushes.Red, SideRenderOffset + 1, my - BarWidth / 2, (Width - SideRenderOffset * 2) * progress - 1, BarWidth);
            
            e.Graphics.FillRectangle(Brushes.White, SideRenderOffset + (Width - SideRenderOffset * 2) * progress, my - 7, 1, 14);
            
            lock (_points)
            {
                foreach (var point in _points)
                {
                    e.Graphics.FillRectangle(Brushes.Black, SideRenderOffset + (Width - SideRenderOffset * 2) * (float)(point.Time / TotalTime.TotalMilliseconds), my + 7 + 3, 1, 8);

                    if (point.Dirty)
                        e.Graphics.FillRectangle(Brushes.LimeGreen, SideRenderOffset + (Width - SideRenderOffset * 2) * (float)(point.Time / TotalTime.TotalMilliseconds), my + 7 + 3 + 8, 1, 5);
                }
            }

            var current = GetCurrentTimeStamp();

            if (current != null)
                e.Graphics.FillRectangle(Brushes.White, SideRenderOffset + (Width - SideRenderOffset * 2) * (float)(current.Time / TotalTime.TotalMilliseconds), my + 7 + 3 + 8, 1, 3);
            
            _channel = Math.Max(0.5f, _channel * 0.9125f);

            if (_last != current)
            {
                _last = current;
                _channel = 0.92f;
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

        public void Add(TimeStamp point)
        {
            lock (_points)
            {
                _points.Add(point);
                Sort();
            }
        }

        public void Remove(TimeStamp point)
        {
            lock (_points)
            {
                _points.Remove(point);
                Sort();
            }
        }

        public void Sort()
        {
            lock (_points)
            {
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