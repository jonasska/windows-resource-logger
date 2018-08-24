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

			timer1.Interval = Config.SamplingInterval * 1000;

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
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			logger.systemLog.nextValues();

			foreach (var ser in chart1.Series)
			{
				while (ser.Points.Count > Config.LivePoints)
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
			hScrollBar1.Value = 0;
			hScrollBar1.Maximum = datapoints;
			if (datapoints > hScrollBar1.LargeChange)
			{
				hScrollBar1.Value = datapoints - hScrollBar1.LargeChange;
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

			historySummary = logger.systemLog.getSelectedInstance().drawHistoryGraph(position, historyInteval, historyDatapopintThinning(historyInteval));

			historySummaryLabel.Text = historySummary;
			reevaluateHistoryAxes(position, historyInteval);
		}



		private void historyDatapointsUpLabel_Click(object sender, EventArgs e)
		{
			historyInteval = historyInteval.Add(TimeSpan.FromSeconds(historyInteval.TotalSeconds / 10));
			//if (nHistoryPoints < 950)
			//	nHistoryPoints += 50;
			//else
			//	nHistoryPoints = 1000;
			applyHistoryDatapoints();
		}

		private void historyDatapointsDownLabel_Click(object sender, EventArgs e)
		{
			historyInteval = historyInteval.Subtract(TimeSpan.FromSeconds(historyInteval.TotalSeconds / 11));
			//if (nHistoryPoints > 100)
			//	nHistoryPoints -= 50;
			//else
			//	nHistoryPoints = 50;
			applyHistoryDatapoints();
		}

		private void applyHistoryDatapoints()
		{
			historyDatapointsLabel.Text = historyInteval.ToString();
			hScrollBar1.LargeChange = (int)historyInteval.TotalSeconds;
			hScrollBar1.SmallChange = historyDatapopintThinning(historyInteval);
			if (hScrollBar1.Value > hScrollBar1.Maximum - hScrollBar1.LargeChange)
			{
				hScrollBar1.Value = hScrollBar1.Maximum - hScrollBar1.LargeChange + 1;
			}
			else
			{
				hScrollBar1_ValueChanged(null, null);
			}
		}

		private int historyDatapopintThinning(TimeSpan width)
		{
			
			int seconds = (int)width.TotalSeconds;
			return (seconds / Config.HistoryDatapointLines) + 1;
		}
	}
}
