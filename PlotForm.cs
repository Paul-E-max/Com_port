// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
// @File:       PlotForm.cs
// @Project:    DISCOVER_COM_port
// @Author:     Foster & Freeman Ltd
// @Created:    19.12.2024
//
// @Brief:      Real-time data plotting form for visualizing serial data.
//
// @Tools:      Visual Studio 2019, C#
//
// @Revision:
// 19.12.2024    Initial version.
//
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace COMport
{
    public partial class PlotForm : Form
    {
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        // Constants
        //
        const int DEFAULT_MAX_POINTS = 100;
        const int MAX_SERIES = 8;

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        // Member variables
        //
        private int maxPoints = DEFAULT_MAX_POINTS;
        private bool isPaused = false;
        private string dataBuffer = "";
        private int dataPointIndex = 0;

        // Regex to match decimal numbers (including negative and scientific notation)
        private static readonly Regex NumberRegex = new Regex(
            @"-?\d+\.?\d*(?:[eE][+-]?\d+)?",
            RegexOptions.Compiled);

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Initialize the plot form.
        /// </summary>
        public PlotForm()
        {
            InitializeComponent();
            InitializeChart();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Set up the chart with default series and appearance.
        /// </summary>
        private void InitializeChart()
        {
            DataChart.ChartAreas.Clear();
            DataChart.Series.Clear();
            DataChart.Legends.Clear();

            // Create chart area
            ChartArea chartArea = new ChartArea("MainArea");
            chartArea.BackColor = Color.Black;
            chartArea.AxisX.MajorGrid.LineColor = Color.DarkGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.DarkGray;
            chartArea.AxisX.LineColor = Color.White;
            chartArea.AxisY.LineColor = Color.White;
            chartArea.AxisX.LabelStyle.ForeColor = Color.White;
            chartArea.AxisY.LabelStyle.ForeColor = Color.White;
            chartArea.AxisX.Title = "Sample";
            chartArea.AxisY.Title = "Value";
            chartArea.AxisX.TitleForeColor = Color.White;
            chartArea.AxisY.TitleForeColor = Color.White;
            chartArea.AxisX.IsStartedFromZero = false;
            DataChart.ChartAreas.Add(chartArea);

            // Create legend
            Legend legend = new Legend("MainLegend");
            legend.BackColor = Color.Transparent;
            legend.ForeColor = Color.White;
            legend.Docking = Docking.Top;
            DataChart.Legends.Add(legend);

            // Create initial series (Series 1)
            AddSeries("Series 1", Color.LimeGreen);

            // Update max points from combo box
            UpdateMaxPoints();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Add a new data series to the chart.
        /// </summary>
        /// <param name="name">Series name</param>
        /// <param name="color">Line color</param>
        private void AddSeries(string name, Color color)
        {
            if (DataChart.Series.Count >= MAX_SERIES) return;

            Series series = new Series(name);
            series.ChartType = SeriesChartType.Line;
            series.BorderWidth = 2;
            series.Color = color;
            series.ChartArea = "MainArea";
            series.Legend = "MainLegend";
            DataChart.Series.Add(series);
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Process incoming text data and extract numeric values for plotting.
        /// Call this method from the main form when data is received.
        /// </summary>
        /// <param name="text">Raw text received from serial device</param>
        public void ProcessData(string text)
        {
            if (isPaused) return;

            // Buffer the incoming text
            dataBuffer += text;

            // Process complete lines
            while (dataBuffer.Contains("\n") || dataBuffer.Contains("\r"))
            {
                int lineEnd = dataBuffer.IndexOfAny(new char[] { '\n', '\r' });
                if (lineEnd < 0) break;

                string line = dataBuffer.Substring(0, lineEnd).Trim();
                dataBuffer = dataBuffer.Substring(lineEnd + 1);

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Extract all numbers from the line
                MatchCollection matches = NumberRegex.Matches(line);
                if (matches.Count > 0)
                {
                    // Ensure we have enough series for the data
                    while (DataChart.Series.Count < matches.Count && DataChart.Series.Count < MAX_SERIES)
                    {
                        Color[] colors = { Color.LimeGreen, Color.Cyan, Color.Yellow, Color.Orange, 
                                           Color.Magenta, Color.Red, Color.DeepSkyBlue, Color.Lime };
                        AddSeries("Series " + (DataChart.Series.Count + 1), 
                                  colors[DataChart.Series.Count % colors.Length]);
                    }

                    // Add data points
                    for (int i = 0; i < matches.Count && i < DataChart.Series.Count; i++)
                    {
                        if (double.TryParse(matches[i].Value, NumberStyles.Float, 
                                           CultureInfo.InvariantCulture, out double value))
                        {
                            AddDataPoint(i, value);
                        }
                    }
                    dataPointIndex++;
                }
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Add a data point to the specified series.
        /// </summary>
        /// <param name="seriesIndex">Index of the series</param>
        /// <param name="value">Value to add</param>
        private void AddDataPoint(int seriesIndex, double value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int, double>(AddDataPoint), seriesIndex, value);
                return;
            }

            if (seriesIndex >= DataChart.Series.Count) return;

            Series series = DataChart.Series[seriesIndex];
            series.Points.AddXY(dataPointIndex, value);

            // Remove old points to maintain scrolling window
            while (series.Points.Count > maxPoints)
            {
                series.Points.RemoveAt(0);
            }

            // Auto-scale X axis
            if (series.Points.Count > 0)
            {
                ChartArea area = DataChart.ChartAreas[0];
                double minX = series.Points[0].XValue;
                double maxX = series.Points[series.Points.Count - 1].XValue;
                area.AxisX.Minimum = minX;
                area.AxisX.Maximum = maxX;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Clear all data from the chart.
        /// </summary>
        public void ClearData()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearData));
                return;
            }

            foreach (Series series in DataChart.Series)
            {
                series.Points.Clear();
            }
            dataPointIndex = 0;
            dataBuffer = "";
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Handle Clear button click.
        /// </summary>
        private void ClearButton_Click(object sender, EventArgs e)
        {
            ClearData();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Handle Pause/Resume button click.
        /// </summary>
        private void PauseButton_Click(object sender, EventArgs e)
        {
            isPaused = !isPaused;
            PauseButton.Text = isPaused ? "Resume" : "Pause";
            PauseButton.BackColor = isPaused ? Color.Orange : SystemColors.Control;
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Handle max points selection change.
        /// </summary>
        private void MaxPointsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMaxPoints();
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Update the maximum number of points from the combo box.
        /// </summary>
        private void UpdateMaxPoints()
        {
            if (int.TryParse(MaxPointsComboBox.Text, out int points) && points > 0)
            {
                maxPoints = points;
            }
        }

        /// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// Prevent the form from being disposed when closed - just hide it.
        /// </summary>
        private void PlotForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                // Notify main form to uncheck the checkbox
                Program.comportform.UncheckPlotCheckBox();
            }
        }
    }
}
