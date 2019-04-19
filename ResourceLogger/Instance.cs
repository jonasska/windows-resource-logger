﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using SQLite;

namespace ResourceLogger
{
	public abstract class AnInstance
	{
		public string instanceName;
		public bool isSelected;
        public string dbPath { get; set; } = Properties.Settings.Default.SystemDirectory + "LocalResourceRecords.db";

        protected List<Series> liveSeries;
		protected List<Series> historySeries;
		//protected TimeSpan currentSpan;

		protected List<Datapoint> datapoints;

		public abstract void nextValues(TimeSpan span, SQLiteConnection db);

		public abstract AxisRange GetLiveYAxisRange();

		public abstract AxisRange GetHistoryYAxisRange();

		public abstract string drawHistoryGraph(DateTime start, TimeSpan width, TimeSpan pointWidth);

		protected AnInstance()
		{
			liveSeries = new List<Series>();
			historySeries = new List<Series>();
			datapoints = new List<Datapoint>();
			isSelected = false;  
        }

		public void SetSeriesVisible(bool setter)
		{
			foreach (var s in liveSeries)
			{
				s.Enabled = setter;
			}
			foreach (var s in historySeries)
			{
				s.Enabled = setter;
			}
		}

		public List<Series> GetLiveSeries()
		{
			return liveSeries;
		}
		public List<Series> GetHistorySeries()
		{
			return historySeries;
		}

        public abstract DateTime GetFirstDatapointDateTime();


		public int getnDatapoints()
		{
			TimeSpan dataspan = DateTime.Now - GetFirstDatapointDateTime();
			return (int)dataspan.TotalSeconds;
		}

	}



	public class MemoryInstance : AnInstance
	{
		private PerformanceCounter memCounter;
		private int totalMemory;
        public MemoryDatapoint currentDatapoint { get; private set; }

        public MemoryInstance(string name)
		{
			instanceName = name;
			memCounter = new PerformanceCounter("Memory", "Available MBytes");
			totalMemory = (int)GetTotalMemoryInMBytes();

			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.DarkOrchid });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.DarkOrchid });

            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<MemoryDatapoint>();
            }
        }

		static ulong GetTotalMemoryInMBytes()
		{
			return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1024 / 1024;
		}

		public override void nextValues(TimeSpan span, SQLiteConnection db)
		{
			int currentMemUsage = totalMemory - (int)memCounter.NextValue();

            currentDatapoint = new MemoryDatapoint(DateTime.Now, span, currentMemUsage);

			writeToSeries();
            saveInfoToDB(db);
		}

        private void saveInfoToDB(SQLiteConnection db)
        {
                db.Insert(currentDatapoint);
        }

        private void writeToSeries()
		{
			liveSeries[0].Points.AddXY(DateTime.Now, currentDatapoint.mem);
		}

		public override AxisRange GetLiveYAxisRange()
		{
			AxisRange r;
			r.lower = 0;
			r.higher = totalMemory;
			return r;
		}

		public override AxisRange GetHistoryYAxisRange()
		{
			return GetLiveYAxisRange();
		}

		public override string drawHistoryGraph(DateTime start, TimeSpan width, TimeSpan pointWidth)
		{
			historySeries[0].Points.Clear();

            getRelevantDatapoints(start,width,pointWidth);

			string historySummary = "";
			int count = 0;
			double total = 0;

			if (datapoints.Count > 0)
			{
				MemoryDatapoint datapoint = (MemoryDatapoint)datapoints[0];
				TimeSpan spanBetweenPoints = datapoint.time - start;
				if (spanBetweenPoints > datapoint.span + datapoint.span)
				{
					historySeries[0].Points.AddXY(start, 0.0);
					historySeries[0].Points.AddXY(datapoint.time - datapoint.span, 0.0);
				}
			}
			else
			{
				historySeries[0].Points.AddXY(start, 0.0);
				historySeries[0].Points.AddXY(start + width, 0.0);
			}

			DateTime prevTime = DateTime.Now;
			foreach (var point in datapoints)
			{
				MemoryDatapoint datapoint = (MemoryDatapoint)point;
				if (historySeries[0].Points.Count > 0)  // for marking 0 if measured large gap
				{
					TimeSpan spanBetweenPoints = datapoint.time - prevTime;
					if (spanBetweenPoints > datapoint.span + datapoint.span)
					{
						historySeries[0].Points.AddXY(prevTime + datapoint.span, 0.0);
						historySeries[0].Points.AddXY(datapoint.time - datapoint.span, 0.0);
					}
				}
				prevTime = datapoint.time;
				historySeries[0].Points.AddXY(datapoint.time, datapoint.mem);
				total += datapoint.mem;
				count++;
			}

			if (datapoints.Count > 0)
			{
				MemoryDatapoint datapoint = (MemoryDatapoint)datapoints[datapoints.Count - 1];
				TimeSpan spanBetweenPoints = start + width - datapoint.time;
				if (spanBetweenPoints > datapoint.span + datapoint.span)
				{
					historySeries[0].Points.AddXY(datapoint.time + datapoint.span, 0.0);
					historySeries[0].Points.AddXY(start + width, 0.0);
				}
			}

			if (datapoints.Count > 1) // position exists
			{
                DateTime firstTime = datapoints[0].time;
                DateTime lastTime = datapoints[datapoints.Count - 1].time;
				TimeSpan selectionSpan = lastTime - firstTime;
				historySummary = "Selection period: " + selectionSpan.ToString() + Environment.NewLine +
								 "Average Memory consumption: " + (total / count).ToString("#.000") + "MB";// + Environment.NewLine;
			}

			return historySummary;
		}

        public override DateTime GetFirstDatapointDateTime()
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                if (db.Find<MemoryDatapoint>(1) != null)
                {
                    var firstPoint = db.Get<MemoryDatapoint>(1);
                    return firstPoint.time;
                }

                return DateTime.Now;
            }
        }

        protected void getRelevantDatapoints(DateTime start, TimeSpan width, TimeSpan pointWidth)
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                datapoints.Clear();
                DateTime end = start + width;
                var query = db.Table<MemoryDatapoint>().Where(d => d.time >= start && d.time <= end).ToList<MemoryDatapoint>();
                datapoints.AddRange(query);
            }
        }

    }

	public class DiskInstance : AnInstance
	{
		public PerformanceCounter readCounter;
		public PerformanceCounter writeCounter;
        public DiskDatapoint currentDatapoint { get; private set; }


        public DiskInstance(string name)
		{
			instanceName = name + "disk usage";
			readCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", name);
			writeCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", name);

			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.GreenYellow, BorderDashStyle = ChartDashStyle.Dash });
			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Green });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.GreenYellow, BorderDashStyle = ChartDashStyle.Dash });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Green });

            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<DiskDatapoint>();
            }
        }

        private void saveInfoToDB(SQLiteConnection db)
        {
                db.Insert(currentDatapoint);
        }

        private void writeToSeries()
		{
			liveSeries[0].Points.AddXY(DateTime.Now, currentDatapoint.write / currentDatapoint.span.TotalSeconds);
			liveSeries[1].Points.AddXY(DateTime.Now, currentDatapoint.read / currentDatapoint.span.TotalSeconds);
		}

		public override void nextValues(TimeSpan span, SQLiteConnection db)
		{
            double currentWriteSpeed = writeCounter.NextValue();
            double currentReadSpeed = readCounter.NextValue();
            double currentDataWrite = currentWriteSpeed * span.TotalSeconds / 1024.0 / 1024.0;
            double currentDataRead = currentReadSpeed * span.TotalSeconds / 1024.0 / 1024.0;

            currentDatapoint = new DiskDatapoint(DateTime.Now, span, currentDataRead, currentDataWrite, instanceName);

			writeToSeries();
            saveInfoToDB(db);
		}

		public override AxisRange GetLiveYAxisRange()
		{
			AxisRange r;
			r.lower = 0;
			r.higher = 1;
			foreach (var point in liveSeries[0].Points)
			{
				while (point.YValues[0] > r.higher)
				{
					r.higher *= 2;
				}
			}
			foreach (var point in liveSeries[1].Points)
			{
				while (point.YValues[0] > r.higher)
				{
					r.higher *= 2;
				}
			}
			return r;
		}

		public override AxisRange GetHistoryYAxisRange()
		{
			AxisRange r;
			r.lower = 0;
			r.higher = 1;
			foreach (var point in historySeries[0].Points)
			{
				while (point.YValues[0] > r.higher)
				{
					r.higher *= 2;
				}
			}
			foreach (var point in historySeries[1].Points)
			{
				while (point.YValues[0] > r.higher)
				{
					r.higher *= 2;
				}
			}
			return r;
		}

		public override string drawHistoryGraph(DateTime start, TimeSpan width, TimeSpan pointWidth)
		{
			historySeries[0].Points.Clear();
			historySeries[1].Points.Clear();

			string historySummary = "";
			int count = 0;
			double totalRead = 0;
			double totalWrite = 0;

            getRelevantDatapoints(start, width, pointWidth);

            if (datapoints.Count > 0)
			{
				DiskDatapoint datapoint = (DiskDatapoint)datapoints[0];
				TimeSpan spanBetweenPoints = datapoint.time - start;
				if (spanBetweenPoints > datapoint.span + datapoint.span)
				{
					historySeries[0].Points.AddXY(start, 0.0);
					historySeries[0].Points.AddXY(datapoint.time - datapoint.span, 0.0);
					historySeries[1].Points.AddXY(start, 0.0);
					historySeries[1].Points.AddXY(datapoint.time - datapoint.span, 0.0);
				}
			}
			else
			{
				historySeries[0].Points.AddXY(start, 0.0);
				historySeries[0].Points.AddXY(start + width, 0.0);
				historySeries[1].Points.AddXY(start, 0.0);
				historySeries[1].Points.AddXY(start + width, 0.0);
			}

			DateTime prevTime = DateTime.Now;
			foreach (var point in datapoints)
			{
				DiskDatapoint datapoint = (DiskDatapoint)point;
				if (historySeries[0].Points.Count > 0)
				{
					TimeSpan spanBetweenPoints = datapoint.time - prevTime;
					if (spanBetweenPoints > datapoint.span + datapoint.span)
					{
						historySeries[0].Points.AddXY(prevTime + datapoint.span, 0.0);
						historySeries[0].Points.AddXY(datapoint.time - datapoint.span, 0.0);
						historySeries[1].Points.AddXY(prevTime + datapoint.span, 0.0);
						historySeries[1].Points.AddXY(datapoint.time - datapoint.span, 0.0);
					}
				}
				prevTime = datapoint.time;
				historySeries[0].Points.AddXY(datapoint.time, datapoint.write / datapoint.span.TotalSeconds);
				historySeries[1].Points.AddXY(datapoint.time, datapoint.read / datapoint.span.TotalSeconds);
				totalRead += datapoint.read;
				totalWrite += datapoint.write;
				count++;//?? eh
			}

			if (datapoints.Count > 0)
			{
				DiskDatapoint datapoint = (DiskDatapoint)datapoints[datapoints.Count - 1];
				TimeSpan spanBetweenPoints = start + width - datapoint.time;
				if (spanBetweenPoints > datapoint.span + datapoint.span)
				{
					historySeries[0].Points.AddXY(datapoint.time + datapoint.span, 0.0);
					historySeries[0].Points.AddXY(start + width, 0.0);
					historySeries[1].Points.AddXY(datapoint.time + datapoint.span, 0.0);
					historySeries[1].Points.AddXY(start + width, 0.0);
				}
			}

			if (datapoints.Count > 1) // position exists
			{
                DateTime firstTime = datapoints[0].time;
                DateTime lastTime = datapoints[datapoints.Count - 1].time;
                TimeSpan selectionSpan = lastTime - firstTime;
				historySummary = "Selection period: " + selectionSpan.ToString() + Environment.NewLine +
				                 "Total disk read: " + (totalRead / 1024).ToString("#.000") + "GB" + Environment.NewLine +
				                 "Total disk write: " + (totalWrite / 1024).ToString("#.000") + "GB" + Environment.NewLine +
				                 "Average disk read speed: " + (totalRead / selectionSpan.TotalSeconds).ToString("#.000") + "MB/s" + Environment.NewLine +
				                 "Average disk write speed: " + (totalWrite / selectionSpan.TotalSeconds).ToString("#.000") + "MB/s";// + Environment.NewLine +
			}

			return historySummary;
		}
        public override DateTime GetFirstDatapointDateTime()
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                if (db.Find<DiskDatapoint>(1) != null)
                {
                    var firstPoint = db.Get<DiskDatapoint>(1);
                    return firstPoint.time;
                }

                return DateTime.Now;
            }
        }
        protected void getRelevantDatapoints(DateTime start, TimeSpan width, TimeSpan pointWidth)
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                datapoints.Clear();
                DateTime end = start + width;
                var query = db.Table<DiskDatapoint>().Where(d => d.time >= start && d.time <= end && d.instanceName == instanceName).ToList<DiskDatapoint>();
                datapoints.AddRange(query);
            }
        }
    };

	public class NetworkInstance : AnInstance
	{
		public PerformanceCounter readCounter;
		public PerformanceCounter writeCounter;
        public NetworkDatapoint currentDatapoint { get; private set; }

        public NetworkInstance(string name)
		{
			instanceName = name + "network usage";
			readCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", name);
			writeCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", name);

			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Salmon, BorderDashStyle = ChartDashStyle.Dash });
			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Red });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Salmon, BorderDashStyle = ChartDashStyle.Dash });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Red });


            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<NetworkDatapoint>();
            }
        }

        private void saveInfoToDB(SQLiteConnection db)
        {
                db.Insert(currentDatapoint);
        }

        private void writeToSeries()
		{
			liveSeries[0].Points.AddXY(DateTime.Now, currentDatapoint.write / currentDatapoint.span.TotalSeconds * 8);
			liveSeries[1].Points.AddXY(DateTime.Now, currentDatapoint.read / currentDatapoint.span.TotalSeconds * 8);
		}

		public override void nextValues(TimeSpan span, SQLiteConnection db)
		{
			double currentWriteSpeed = writeCounter.NextValue();
            double currentReadSpeed = readCounter.NextValue();
            double currentDataWrite = currentWriteSpeed * span.TotalSeconds / 1024.0 / 1024.0;
            double currentDataRead = currentReadSpeed * span.TotalSeconds / 1024.0 / 1024.0;

            currentDatapoint = new NetworkDatapoint(DateTime.Now,span,currentDataRead,currentDataWrite,instanceName);

			writeToSeries();
            saveInfoToDB(db);
        }

		public override AxisRange GetLiveYAxisRange()
		{
			AxisRange r;
			r.lower = 0;
			r.higher = 1;
			foreach (var point in liveSeries[0].Points)
			{
				while (point.YValues[0] > r.higher)
				{
					r.higher *= 2;
				}
			}
			foreach (var point in liveSeries[1].Points)
			{
				while (point.YValues[0] > r.higher)
				{
					r.higher *= 2;
				}
			}
			return r;
		}

		public override AxisRange GetHistoryYAxisRange()
		{
			AxisRange r;
			r.lower = 0;
			r.higher = 1;
			foreach (var point in historySeries[0].Points)
			{
				while (point.YValues[0] > r.higher)
				{
					r.higher *= 2;
				}
			}
			foreach (var point in historySeries[1].Points)
			{
				while (point.YValues[0] > r.higher)
				{
					r.higher *= 2;
				}
			}
			return r;
		}

		public override string drawHistoryGraph(DateTime start, TimeSpan width, TimeSpan pointWidth)
		{
			historySeries[0].Points.Clear();
			historySeries[1].Points.Clear();

			string historySummary = "";
			int count = 0;
			double totalRead = 0;
			double totalWrite = 0;

            getRelevantDatapoints(start, width, pointWidth);

            if (datapoints.Count > 0)
			{
				NetworkDatapoint datapoint = (NetworkDatapoint)datapoints[0];
				TimeSpan spanBetweenPoints = datapoint.time - start;
				if (spanBetweenPoints > datapoint.span + datapoint.span)
				{
					historySeries[0].Points.AddXY(start, 0.0);
					historySeries[0].Points.AddXY(datapoint.time - datapoint.span, 0.0);
					historySeries[1].Points.AddXY(start, 0.0);
					historySeries[1].Points.AddXY(datapoint.time - datapoint.span, 0.0);
				}
			}
			else
			{
				historySeries[0].Points.AddXY(start, 0.0);
				historySeries[0].Points.AddXY(start + width, 0.0);
				historySeries[1].Points.AddXY(start, 0.0);
				historySeries[1].Points.AddXY(start + width, 0.0);
			}

			DateTime prevTime = DateTime.Now;
			foreach (var point in datapoints)
			{
				NetworkDatapoint datapoint = (NetworkDatapoint)point;
				if (historySeries[0].Points.Count > 0)
				{
					TimeSpan spanBetweenPoints = datapoint.time - prevTime;
					if (spanBetweenPoints > datapoint.span + datapoint.span)
					{
						historySeries[0].Points.AddXY(prevTime + datapoint.span, 0.0);
						historySeries[0].Points.AddXY(datapoint.time - datapoint.span, 0.0);
						historySeries[1].Points.AddXY(prevTime + datapoint.span, 0.0);
						historySeries[1].Points.AddXY(datapoint.time - datapoint.span, 0.0);
					}
				}
				prevTime = datapoint.time;
				historySeries[0].Points.AddXY(datapoint.time, datapoint.write / datapoint.span.TotalSeconds*8);
				historySeries[1].Points.AddXY(datapoint.time, datapoint.read / datapoint.span.TotalSeconds*8);
				totalRead += datapoint.read;
				totalWrite += datapoint.write;
				count++;//?? eh
			}

			if (datapoints.Count > 0)
			{
				NetworkDatapoint datapoint = (NetworkDatapoint)datapoints[datapoints.Count-1];
				TimeSpan spanBetweenPoints = start + width - datapoint.time;
				if (spanBetweenPoints > datapoint.span + datapoint.span)
				{
					historySeries[0].Points.AddXY(datapoint.time + datapoint.span, 0.0);
					historySeries[0].Points.AddXY(start + width, 0.0);
					historySeries[1].Points.AddXY(datapoint.time + datapoint.span, 0.0);
					historySeries[1].Points.AddXY(start + width, 0.0);
				}
			}

			if (datapoints.Count > 1) // position exists
			{
                DateTime firstTime = datapoints[0].time;
                DateTime lastTime = datapoints[datapoints.Count - 1].time;
                TimeSpan selectionSpan = lastTime - firstTime;
				historySummary = "Selection period: " + selectionSpan.ToString() + Environment.NewLine +
				                 "Total network received: " + (totalRead / 1024).ToString("#.000") + "GB" + Environment.NewLine +
				                 "Total network sent: " + (totalWrite / 1024).ToString("#.000") + "GB" + Environment.NewLine +
				                 "Average network received speed: " + (totalRead / selectionSpan.TotalSeconds * 8).ToString("#.000") + "Mbps" + Environment.NewLine +
				                 "Average network sent speed: " + (totalWrite / selectionSpan.TotalSeconds * 8).ToString("#.000") + "Mbps";// + Environment.NewLine +
			}

			return historySummary;
		}
        public override DateTime GetFirstDatapointDateTime()
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                if (db.Find<NetworkDatapoint>(1) != null)
                {
                    var firstPoint = db.Get<NetworkDatapoint>(1);
                    return firstPoint.time;
                }

                return DateTime.Now;
            }
        }
        protected void getRelevantDatapoints(DateTime start, TimeSpan width, TimeSpan pointWidth)
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                datapoints.Clear();
                DateTime end = start + width;
                var query = db.Table<NetworkDatapoint>().Where(d => d.time >= start && d.time <= end && d.instanceName == instanceName).ToList<NetworkDatapoint>();
                datapoints.AddRange(query);
            }
        }
    };

	public class CpuInstance : AnInstance
	{
		PerformanceCounter totalCpuTimeCount;
        public CPUDatapoint currentDatapoint { get; private set; }

        public CpuInstance(string name)
		{
			instanceName = name;
			totalCpuTimeCount = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");

			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.DodgerBlue });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.DodgerBlue });

            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<CPUDatapoint>();
            }
        }

		private void writeToSeries()
		{
			liveSeries[0].Points.AddXY(DateTime.Now, currentDatapoint.usage);

		}

		public override void nextValues(TimeSpan span, SQLiteConnection db)
		{
			float currentCpuUsage = totalCpuTimeCount.NextValue();
			long cpuElapsed = (long)(span.Ticks * currentCpuUsage / 100);
			TimeSpan currentcpuSpan = new TimeSpan(cpuElapsed);

            currentDatapoint = new CPUDatapoint(DateTime.Now, currentcpuSpan, span);

			writeToSeries();
            saveInfoToDB(db);
        }


        private void saveInfoToDB(SQLiteConnection db)
        {
                db.Insert(currentDatapoint);
        }


		public override AxisRange GetLiveYAxisRange()
		{
			AxisRange r;
			r.lower = 0;
			r.higher = 100;
			return r;
		}

		public override AxisRange GetHistoryYAxisRange()
		{
			return GetLiveYAxisRange();
		}

		public override string drawHistoryGraph(DateTime start, TimeSpan width, TimeSpan pointWidth)
		{
			historySeries[0].Points.Clear();

            getRelevantDatapoints(start, width, pointWidth);

            string historySummary = "";
			int count = 0;
			double total = 0;


			if (datapoints.Count > 0)
			{
				CPUDatapoint datapoint = (CPUDatapoint)datapoints[0];
				TimeSpan spanBetweenPoints = datapoint.time - start;
				if (spanBetweenPoints > datapoint.span + datapoint.span)
				{
					historySeries[0].Points.AddXY(start, 0.0);
					historySeries[0].Points.AddXY(datapoint.time - datapoint.span, 0.0);
				}
			}
			else
			{
				historySeries[0].Points.AddXY(start, 0.0);
				historySeries[0].Points.AddXY(start + width, 0.0);
			}

			DateTime prevTime = DateTime.Now;
			foreach (var point in datapoints)
			{
				CPUDatapoint datapoint = (CPUDatapoint)point;
				if (historySeries[0].Points.Count > 0)
				{
					TimeSpan spanBetweenPoints = datapoint.time - prevTime;
					if (spanBetweenPoints > datapoint.span + datapoint.span)
					{
						DateTime first = prevTime + datapoint.span;
						DateTime second = datapoint.time - datapoint.span;
						historySeries[0].Points.AddXY(first, 0);
						historySeries[0].Points.AddXY(second, 0);
					}
				}
				prevTime = datapoint.time;
				historySeries[0].Points.AddXY(datapoint.time, datapoint.usage);
				total += datapoint.usage;
				count++;
			}

			if (datapoints.Count > 0)
			{
				CPUDatapoint datapoint = (CPUDatapoint)datapoints[datapoints.Count-1];
				TimeSpan spanBetweenPoints = start + width - datapoint.time;
				if (spanBetweenPoints > datapoint.span + datapoint.span)
				{
					historySeries[0].Points.AddXY(datapoint.time + datapoint.span, 0.0);
					historySeries[0].Points.AddXY(start + width, 0.0);
				}
			}

			if (datapoints.Count > 1) // position exists
            {
                DateTime firstTime = datapoints[0].time;
                DateTime lastTime = datapoints[datapoints.Count - 1].time;

                TimeSpan selectionSpan = lastTime - firstTime;
				historySummary = "Selection period: " + selectionSpan.ToString() + Environment.NewLine +
				                 "Average CPU usage: " + (total / count).ToString("#.000") + "%";// + Environment.NewLine;
			}

			return historySummary;
		}
        public override DateTime GetFirstDatapointDateTime()
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                if (db.Find<CPUDatapoint>(1)!=null)
                {
                    var firstPoint = db.Get<CPUDatapoint>(1);
                    return firstPoint.time;
                }

                return DateTime.Now;
            }
        }
        protected void getRelevantDatapoints(DateTime start, TimeSpan width, TimeSpan pointWidth)
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                datapoints.Clear();
                //var datapoints = db.Query<CPUDatapoint>("SELECT * FROM CPUDatapoint WHERE time...");
                DateTime end = start + width;
                var query = db.Table<CPUDatapoint>().Where(d => d.time >= start && d.time <=end).ToList<CPUDatapoint>();
                datapoints.AddRange(query);
            }
        }
	}
}
