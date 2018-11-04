using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Blox_Saber_Editor
{
    public partial class TimeLine : UserControl
    {
        public event EventHandler OnActiveNodeChanged;

        private float _brightness = 0.65f;

        private TimeSpan _currentTime = TimeSpan.Zero;

        public TimeSpan Length = TimeSpan.FromSeconds(100000);

        public TimeSpan CurrentTime
        {
            get => _currentTime;

            set
            {
                var activeNode = GetActiveNode();

                _currentTime = value;

                if (activeNode != GetActiveNode())
                {
                    OnActiveNodeChanged?.Invoke(this, null);

                    _brightness = 0.85f;
                }
            }
        }

        public int BarHeight { get; set; } = 5;

        private readonly List<TimeNode> _points = new List<TimeNode>();

        private bool _mouseDown;

        public TimeLine()
        {
            InitializeComponent();

            BackColor = Color.FromArgb((int)_brightness * 255, (int)_brightness * 255, (int)_brightness * 255);
        }

        private void TimeLine_Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            //e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var channel = (int)(_brightness * 255);
            var color = Color.FromArgb(channel, channel, channel);

            e.Graphics.FillRectangle(new SolidBrush(color), ClientRectangle);

            _brightness = Math.Max(_brightness * 0.95f, 0.65f);

            float progress = (float)(CurrentTime.TotalMilliseconds / Length.TotalMilliseconds);

            float x = (Width - 20) * progress;
            float y = Height / 2f;

            e.Graphics.FillRectangle(Brushes.Black, 10 - 1, y - BarHeight / 2f - 1, Width - 20 + 2, BarHeight + 2);
            e.Graphics.FillRectangle(Brushes.Red, 10, y - BarHeight / 2f, x, BarHeight);

            DrawPoints(e.Graphics);

            e.Graphics.FillRectangle(Brushes.White, 10 + x, y - 8, 1, 16);
        }

        private void DrawPoints(Graphics g)
        {
            lock (_points)
            {
                foreach (var point in _points)
                {
                    float progress = (float)(point.Time / Length.TotalMilliseconds);

                    float x = (Width - 20) * progress;
                    float y = Height / 2f;

                    g.FillRectangle(Brushes.White, 10 + x, y + BarHeight + 5, 1, 8);

                    if (point.Tampered)
                        g.FillRectangle(Brushes.LawnGreen, 10 + x, y + BarHeight + 5 + 8, 1, 3);
                }
            }
        }

        public void AddPoint(TimeNode t)
        {
            lock (_points)
            {
                _points.Add(t);
            }

            Invalidate();
        }

        public TimeNode GetPreviousNode()
        {
            lock (_points)
            {
                var l = GetPoints();

                TimeNode node = null;

                for (var index = 0; index < l.Count; index++)
                {
                    var n = l[index];

                    if (n.Time >= (int)CurrentTime.TotalMilliseconds)
                    {
                        break;
                    }

                    node = n;
                }

                return node ?? GetActiveNode();

                //.LastOrDefault(p => p.Time < (int)CurrentTime.TotalMilliseconds) ?? GetActiveNode();
            }
        }

        public TimeNode GetNextNode()
        {
            lock (_points)
            {
                var l = GetPoints();

                var ind = Math.Min(l.IndexOf(GetActiveNode()) + 1, l.Count - 1);

                return l[ind];

                TimeNode node = null;

                for (var index = 0; index < l.Count; index++)
                {
                    var n = l[index];

                    if (n.Time > (int)CurrentTime.TotalMilliseconds)
                    {
                        node = n;
                        break;
                    }
                }

                return node ?? l.Last();

                //return GetPoints().FirstOrDefault(p => p.Time > (int)CurrentTime.TotalMilliseconds) ?? GetLastNode();
            }
        }

        public TimeNode GetActiveNode()
        {
            lock (_points)
            {
                var l = GetPoints();

                TimeNode node = null;

                foreach (var n in l)
                {
                    if (n.Time < (int) CurrentTime.TotalMilliseconds)
                    {
                        node = n;
                    }
                    else
                    {
                        break;
                    }
                }

                return node ?? (l.Count > 0 ? l.First() : null);
            }
        }

        public void ClearPoints()
        {
            lock (_points)
            {
                _points.Clear();
            }

            Invalidate();
        }

        public List<TimeNode> GetPoints()
        {
            List<TimeNode> points;

            lock (_points)
            {
                points = new List<TimeNode>(_points).OrderBy(p => p.Time).ToList();
            }

            return points;
        }

        private void TimeLine_MouseDown(object sender, MouseEventArgs e)
        {
            float y = Height / 2f;

            var rect = new RectangleF(10 - 1, y - BarHeight / 2f - 1, Width - 20 + 2, BarHeight + 2);

            if (rect.Contains(e.Location))
                _mouseDown = true;
        }

        private void TimeLine_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
        }

        private void TimeLine_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDown)
            {
                float progress = Math.Max(Math.Min((float)(e.X - 10) / (Width - 20), 1), 0);

                float x = (Width - 20) * progress;
            }
        }

        private void TimeLine_MouseLeave(object sender, EventArgs e)
        {
            _mouseDown = false;
        }
    }

    class TimeLineDragEventArgs : EventArgs
    {
        public float Precentage;
    }
}