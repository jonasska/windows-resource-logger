using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Windows;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;

namespace ResourceLogger
{
	public partial class Form1 : Form
	{
		Logger logger;
		
		private TimeSpan historyInteval;
		

		public Form1()
		{
			InitializeComponent();
			logger = new Logger();

			timer1.Interval = Properties.Settings.Default.SamplingInterval * 1000;

			historyInteval = TimeSpan.FromMinutes(10);
			historyDatapointsLabel.Text = historyInteval.ToString();
			hScrollBar1.LargeChange = (int)historyInteval.TotalSeconds;

			chart1.Series.Clear();
			chart2.Series.Clear();
			foreach (var i in logger.systemLog.instances)
			{
				foreach (var s in i.GetLiveSeries())
				{
					chart1.Series.Add(s);
				}
				foreach (var s in i.GetHistorySeries())
				{
					chart2.Series.Add(s);
				}
			}

			listBox1.Items.AddRange(logger.systemLog.getAllInstanceNames());
			listBox1.SetSelected(0, true);
			chart1.ChartAreas[0].AxisX.Enabled = AxisEnabled.False;
			chart2.ChartAreas[0].AxisX.Enabled = AxisEnabled.False;

			reloadHistoryButton_Click(null, null);
			fillSettingsTextFields();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			logger.systemLog.nextValues();

			foreach (var ser in chart1.Series)
			{
				while (ser.Points.Count > Properties.Settings.Default.LivePoints)
				{
					ser.Points.RemoveAt(0);
				}
			}

			reevaluateLiveYAxes();
		}

		private void reevaluateLiveYAxes()
		{
			if (chart1.Series[0].Points.Count > 0)
			{
				chart1.ChartAreas[0].AxisX.Minimum = chart1.Series[0].Points[0].XValue;
				chart1.ChartAreas[0].AxisX.Maximum = chart1.Series[0].Points[chart1.Series[0].Points.Count - 1].XValue;
			}

			int selected = logger.systemLog.getSelectedInstanceIndex();

			if (selected != -1)
			{
				AxisRange r = logger.systemLog.selectedInstance.GetLiveYAxisRange();
				chart1.ChartAreas[0].AxisY.Minimum = r.lower;
				chart1.ChartAreas[0].AxisY.Maximum = r.higher;
				chart1.ChartAreas[0].AxisY.MajorGrid.Interval = r.higher / 4.0;
				chart1.ChartAreas[0].AxisY.Interval = r.higher / 4.0;

			}
			else
			{
				Debug.WriteLine("not selected");
			}
		}

		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			for (int i = 0; i < listBox1.Items.Count; i++)
			{
				if (listBox1.GetSelected(i))
				{
					logger.systemLog.setSelectedInstance(listBox1.Items[i].ToString());
				}
			}
			reevaluateLiveYAxes();

			reloadHistoryButton_Click(null, null);
		}

		private void reloadHistoryButton_Click(object sender, EventArgs e)
		{
			int datapoints = logger.systemLog.getSelectedInstance().getnDatapoints();
			//hScrollBar1.Value = 0;
			hScrollBar1.Maximum = datapoints;
			if (datapoints > hScrollBar1.LargeChange)
			{
				hScrollBar1.Value = datapoints - hScrollBar1.LargeChange;
			}
            else
            {
                hScrollBar1.Value = 0;
            }

		}

		private void reevaluateHistoryAxes(DateTime position, TimeSpan width)
		{
			int selected = logger.systemLog.getSelectedInstanceIndex();

			if (selected != -1)
			{
				AxisRange r = logger.systemLog.selectedInstance.GetHistoryYAxisRange();
				chart2.ChartAreas[0].AxisY.Minimum = r.lower;
				chart2.ChartAreas[0].AxisY.Maximum = r.higher;
				chart2.ChartAreas[0].AxisY.MajorGrid.Interval = r.higher / 4.0;
				chart2.ChartAreas[0].AxisY.Interval = r.higher / 4.0;

				Series rangeSeries = new Series();
				rangeSeries.Points.AddXY(position, 0);
				rangeSeries.Points.AddXY(position + width, 0);
				chart2.ChartAreas[0].AxisX.Minimum = rangeSeries.Points[0].XValue;
				chart2.ChartAreas[0].AxisX.Maximum = rangeSeries.Points[1].XValue;
			}
			else
			{
				Debug.WriteLine("not selected");
			}
		}

		private void hScrollBar1_ValueChanged(object sender, EventArgs e)
		{
			Debug.WriteLine("scrollbar:{0}", hScrollBar1.Value);
			string historySummary = "";
			DateTime position = logger.systemLog.getSelectedInstance().GetFirstDatapointDateTime() +
								TimeSpan.FromSeconds(hScrollBar1.Value);

			historySummary = logger.systemLog.getSelectedInstance().drawHistoryGraph(position, historyInteval, historyDatapopintPointWidth(historyInteval));

			historySummaryLabel.Text = historySummary;
			reevaluateHistoryAxes(position, historyInteval);
		}



		private void historyDatapointsUpLabel_Click(object sender, EventArgs e)
		{
			historyInteval = historyInteval.Add(TimeSpan.FromSeconds(historyInteval.TotalSeconds / 10));
			applyHistoryDatapoints();
		}

		private void historyDatapointsDownLabel_Click(object sender, EventArgs e)
		{
			historyInteval = historyInteval.Subtract(TimeSpan.FromSeconds(historyInteval.TotalSeconds / 11));
			applyHistoryDatapoints();
		}

		private void applyHistoryDatapoints()
		{
			historyDatapointsLabel.Text = historyInteval.ToString();
			hScrollBar1.LargeChange = (int)historyInteval.TotalSeconds;
			hScrollBar1.SmallChange =  (int)(historyInteval.TotalSeconds/50);
			if (hScrollBar1.Value > hScrollBar1.Maximum - hScrollBar1.LargeChange)
			{
				hScrollBar1.Value = hScrollBar1.Maximum - hScrollBar1.LargeChange + 1;
			}
			else
			{
				hScrollBar1_ValueChanged(null, null);
			}
		}

		private TimeSpan historyDatapopintPointWidth(TimeSpan width)
		{
			return TimeSpan.FromTicks(width.Ticks / Properties.Settings.Default.HistoryPoints);
			//return TimeSpan.FromTicks(width.Ticks / Config.HistoryPoints);
		}

		private void systemLogDirectoryButton_Click(object sender, EventArgs e)
		{
			folderBrowserDialog1.ShowDialog();
			if (folderBrowserDialog1.SelectedPath != null)
			{
				var folder = folderBrowserDialog1.SelectedPath;
				textBox1.Text = folder;
			}
		}

		private void restoreDefaultSettingsButton_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.Reset();
			fillSettingsTextFields();
		}
		
		private void cancelSettingsButton_Click(object sender, EventArgs e)
		{
			fillSettingsTextFields();
		}

		private void saveSettingButton_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.SystemDirectory = textBox1.Text;

			Properties.Settings.Default.SamplingInterval = int.Parse(textBox2.Text);
			Properties.Settings.Default.DatapointsInOneFile = int.Parse(textBox3.Text);
			Properties.Settings.Default.LivePoints = int.Parse(textBox4.Text);
			Properties.Settings.Default.HistoryPoints = int.Parse(textBox5.Text);

			Properties.Settings.Default.Save();
		}

		private void fillSettingsTextFields()
		{
			textBox1.Text = Properties.Settings.Default.SystemDirectory;

			textBox2.Text = Properties.Settings.Default.SamplingInterval.ToString();
			textBox3.Text = Properties.Settings.Default.DatapointsInOneFile.ToString();
			textBox4.Text = Properties.Settings.Default.LivePoints.ToString();
			textBox5.Text = Properties.Settings.Default.HistoryPoints.ToString();
		}
	}
}
