using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAudio.Midi;

namespace Blox_Saber_Editor
{
    public partial class Timeline : UserControl
    {
        public int SideRenderOffset = 10;

        public event EventHandler<float> OnDrag;
        public event EventHandler OnDragBegin;
        public event EventHandler OnDragEnd;

        public TimeSpan TotalTime { get; set; }
        public TimeSpan CurrentTime { get; set; }

        public bool Smooth { get; set; }

        public int BarWidth = 5;

        private float _channel = 0.5f;

        private bool _dragging;

        private TimeStamp _last;

        private List<TimeStamp> _points = new List<TimeStamp>();

		private Stopwatch _frameTimer = new Stopwatch();

        public Timeline()
        {
            InitializeComponent();
        }

        private void Timeline_Paint(object sender, PaintEventArgs e)
        {
	        var delta = _frameTimer.Elapsed.TotalSeconds;
	        _frameTimer.Restart();


			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;

            var progress = (float)(TotalTime.TotalMilliseconds == 0 ? 0 : CurrentTime.TotalMilliseconds / TotalTime.TotalMilliseconds);

            var my = Height / 2;

            var r = (int)(_channel * 255);

            var c = Color.FromArgb(r, r, r);

            e.Graphics.Clear(c);

            e.Graphics.FillRectangle(Brushes.Black, SideRenderOffset, my - BarWidth / 2 - 1, (Width - SideRenderOffset * 2) + 1, BarWidth + 2);
            e.Graphics.FillRectangle(Brushes.Red, SideRenderOffset + 1, my - BarWidth / 2, (Width - SideRenderOffset * 2) * progress - 1, BarWidth);

            e.Graphics.FillRectangle(Brushes.White, SideRenderOffset + (Width - SideRenderOffset * 2) * progress, my - 7, 1, 14);

            if (Smooth)
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            }

            lock (_points)
            {
                for (int i = 0; i < _points.Count; i++)
                {
                    TimeStamp point = _points[i];

                    var pointSize = i % 2 == 0 ? 8 : 6;

                    e.Graphics.FillRectangle(Brushes.Black, SideRenderOffset + (Width - SideRenderOffset * 2) * (float)(TotalTime.TotalMilliseconds == 0 ? 0 : point.Time / TotalTime.TotalMilliseconds), my + 7 + 3, 1, pointSize);

                    if (point.Dirty)
                        e.Graphics.FillRectangle(Brushes.LimeGreen, SideRenderOffset + (Width - SideRenderOffset * 2) * (float)(TotalTime.TotalMilliseconds == 0 ? 0 : point.Time / TotalTime.TotalMilliseconds), my + 7 + 3 + pointSize, 1, i % 2 == 0 ? 6 : 8);
                }
            }

            var current = GetCurrentTimeStamp();

            if (current != null)
            {
                var pointSize = (GetNumber(current) - 1) % 2 == 0 ? 8 : 6;

                e.Graphics.FillRectangle(Brushes.White, SideRenderOffset + (Width - SideRenderOffset * 2) * (float)(TotalTime.TotalMilliseconds == 0 ? 0 : current.Time / TotalTime.TotalMilliseconds), my + 7 + 3 + (8 + 6), 1, 3);
            }

            _channel = Math.Max(0.5f, _channel - (float)delta * 4);

            if (_last != current)
            {
                _last = current;
                _channel = 0.9f;
            }
        }

        private void Timeline_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void Timeline_MouseDown(object sender, MouseEventArgs e)
        {
            var rect = new RectangleF(SideRenderOffset, Height / 2 - BarWidth / 2 - 1, (Width - SideRenderOffset * 2) + 1, BarWidth + 2);

            if (rect.Contains(e.Location))
            {
                _dragging = true;

                OnDragBegin.Invoke(this, null);
                Timeline_MouseMove(sender, e);
            }
        }

        private void Timeline_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;

            OnDragEnd?.Invoke(this, null);
        }

        private void Timeline_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                var pos = (e.Location.X - SideRenderOffset) / (Width - SideRenderOffset * 2f);

                pos = Math.Max(Math.Min(pos, 1), 0);

                OnDrag?.Invoke(this, pos);
            }
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

        public int GetNumber(TimeStamp ts)
        {
            return _points.IndexOf(ts) + 1;
        }

        public int GetCount()
        {
            return _points.Count;
        }

        public void Add(TimeStamp point)
        {
            lock (_points)
            {
                if (_points.Any(p => p.Time == point.Time))
                    _points.RemoveAll(p => p.Time == point.Time);

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