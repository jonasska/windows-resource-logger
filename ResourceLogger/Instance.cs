using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace ResourceLogger
{
	public abstract class AnInstance
	{
		public string instanceName;
		public bool isSelected;
		protected List<Series> liveSeries;
		protected List<Series> historySeries;
		protected TimeSpan currentSpan;
		protected string filepath;
		protected string filepath2;
		protected string datapointFileNumber;
		protected int pointsInTheLatestFile;
		protected List<string> datapointLines;
		protected List<Datapoint> datapoints;


		public abstract void nextValues(TimeSpan span);

		public abstract AxisRange GetLiveYAxisRange();

		public abstract AxisRange GetHistoryYAxisRange();

		public abstract string drawHistoryGraph(DateTime start, TimeSpan width, TimeSpan pointWidth);

		protected abstract void parseDatapoints(TimeSpan width, TimeSpan pointWidth);

		protected AnInstance()
		{
			liveSeries = new List<Series>();
			historySeries = new List<Series>();
			datapointLines = new List<string>();
			datapoints = new List<Datapoint>();
			isSelected = false;
			pointsInTheLatestFile = 0;
			datapointFileNumber = (DateTime.Now.Ticks / 10000).ToString();
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

		protected void readRelevantDatapoints(DateTime position, TimeSpan width, TimeSpan pointWidth)
		{
			if (System.IO.Directory.Exists(Config.systemDirectory))
			{
				if (System.IO.Directory.Exists(filepath2))
				{
					datapointLines.Clear();
					var paths = System.IO.Directory.GetFiles(filepath2);


					for (int i = 0; i <paths.Length; i++)
					{
						var thisFileParts = paths[i].Split(new char[] { '.', '/' });
						DateTime thisFilePosition = new DateTime(long.Parse(thisFileParts[thisFileParts.Length - 2]) * 10000);
						if (thisFilePosition > position + width)
						{
							continue;
						}
						if (i+1 < paths.Length) //next exist
						{
							var nextFileParts = paths[i+1].Split(new char[] { '.', '/' });
							DateTime nextFilePosition = new DateTime(long.Parse(nextFileParts[nextFileParts.Length - 2]) * 10000);
							if (nextFilePosition < position)
							{
								continue;
							}
						}
						System.IO.StreamReader reader = new System.IO.StreamReader(paths[i]);
						while (!reader.EndOfStream)
						{
							string line = reader.ReadLine();
							var data = line.Split(',');
							long ticks = long.Parse(data[0]) * 10000;
							DateTime linePosition = new DateTime(ticks);
							if (linePosition < position || linePosition > position + width)
							{
								continue;
							}
							datapointLines.Add(line);
						}
						reader.Close();
					}
				}
			}
			parseDatapoints(width, pointWidth);
		}

		public DateTime GetFirstDatapointDateTime()
		{
			if (System.IO.Directory.Exists(Config.systemDirectory))
			{
				if (System.IO.Directory.Exists(filepath2))
				{
					var paths = System.IO.Directory.GetFiles(filepath2);
					var parts = paths[0].Split(new char[] { '.', '/' });
					DateTime filePosition = new DateTime(long.Parse(parts[parts.Length - 2]) * 10000);
					return filePosition;
				}
			}

			return DateTime.Now;
		}

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
		private double currentMemUsage;

		public MemoryInstance(string name)
		{
			instanceName = name;
			memCounter = new PerformanceCounter("Memory", "Available MBytes");
			totalMemory = (int)GetTotalMemoryInMBytes();

			filepath = Config.systemDirectory + "MemoryTotal.txt";//  ???
			filepath2 = Config.systemDirectory + "MemoryDatapoints/";//+ "MemoryDatapoints.txt";

			//readInfoFromFile();  //????

			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.DarkOrchid });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.DarkOrchid });
		}

		static ulong GetTotalMemoryInMBytes()
		{
			return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1024 / 1024;
		}

		public override void nextValues(TimeSpan span)
		{
			currentMemUsage = totalMemory - memCounter.NextValue();
			currentSpan = span;
			saveInfoToFile();
			writeToSeries();
		}

		private void saveInfoToFile()
		{
			System.IO.Directory.CreateDirectory(Config.systemDirectory);
			System.IO.Directory.CreateDirectory(filepath2);
			//System.IO.StreamWriter writer = new System.IO.StreamWriter(filepath);
			//writer.WriteLine(); // total mem????
			//writer.Close();

			string line = (DateTime.Now.Ticks / 10000) + "," + currentMemUsage + "," + (currentSpan.Ticks / 10000);
			line += Environment.NewLine;
			File.AppendAllText(filepath2 + datapointFileNumber + ".txt", line);
			pointsInTheLatestFile++;
			if (pointsInTheLatestFile > Config.DatapointsInOneFile)
			{
				datapointFileNumber = (DateTime.Now.Ticks / 10000).ToString();
				pointsInTheLatestFile = 0;
			}
		}

		private void writeToSeries()
		{
			liveSeries[0].Points.AddXY(DateTime.Now, currentMemUsage);
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

			readRelevantDatapoints(start, width, pointWidth);

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

			if (datapointLines.Count > 1) // position exists
			{
				DateTime firstTime = new DateTime(long.Parse(datapointLines[0].Split(',')[0]) * 10000);
				DateTime lastTime = new DateTime(long.Parse(datapointLines[datapointLines.Count - 1].Split(',')[0]) * 10000);
				TimeSpan selectionSpan = lastTime - firstTime;
				historySummary = "Selection period: " + selectionSpan.ToString() + Environment.NewLine +
								 "Average Memory consumption: " + (total / count).ToString("#.000") + "MB";// + Environment.NewLine;
			}

			return historySummary;
		}

		protected override void parseDatapoints(TimeSpan width, TimeSpan pointWidth)
		{
			datapoints.Clear();

			for (int i = 0; i< datapointLines.Count; i++)
			{
				var datapoint = new MemoryDatapoint(datapointLines[i]);
				for (; i < datapointLines.Count && datapoint.span < pointWidth; i++)
				{
					datapoint.addDatapoint(datapointLines[i]);
				}
				datapoints.Add(datapoint);
			}
		}
	}

	public class DiskInstance : AnInstance
	{
		public PerformanceCounter readCounter;
		public PerformanceCounter writeCounter;
		float currentWriteSpeed;
		float currentReadSpeed;
		public double totalDataWritten;
		public double totalDataRead;
		double currentDataWrite;
		double currentDataRead;


		public DiskInstance(string name)
		{
			instanceName = name + "disk usage";
			readCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", name);
			writeCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", name);


			totalDataWritten = 0;
			totalDataRead = 0;

			filepath = Config.systemDirectory + instanceName.Replace(':', ' ') + "Total.txt";
			filepath2 = Config.systemDirectory + instanceName.Replace(':', ' ') + " datapoints/";

			readInfoFromFile();

			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.GreenYellow, BorderDashStyle = ChartDashStyle.Dash });
			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Green });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.GreenYellow, BorderDashStyle = ChartDashStyle.Dash });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Green });
		}

		private void saveInfoToFile()
		{
			System.IO.Directory.CreateDirectory(Config.systemDirectory);
			System.IO.Directory.CreateDirectory(filepath2);
			System.IO.StreamWriter writer = new System.IO.StreamWriter(filepath);
			writer.WriteLine(totalDataRead);
			writer.WriteLine(totalDataWritten);
			writer.Close();
			string line = (DateTime.Now.Ticks / 10000) + "," + currentDataRead.ToString("0.000") + "," + currentDataWrite.ToString("0.000") + "," + (currentSpan.Ticks / 10000);
			line += Environment.NewLine;
			File.AppendAllText(filepath2 + datapointFileNumber + ".txt", line);
			pointsInTheLatestFile++;
			if (pointsInTheLatestFile > Config.DatapointsInOneFile)
			{
				datapointFileNumber = (DateTime.Now.Ticks / 10000).ToString();
				pointsInTheLatestFile = 0;
			}
		}

		private void writeToSeries()
		{
			liveSeries[0].Points.AddXY(DateTime.Now, currentWriteSpeed / 1024 / 1024);
			liveSeries[1].Points.AddXY(DateTime.Now, currentReadSpeed / 1024 / 1024);
		}

		public override void nextValues(TimeSpan span)
		{
			currentWriteSpeed = writeCounter.NextValue();
			currentReadSpeed = readCounter.NextValue();
			currentDataWrite = currentWriteSpeed * span.TotalSeconds / 1024.0 / 1024.0;
			currentDataRead = currentReadSpeed * span.TotalSeconds / 1024.0 / 1024.0;
			totalDataWritten += currentDataWrite;
			totalDataRead += currentDataRead;
			currentSpan = span;
			saveInfoToFile();
			writeToSeries();
		}

		private void readInfoFromFile()
		{
			if (System.IO.Directory.Exists(Config.systemDirectory))
			{
				if (System.IO.File.Exists(filepath))
				{
					System.IO.StreamReader reader = new System.IO.StreamReader(filepath);
					totalDataRead = double.Parse(reader.ReadLine());
					totalDataWritten = double.Parse(reader.ReadLine());
					reader.Close();
				}
			}
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

			readRelevantDatapoints(start, width, pointWidth);

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

			if (datapointLines.Count > 1) // position exists
			{
				DateTime firstTime = new DateTime(long.Parse(datapointLines[0].Split(',')[0]) * 10000);
				DateTime lastTime = new DateTime(long.Parse(datapointLines[datapointLines.Count - 1].Split(',')[0]) * 10000);
				TimeSpan selectionSpan = lastTime - firstTime;
				historySummary = "Selection period: " + selectionSpan.ToString() + Environment.NewLine +
				                 "Total disk read: " + (totalRead / 1024).ToString("#.000") + "GB" + Environment.NewLine +
				                 "Total disk write: " + (totalWrite / 1024).ToString("#.000") + "GB" + Environment.NewLine +
				                 "Average disk read speed: " + (totalRead / selectionSpan.TotalSeconds).ToString("#.000") + "MB/s" + Environment.NewLine +
				                 "Average disk write speed: " + (totalWrite / selectionSpan.TotalSeconds).ToString("#.000") + "MB/s";// + Environment.NewLine +
			}

			return historySummary;
		}

		protected override void parseDatapoints(TimeSpan width, TimeSpan pointWidth)
		{
			datapoints.Clear();

			for (int i = 0; i < datapointLines.Count; i++)
			{
				var datapoint = new DiskDatapoint(datapointLines[i]);
				for (; i < datapointLines.Count && datapoint.span < pointWidth; i++)
				{
					datapoint.addDatapoint(datapointLines[i]);
				}
				datapoints.Add(datapoint);
			}
		}
	};

	public class NetworkInstance : AnInstance
	{
		public PerformanceCounter readCounter;
		public PerformanceCounter writeCounter;
		float currentWriteSpeed;
		float currentReadSpeed;
		public double totalDataWritten;
		public double totalDataRead;
		double currentDataWrite;
		double currentDataRead;

		public NetworkInstance(string name)
		{
			instanceName = name + "network usage";
			readCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", name);
			writeCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", name);

			totalDataWritten = 0;
			totalDataRead = 0;

			filepath = Config.systemDirectory + instanceName.Replace(':', ' ') + "networkTotal.txt";
			filepath2 = Config.systemDirectory + instanceName.Replace(':', ' ') + " networkDatapoints/";

			readInfoFromFile();

			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Salmon, BorderDashStyle = ChartDashStyle.Dash });
			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Red });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Salmon, BorderDashStyle = ChartDashStyle.Dash });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.Red });
		}

		private void saveInfoToFile()
		{
			System.IO.Directory.CreateDirectory(Config.systemDirectory);
			System.IO.Directory.CreateDirectory(filepath2);
			System.IO.StreamWriter writer = new System.IO.StreamWriter(filepath);
			writer.WriteLine(totalDataRead);
			writer.WriteLine(totalDataWritten);
			writer.Close();
			string line = (DateTime.Now.Ticks / 10000) + "," + currentDataRead.ToString("0.000") + "," + currentDataWrite.ToString("0.000") + "," + (currentSpan.Ticks / 10000);
			line += Environment.NewLine;
			File.AppendAllText(filepath2 + datapointFileNumber + ".txt", line);
			pointsInTheLatestFile++;
			if (pointsInTheLatestFile > Config.DatapointsInOneFile)
			{
				datapointFileNumber = (DateTime.Now.Ticks / 10000).ToString();
				pointsInTheLatestFile = 0;
			}
		}

		private void writeToSeries()
		{
			liveSeries[0].Points.AddXY(DateTime.Now, currentWriteSpeed / 1024 / 1024 * 8);
			liveSeries[1].Points.AddXY(DateTime.Now, currentReadSpeed / 1024 / 1024 * 8);
		}

		public override void nextValues(TimeSpan span)
		{
			currentWriteSpeed = writeCounter.NextValue();
			currentReadSpeed = readCounter.NextValue();
			currentDataWrite = currentWriteSpeed * span.TotalSeconds / 1024.0 / 1024.0;
			currentDataRead = currentReadSpeed * span.TotalSeconds / 1024.0 / 1024.0;
			totalDataWritten += currentDataWrite;
			totalDataRead += currentDataRead;
			currentSpan = span;
			saveInfoToFile();
			writeToSeries();
		}

		private void readInfoFromFile()
		{
			if (System.IO.Directory.Exists(Config.systemDirectory))
			{
				if (System.IO.File.Exists(filepath))
				{
					System.IO.StreamReader reader = new System.IO.StreamReader(filepath);
					totalDataRead = double.Parse(reader.ReadLine());
					totalDataWritten = double.Parse(reader.ReadLine());
					reader.Close();
				}
			}
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

			readRelevantDatapoints(start, width, pointWidth);

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

			if (datapointLines.Count > 0)
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

			if (datapointLines.Count > 1) // position exists
			{
				DateTime firstTime = new DateTime(long.Parse(datapointLines[0].Split(',')[0]) * 10000);
				DateTime lastTime = new DateTime(long.Parse(datapointLines[datapointLines.Count - 1].Split(',')[0]) * 10000);
				TimeSpan selectionSpan = lastTime - firstTime;
				historySummary = "Selection period: " + selectionSpan.ToString() + Environment.NewLine +
				                 "Total network received: " + (totalRead / 1024).ToString("#.000") + "GB" + Environment.NewLine +
				                 "Total network sent: " + (totalWrite / 1024).ToString("#.000") + "GB" + Environment.NewLine +
				                 "Average network received speed: " + (totalRead / selectionSpan.TotalSeconds * 8).ToString("#.000") + "Mbps" + Environment.NewLine +
				                 "Average network sent speed: " + (totalWrite / selectionSpan.TotalSeconds * 8).ToString("#.000") + "Mbps";// + Environment.NewLine +
			}

			return historySummary;
		}

		protected override void parseDatapoints(TimeSpan width, TimeSpan pointWidth)
		{
			datapoints.Clear();

			for (int i = 0; i < datapointLines.Count; i++)
			{
				var datapoint = new NetworkDatapoint(datapointLines[i]);
				for (; i < datapointLines.Count && datapoint.span < pointWidth; i++)
				{
					datapoint.addDatapoint(datapointLines[i]);
				}
				datapoints.Add(datapoint);
			}
		}
	};

	public class CpuInstance : AnInstance
	{
		PerformanceCounter totalCpuTimeCount;
		public float currentCpuUsage;
		public TimeSpan totalCpuSpan { get; private set; } // cpu time 
		public TimeSpan totalSpan { get; private set; } // total time 
		public TimeSpan currentCpuSpan { get; private set; }

		public CpuInstance(string name)
		{
			instanceName = name;
			totalCpuTimeCount = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");

			filepath = Config.systemDirectory + "CPUTotal.txt";
			filepath2 = Config.systemDirectory + "CPUDatapoints/";

			readInfoFromFile();

			liveSeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.DodgerBlue });
			historySeries.Add(new Series { ChartType = SeriesChartType.Line, Color = Color.DodgerBlue });
		}

		private void writeToSeries()
		{
			liveSeries[0].Points.AddXY(DateTime.Now, currentCpuUsage);

		}

		public override void nextValues(TimeSpan span)
		{
			currentCpuUsage = totalCpuTimeCount.NextValue();
			long cpuElapsed = (long)(span.Ticks * currentCpuUsage / 100);
			TimeSpan currentcpuSpan = new TimeSpan(cpuElapsed);
			totalCpuSpan = totalCpuSpan.Add(currentcpuSpan);
			totalSpan = totalSpan.Add(span);
			currentCpuSpan = currentcpuSpan;
			currentSpan = span;
			saveInfoToFile();
			writeToSeries();
		}

		private void saveInfoToFile()
		{
			System.IO.Directory.CreateDirectory(Config.systemDirectory);
			System.IO.Directory.CreateDirectory(filepath2);
			System.IO.StreamWriter writer = new System.IO.StreamWriter(filepath);
			string line = totalCpuSpan.Ticks + "," + totalCpuSpan;
			writer.WriteLine(line);
			line = totalSpan.Ticks + "," + totalSpan;
			writer.WriteLine(line);
			writer.Close();

			line = (DateTime.Now.Ticks / 10000) + "," + (currentCpuSpan.Ticks / 10000) + "," + (currentSpan.Ticks / 10000);
			line += Environment.NewLine;
			File.AppendAllText(filepath2 + datapointFileNumber + ".txt", line);
			pointsInTheLatestFile++;
			if (pointsInTheLatestFile > Config.DatapointsInOneFile)
			{
				datapointFileNumber = (DateTime.Now.Ticks / 10000).ToString();
				pointsInTheLatestFile = 0;
			}
		}

		private void readInfoFromFile()
		{
			if (System.IO.Directory.Exists(Config.systemDirectory))
			{
				if (System.IO.File.Exists(filepath))
				{
					System.IO.StreamReader reader = new System.IO.StreamReader(filepath);
					string[] line = reader.ReadLine().Split(',');
					totalCpuSpan = new TimeSpan(long.Parse(line[0]));
					line = reader.ReadLine().Split(',');
					totalSpan = new TimeSpan(long.Parse(line[0]));
					reader.Close();
				}
			}
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

			readRelevantDatapoints(start, width, pointWidth);

			string historySummary = "";
			int count = 0;
			double total = 0;


			if (datapointLines.Count > 0)
			{
				//CPUDatapoint datapoint = new CPUDatapoint(datapointLines[0]);
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

			if (datapointLines.Count > 0)
			{
				CPUDatapoint datapoint = (CPUDatapoint)datapoints[datapoints.Count-1];
				TimeSpan spanBetweenPoints = start + width - datapoint.time;
				if (spanBetweenPoints > datapoint.span + datapoint.span)
				{
					historySeries[0].Points.AddXY(datapoint.time + datapoint.span, 0.0);
					historySeries[0].Points.AddXY(start + width, 0.0);
				}
			}

			if (datapointLines.Count > 1) // position exists
			{
				DateTime firstTime = new DateTime(long.Parse(datapointLines[0].Split(',')[0]) * 10000);
				DateTime lastTime = new DateTime(long.Parse(datapointLines[datapointLines.Count - 1].Split(',')[0]) * 10000);
				TimeSpan selectionSpan = lastTime - firstTime;
				historySummary = "Selection period: " + selectionSpan.ToString() + Environment.NewLine +
				                 "Average CPU usage: " + (total / count).ToString("#.000") + "%";// + Environment.NewLine;
			}

			return historySummary;
		}

		protected override void parseDatapoints(TimeSpan width, TimeSpan pointWidth)
		{
			datapoints.Clear();

			for (int i = 0; i < datapointLines.Count; i++)
			{
				var datapoint = new CPUDatapoint(datapointLines[i]);
				for (; i < datapointLines.Count && datapoint.span < pointWidth; i++)
				{
					datapoint.addDatapoint(datapointLines[i]);
				}
				datapoints.Add(datapoint);
			}
		}
	}
}
