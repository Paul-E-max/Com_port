// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
// @File:       PowerMeterChart.cs
// @Project:    ThorlabsPowerMeterApp
// @Author:     Thorlabs Power Meter Integration
// @Created:    19.12.2024
//
// @Brief:      Custom GDI+ chart control for real-time power measurement display.
//              Provides scrolling time-series visualization with auto-scaling.
//
// @Revision:
// 19.12.2024 - v1.1.0 - Initial version with real-time charting.
//
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ThorlabsPowerMeterApp.Controls
{
    /// <summary>
    /// Custom chart control for real-time power measurement visualization.
    /// </summary>
    public class PowerMeterChart : Control
    {
        #region Private Fields

        private readonly List<DataPoint> _dataPoints = new List<DataPoint>();
        private readonly object _dataLock = new object();
        
        private double _minValue = 0;
        private double _maxValue = 1;
        private bool _autoScale = true;
        private int _historySeconds = 30;
        private string _unit = "µW";

        // Colors for dark theme
        private readonly Color _backgroundColor = Color.FromArgb(30, 30, 30);
        private readonly Color _gridColor = Color.FromArgb(60, 60, 60);
        private readonly Color _axisColor = Color.FromArgb(100, 100, 100);
        private readonly Color _lineColor = Color.FromArgb(0, 200, 255);
        private readonly Color _fillColor = Color.FromArgb(40, 0, 200, 255);
        private readonly Color _textColor = Color.FromArgb(200, 200, 200);
        private readonly Color _minLineColor = Color.FromArgb(100, 255, 100);
        private readonly Color _maxLineColor = Color.FromArgb(255, 100, 100);
        private readonly Color _avgLineColor = Color.FromArgb(255, 200, 100);

        #endregion

        #region Data Structure

        private struct DataPoint
        {
            public DateTime Time;
            public double Value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// History duration in seconds (30, 60, 300)
        /// </summary>
        public int HistorySeconds
        {
            get => _historySeconds;
            set
            {
                _historySeconds = Math.Max(10, Math.Min(600, value));
                Invalidate();
            }
        }

        /// <summary>
        /// Enable auto-scaling of Y-axis
        /// </summary>
        public bool AutoScale
        {
            get => _autoScale;
            set
            {
                _autoScale = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Unit label for Y-axis
        /// </summary>
        public string Unit
        {
            get => _unit;
            set
            {
                _unit = value ?? "";
                Invalidate();
            }
        }

        /// <summary>
        /// Current data point count
        /// </summary>
        public int DataCount
        {
            get
            {
                lock (_dataLock)
                {
                    return _dataPoints.Count;
                }
            }
        }

        #endregion

        #region Constructor

        public PowerMeterChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            
            BackColor = _backgroundColor;
            MinimumSize = new Size(200, 100);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a new data point to the chart
        /// </summary>
        /// <param name="value">Power value</param>
        /// <param name="unit">Unit string (e.g., "µW", "mW")</param>
        public void AddDataPoint(double value, string unit = null)
        {
            if (unit != null)
            {
                _unit = unit;
            }

            lock (_dataLock)
            {
                _dataPoints.Add(new DataPoint
                {
                    Time = DateTime.Now,
                    Value = value
                });

                // Remove old data points
                DateTime cutoff = DateTime.Now.AddSeconds(-_historySeconds - 5);
                _dataPoints.RemoveAll(p => p.Time < cutoff);
            }

            // Update on UI thread
            if (InvokeRequired)
            {
                BeginInvoke(new Action(Invalidate));
            }
            else
            {
                Invalidate();
            }
        }

        /// <summary>
        /// Clear all data points
        /// </summary>
        public void ClearData()
        {
            lock (_dataLock)
            {
                _dataPoints.Clear();
            }
            Invalidate();
        }

        /// <summary>
        /// Get statistics for current visible data
        /// </summary>
        public (double min, double max, double avg) GetStatistics()
        {
            lock (_dataLock)
            {
                if (_dataPoints.Count == 0)
                    return (0, 0, 0);

                DateTime cutoff = DateTime.Now.AddSeconds(-_historySeconds);
                var visiblePoints = _dataPoints.FindAll(p => p.Time >= cutoff);

                if (visiblePoints.Count == 0)
                    return (0, 0, 0);

                double min = double.MaxValue;
                double max = double.MinValue;
                double sum = 0;

                foreach (var p in visiblePoints)
                {
                    if (p.Value < min) min = p.Value;
                    if (p.Value > max) max = p.Value;
                    sum += p.Value;
                }

                return (min, max, sum / visiblePoints.Count);
            }
        }

        #endregion

        #region Painting

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Chart area (with margins for labels)
            int leftMargin = 60;
            int rightMargin = 10;
            int topMargin = 10;
            int bottomMargin = 25;

            Rectangle chartArea = new Rectangle(
                leftMargin, topMargin,
                Width - leftMargin - rightMargin,
                Height - topMargin - bottomMargin);

            // Background
            using (SolidBrush bgBrush = new SolidBrush(_backgroundColor))
            {
                g.FillRectangle(bgBrush, ClientRectangle);
            }

            // Get visible data
            List<DataPoint> visibleData;
            DateTime now = DateTime.Now;
            DateTime startTime = now.AddSeconds(-_historySeconds);

            lock (_dataLock)
            {
                visibleData = _dataPoints.FindAll(p => p.Time >= startTime);
            }

            // Calculate Y-axis range
            CalculateYRange(visibleData);

            // Draw grid
            DrawGrid(g, chartArea, startTime, now);

            // Draw chart area border
            using (Pen borderPen = new Pen(_axisColor))
            {
                g.DrawRectangle(borderPen, chartArea);
            }

            // Draw data line
            if (visibleData.Count > 1)
            {
                DrawDataLine(g, chartArea, visibleData, startTime, now);
            }

            // Draw statistics lines
            DrawStatisticsLines(g, chartArea, visibleData);

            // Draw labels
            DrawLabels(g, chartArea, startTime, now);
        }

        private void CalculateYRange(List<DataPoint> data)
        {
            if (!_autoScale || data.Count == 0)
                return;

            double min = double.MaxValue;
            double max = double.MinValue;

            foreach (var p in data)
            {
                if (p.Value < min) min = p.Value;
                if (p.Value > max) max = p.Value;
            }

            // Add 10% padding
            double range = max - min;
            if (range < 0.0001) range = max * 0.1;
            if (range < 0.0001) range = 1;

            _minValue = min - range * 0.1;
            _maxValue = max + range * 0.1;

            if (_minValue < 0) _minValue = 0;
        }

        private void DrawGrid(Graphics g, Rectangle area, DateTime startTime, DateTime endTime)
        {
            using (Pen gridPen = new Pen(_gridColor) { DashStyle = DashStyle.Dot })
            {
                // Horizontal grid lines (5 lines)
                for (int i = 1; i < 5; i++)
                {
                    int y = area.Top + (area.Height * i / 5);
                    g.DrawLine(gridPen, area.Left, y, area.Right, y);
                }

                // Vertical grid lines (every 5 seconds for 30s, etc.)
                int gridInterval = _historySeconds <= 30 ? 5 : (_historySeconds <= 60 ? 10 : 60);
                double totalSeconds = (endTime - startTime).TotalSeconds;

                for (int s = gridInterval; s < totalSeconds; s += gridInterval)
                {
                    int x = area.Left + (int)(area.Width * s / totalSeconds);
                    g.DrawLine(gridPen, x, area.Top, x, area.Bottom);
                }
            }
        }

        private void DrawDataLine(Graphics g, Rectangle area, List<DataPoint> data, 
                                   DateTime startTime, DateTime endTime)
        {
            if (data.Count < 2) return;

            double totalSeconds = (endTime - startTime).TotalSeconds;
            double valueRange = _maxValue - _minValue;
            if (valueRange < 0.0001) valueRange = 1;

            PointF[] points = new PointF[data.Count];

            for (int i = 0; i < data.Count; i++)
            {
                double timeFraction = (data[i].Time - startTime).TotalSeconds / totalSeconds;
                double valueFraction = (data[i].Value - _minValue) / valueRange;

                points[i] = new PointF(
                    area.Left + (float)(area.Width * timeFraction),
                    area.Bottom - (float)(area.Height * valueFraction));
            }

            // Draw filled area
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddLines(points);
                path.AddLine(points[points.Length - 1], new PointF(points[points.Length - 1].X, area.Bottom));
                path.AddLine(new PointF(points[points.Length - 1].X, area.Bottom), new PointF(points[0].X, area.Bottom));
                path.CloseFigure();

                using (SolidBrush fillBrush = new SolidBrush(_fillColor))
                {
                    g.FillPath(fillBrush, path);
                }
            }

            // Draw line
            using (Pen linePen = new Pen(_lineColor, 2f))
            {
                g.DrawLines(linePen, points);
            }
        }

        private void DrawStatisticsLines(Graphics g, Rectangle area, List<DataPoint> data)
        {
            if (data.Count == 0) return;

            var (min, max, avg) = GetStatistics();
            double valueRange = _maxValue - _minValue;
            if (valueRange < 0.0001) return;

            // Min line
            int minY = area.Bottom - (int)(area.Height * (min - _minValue) / valueRange);
            using (Pen minPen = new Pen(_minLineColor) { DashStyle = DashStyle.Dash })
            {
                g.DrawLine(minPen, area.Left, minY, area.Right, minY);
            }

            // Max line
            int maxY = area.Bottom - (int)(area.Height * (max - _minValue) / valueRange);
            using (Pen maxPen = new Pen(_maxLineColor) { DashStyle = DashStyle.Dash })
            {
                g.DrawLine(maxPen, area.Left, maxY, area.Right, maxY);
            }

            // Avg line
            int avgY = area.Bottom - (int)(area.Height * (avg - _minValue) / valueRange);
            using (Pen avgPen = new Pen(_avgLineColor) { DashStyle = DashStyle.Dash })
            {
                g.DrawLine(avgPen, area.Left, avgY, area.Right, avgY);
            }
        }

        private void DrawLabels(Graphics g, Rectangle area, DateTime startTime, DateTime endTime)
        {
            using (Font font = new Font("Segoe UI", 8f))
            using (SolidBrush textBrush = new SolidBrush(_textColor))
            {
                // Y-axis labels
                string maxLabel = FormatValue(_maxValue);
                string minLabel = FormatValue(_minValue);
                string midLabel = FormatValue((_maxValue + _minValue) / 2);

                g.DrawString(maxLabel, font, textBrush, 2, area.Top - 5);
                g.DrawString(midLabel, font, textBrush, 2, area.Top + area.Height / 2 - 5);
                g.DrawString(minLabel, font, textBrush, 2, area.Bottom - 10);

                // X-axis labels
                string startLabel = $"-{_historySeconds}s";
                string endLabel = "Now";
                g.DrawString(startLabel, font, textBrush, area.Left, area.Bottom + 3);
                
                SizeF endSize = g.MeasureString(endLabel, font);
                g.DrawString(endLabel, font, textBrush, area.Right - endSize.Width, area.Bottom + 3);

                // Unit label
                g.DrawString(_unit, font, textBrush, 2, area.Top + area.Height / 2 - 20);
            }
        }

        private string FormatValue(double value)
        {
            if (Math.Abs(value) >= 1000)
                return $"{value:F0}";
            else if (Math.Abs(value) >= 100)
                return $"{value:F1}";
            else if (Math.Abs(value) >= 10)
                return $"{value:F2}";
            else
                return $"{value:F3}";
        }

        #endregion
    }
}
