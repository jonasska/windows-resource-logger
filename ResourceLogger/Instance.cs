using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
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

        protected AnInstance()
        {
            liveSeries = new List<Series>();
            historySeries = new List<Series>();
            isSelected = false;
        }

        public abstract void nextValues(TimeSpan span, SQLiteConnection db);

        public abstract AxisRange GetLiveYAxisRange();
        public abstract AxisRange GetHistoryYAxisRange();
        public abstract string drawHistoryGraph(DateTime start, TimeSpan width, TimeSpan pointWidth);

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
	public abstract class AnInstanceWithDatapoint<TDataPointType> : AnInstance where TDataPointType : Datapoint , new()  
    {
        public TDataPointType currentDatapoint;
        protected List<TDataPointType> queriedDatapoints;
        protected List<TDataPointType> compressedDatapoints;
        protected DateTime loadedDatapointStart;
        protected DateTime loadedDatapointEnd;

        public AnInstanceWithDatapoint()
        {
            queriedDatapoints = new List<TDataPointType>();
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<TDataPointType>();
            }
        }
        public override DateTime GetFirstDatapointDateTime()
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                if (db.Find<TDataPointType>(1) != null)
                {
                    var firstPoint = db.Get<TDataPointType>(1);
                    return firstPoint.time;
                }

                return DateTime.Now;
            }
        }

        protected void saveInfoToDB(SQLiteConnection db)
        {
            db.Insert(currentDatapoint);
        }
        
        protected void getRelevantDatapoints(DateTime start, TimeSpan width, TimeSpan pointWidth)
        {
            using (var db = new SQLiteConnection(dbPath))
            {
                
                if (loadedDatapointStart.Year < 2018 || loadedDatapointEnd.Year < 2018)
                {
                    queriedDatapoints.Clear();
                    DateTime end = start + width;
                    var query = db.Table<TDataPointType>()
                        .Where(d => d.time >= start && d.time <= end && (d.instanceName == null || d.instanceName == instanceName))
                        .ToList<TDataPointType>();
                    queriedDatapoints.AddRange(query);
                    loadedDatapointStart = start;
                    loadedDatapointEnd = end;
                }
                else
                {
                    if (start < loadedDatapointStart)
                    {
                        DateTime end = loadedDatapointStart; // finish at the start of existing data
                        var query = db.Table<TDataPointType>()
                            .Where(d => d.time >= start && d.time <= end && (d.instanceName == null || d.instanceName == instanceName))
                            .ToList<TDataPointType>();
                        queriedDatapoints.AddRange(query);
                        loadedDatapointStart = start;
                    }
                    if (start + width > loadedDatapointEnd)
                    {
                        DateTime end = start + width; // start at the end of existing data and finish at the end
                        var query = db.Table<TDataPointType>()
                            .Where(d => d.time >= loadedDatapointEnd && d.time <= end && (d.instanceName == null || d.instanceName == instanceName))
                            .ToList<TDataPointType>();
                        queriedDatapoints.AddRange(query);
                        loadedDatapointEnd = start + width;
                    }
                    queriedDatapoints.Sort((a, b) => a.time.CompareTo(b.time));
                }
                
            }
            compressAndFixDatapoints(start, width, pointWidth);
        }

        private void compressAndFixDatapoints(DateTime start, TimeSpan width, TimeSpan pointWidth)
        {
            List<TDataPointType> relevantDataPoints = new List<TDataPointType>();
            foreach (var p in queriedDatapoints)
            {
                if (p.time >= start && p.time <= start+width)
                {
                    relevantDataPoints.Add((TDataPointType)p.Clone());
                }
            }
            List<TDataPointType> points = new List<TDataPointType>();

            if (relevantDataPoints.Count > 0) // add datapoint at the begining if needed
            {
                TDataPointType datapoint = relevantDataPoints[0];
                TimeSpan spanBetweenPoints = datapoint.time - start;
                if (spanBetweenPoints > pointWidth+ pointWidth)
                {
                    points.Add(new TDataPointType() { time = start, span = pointWidth });
                    points.Add(new TDataPointType(){time = datapoint.time - pointWidth, span = pointWidth });
                }
            }
            else
            {
                points.Add(new TDataPointType() { time = start, span = pointWidth });
                points.Add(new TDataPointType() { time = start+width- pointWidth, span = pointWidth });
            }



            for (int i = 0; i < relevantDataPoints.Count; i++)
            {
                TDataPointType p = relevantDataPoints[i];
                for (  ; i < relevantDataPoints.Count-1; i++)
                {
                    if (p.time + p.span + pointWidth+ pointWidth > relevantDataPoints[i + 1].time) // if span is close to next point 
                    {
                        if (p.span < pointWidth) // if span is less than width
                        {
                            p.addDatapoint(relevantDataPoints[i + 1]);
                        }
                        else
                        {
                            points.Add(p);
                            break;
                        }
                    }
                    else // if not close to next point
                    {
                        points.Add(p);
                        // then add 2 zero points on both ends
                        points.Add(new TDataPointType() { time = p.time + pointWidth, span = pointWidth });
                        points.Add(new TDataPointType() { time = relevantDataPoints[i + 1].time - pointWidth, span = pointWidth });
                        break;
                    }

                }
            }

            if (relevantDataPoints.Count > 0) // at the end
            {
                TDataPointType datapoint = relevantDataPoints[relevantDataPoints.Count - 1];
                TimeSpan spanBetweenPoints = start + width - datapoint.time;
                if (spanBetweenPoints > pointWidth + pointWidth)
                {
                    points.Add(new TDataPointType() {time = datapoint.time + pointWidth, span = pointWidth});
                    points.Add(new TDataPointType() { time = start + width - pointWidth, span = pointWidth });
                }
            }

            compressedDatapoints = points;
        }

    }



	public class MemoryInstance : AnInstanceWithDatapoint<MemoryDatapoint>
	{
		private PerformanceCounter memCounter;
		private int totalMemory;

        public MemoryInstance(string name)
		{
            instanceName = name;
			memCounter = new PerformanceCounter("Memory", "Available MBytes");
			totalMemory = (int)GetTotalMemoryInMBytes();

			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.DarkOrchid });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.DarkOrchid });
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

        private void writeToSeries()
		{
			liveSeries[0].Points.AddXY(DateTime.Now, ((MemoryDatapoint)currentDatapoint).mem);
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
			double total = 0;
            int count = 0;

			foreach (var point in compressedDatapoints)
			{
				MemoryDatapoint datapoint = (MemoryDatapoint)point;
				historySeries[0].Points.AddXY(datapoint.time, datapoint.mem);
				total += datapoint.mem;
                count++;
            }

			if (compressedDatapoints.Count > 1) // position exists
			{
                DateTime firstTime = compressedDatapoints[0].time;
                DateTime lastTime = compressedDatapoints[compressedDatapoints.Count - 1].time;
				TimeSpan selectionSpan = lastTime - firstTime;
				historySummary = "Selection period: " + selectionSpan.ToString() + Environment.NewLine +
								 "Average Memory consumption: " + (total / count).ToString("#.000") + "MB";// + Environment.NewLine;
			}

			return historySummary;
		}

    }

	public class DiskInstance : AnInstanceWithDatapoint<DiskDatapoint>
    {
		public PerformanceCounter readCounter;
		public PerformanceCounter writeCounter;

        public DiskInstance(string name)
        {
            instanceName = name + "disk usage";
			readCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", name);
			writeCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", name);

			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.GreenYellow, BorderDashStyle = ChartDashStyle.Dash });
			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Green });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.GreenYellow, BorderDashStyle = ChartDashStyle.Dash });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Green });
        }

        private void writeToSeries()
		{
			liveSeries[0].Points.AddXY(DateTime.Now, ((DiskDatapoint)currentDatapoint).write / currentDatapoint.span.TotalSeconds);
			liveSeries[1].Points.AddXY(DateTime.Now, ((DiskDatapoint)currentDatapoint).read / currentDatapoint.span.TotalSeconds);
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
			double totalRead = 0;
			double totalWrite = 0;

            getRelevantDatapoints(start, width, pointWidth);

			foreach (var point in compressedDatapoints)
			{
                DiskDatapoint datapoint = (DiskDatapoint)point;

                historySeries[0].Points.AddXY(datapoint.time, datapoint.write / datapoint.span.TotalSeconds);
				historySeries[1].Points.AddXY(datapoint.time, datapoint.read / datapoint.span.TotalSeconds);
				totalRead += datapoint.read;
				totalWrite += datapoint.write;
			}

			if (compressedDatapoints.Count > 1) // position exists
			{
                DateTime firstTime = compressedDatapoints[0].time;
                DateTime lastTime = compressedDatapoints[compressedDatapoints.Count - 1].time;
                TimeSpan selectionSpan = lastTime - firstTime;
				historySummary = "Selection period: " + selectionSpan.ToString() + Environment.NewLine +
				                 "Total disk read: " + (totalRead / 1024).ToString("#.000") + "GB" + Environment.NewLine +
				                 "Total disk write: " + (totalWrite / 1024).ToString("#.000") + "GB" + Environment.NewLine +
				                 "Average disk read speed: " + (totalRead / selectionSpan.TotalSeconds).ToString("#.000") + "MB/s" + Environment.NewLine +
				                 "Average disk write speed: " + (totalWrite / selectionSpan.TotalSeconds).ToString("#.000") + "MB/s";// + Environment.NewLine +
			}

			return historySummary;
		}
    };

	public class NetworkInstance : AnInstanceWithDatapoint<NetworkDatapoint>
	{
		public PerformanceCounter readCounter;
		public PerformanceCounter writeCounter;

        public NetworkInstance(string name)
		{
            instanceName = name + "network usage";
			readCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", name);
			writeCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", name);

			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Salmon, BorderDashStyle = ChartDashStyle.Dash });
			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Red });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Salmon, BorderDashStyle = ChartDashStyle.Dash });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Red });
        }

        private void writeToSeries()
		{
			liveSeries[0].Points.AddXY(DateTime.Now, ((NetworkDatapoint)currentDatapoint).write / currentDatapoint.span.TotalSeconds * 8);
			liveSeries[1].Points.AddXY(DateTime.Now, ((NetworkDatapoint)currentDatapoint).read / currentDatapoint.span.TotalSeconds * 8);
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
			double totalRead = 0;
			double totalWrite = 0;

            getRelevantDatapoints(start, width, pointWidth);

			foreach (var point in compressedDatapoints)
			{
				NetworkDatapoint datapoint = (NetworkDatapoint)point;

				historySeries[0].Points.AddXY(datapoint.time, datapoint.write / datapoint.span.TotalSeconds*8);
				historySeries[1].Points.AddXY(datapoint.time, datapoint.read / datapoint.span.TotalSeconds*8);
				totalRead += datapoint.read;
				totalWrite += datapoint.write;
			}

			if (compressedDatapoints.Count > 1) // position exists
			{
                DateTime firstTime = compressedDatapoints[0].time;
                DateTime lastTime = compressedDatapoints[compressedDatapoints.Count - 1].time;
                TimeSpan selectionSpan = lastTime - firstTime;
				historySummary = "Selection period: " + selectionSpan.ToString() + Environment.NewLine +
				                 "Total network received: " + (totalRead / 1024).ToString("#.000") + "GB" + Environment.NewLine +
				                 "Total network sent: " + (totalWrite / 1024).ToString("#.000") + "GB" + Environment.NewLine +
				                 "Average network received speed: " + (totalRead / selectionSpan.TotalSeconds * 8).ToString("#.000") + "Mbps" + Environment.NewLine +
				                 "Average network sent speed: " + (totalWrite / selectionSpan.TotalSeconds * 8).ToString("#.000") + "Mbps";// + Environment.NewLine +
			}

			return historySummary;
		}
    };

	public class CpuInstance : AnInstanceWithDatapoint<CPUDatapoint>
	{
		PerformanceCounter totalCpuTimeCount;

        public CpuInstance(string name)
		{
            instanceName = name;
			totalCpuTimeCount = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");

			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.DodgerBlue });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.DodgerBlue });
        }

		private void writeToSeries()
		{
			liveSeries[0].Points.AddXY(DateTime.Now, ((CPUDatapoint)currentDatapoint).usage);

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

			foreach (var point in compressedDatapoints)
			{
				CPUDatapoint datapoint = (CPUDatapoint)point;

				historySeries[0].Points.AddXY(datapoint.time, datapoint.usage);
				total += datapoint.usage;
				count++;
			}

			if (compressedDatapoints.Count > 1) // position exists
            {
                DateTime firstTime = compressedDatapoints[0].time;
                DateTime lastTime = compressedDatapoints[compressedDatapoints.Count - 1].time;

                TimeSpan selectionSpan = lastTime - firstTime;
				historySummary = "Selection period: " + selectionSpan.ToString() + Environment.NewLine +
				                 "Average CPU usage: " + (total / count).ToString("#.000") + "%";// + Environment.NewLine;
			}

			return historySummary;
		}
	}
}
