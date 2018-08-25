using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceLogger
{
	public abstract class Datapoint
	{
		public DateTime time;
		public TimeSpan span;

		//public abstract T aggregateDatapoints(List<T> points);
	}

	public class MemoryDatapoint : Datapoint
	{
		public int mem;

		public MemoryDatapoint(string point)
		{
			var data = point.Split(',');
			time = new DateTime(long.Parse(data[0]) * 10000);
			mem = int.Parse(data[1]);
			span = new TimeSpan(long.Parse(data[2]) * 10000);
		}

		public MemoryDatapoint(List<String> points)
		{
			time = new MemoryDatapoint(points[0]).time;
			mem = 0;
			span = TimeSpan.FromSeconds(0);
			foreach (var strPoint in points)
			{
				MemoryDatapoint point = new MemoryDatapoint(strPoint);
				mem += point.mem/ points.Count;
				span += point.span;
			}
		}

		public void addDatapoint(string point)
		{
			MemoryDatapoint p = new MemoryDatapoint(point);
			span += p.span;
		}
	}

	public class DiskDatapoint : Datapoint
	{
		public double read;
		public double write;

		public DiskDatapoint(string point)
		{
			var data = point.Split(',');
			time = new DateTime(long.Parse(data[0]) * 10000);
			read = double.Parse(data[1]);
			write = double.Parse(data[2]);
			span = new TimeSpan(long.Parse(data[3]) * 10000);
		}

		public DiskDatapoint(List<String>points)
		{
			time = new DiskDatapoint(points[0]).time;
			read = 0;
			write = 0;
			span = TimeSpan.FromSeconds(0);
			foreach (var strPoint in points)
			{
				DiskDatapoint point = new DiskDatapoint(strPoint);
				read += point.read;
				write += point.write;
				span += point.span;
			}
		}

		public void addDatapoint(string point)
		{
			DiskDatapoint p = new DiskDatapoint(point);
			read += p.read;
			write += p.write;
			span += p.span;
		}

	}

	public class NetworkDatapoint : Datapoint
	{
		public double read;
		public double write;

		public NetworkDatapoint(string point)
		{
			var data = point.Split(',');
			time = new DateTime(long.Parse(data[0]) * 10000);
			read = double.Parse(data[1]);
			write = double.Parse(data[2]);
			span = new TimeSpan(long.Parse(data[3]) * 10000);
		}

		public NetworkDatapoint(List<String> points)
		{
			time = new NetworkDatapoint(points[0]).time;
			read = 0;
			write = 0;
			span = TimeSpan.FromSeconds(0);
			foreach (var strPoint in points)
			{
				NetworkDatapoint point = new NetworkDatapoint(strPoint);
				read += point.read;
				write += point.write;
				span += point.span;
			}
		}

		public void addDatapoint(string point)
		{
			NetworkDatapoint p = new NetworkDatapoint(point);
			read += p.read;
			write += p.write;
			span += p.span;
		}
	}

	public class CPUDatapoint : Datapoint
	{
		public TimeSpan cpuSpan;
		public double usage;

		public CPUDatapoint(string point)
		{
			var data = point.Split(',');
			time = new DateTime(long.Parse(data[0]) * 10000);
			cpuSpan = new TimeSpan(long.Parse(data[1]) * 10000);
			span = new TimeSpan(long.Parse(data[2]) * 10000);
			usage = cpuSpan.TotalSeconds / span.TotalSeconds * 100.0;
		}

		public CPUDatapoint(List<String> points)
		{
			time = new CPUDatapoint(points[0]).time;
			cpuSpan = TimeSpan.FromSeconds(0);
			span = TimeSpan.FromSeconds(0);
			foreach (var strPoint in points)
			{
				CPUDatapoint point = new CPUDatapoint(strPoint);
				cpuSpan += point.cpuSpan;
				span += point.span;
			}
			usage = 100*cpuSpan.TotalSeconds / span.TotalSeconds;

			//if (new CPUDatapoint(points[points.Count - 1]).time - new CPUDatapoint(points[0]).time > TimeSpan.FromSeconds(3))
			//{
			//	TimeSpan s = new CPUDatapoint(points[points.Count - 1]).time - new CPUDatapoint(points[0]).time;
			//	Debug.WriteLine(s);
			//}
		}

		public void addDatapoint(string point)
		{
			CPUDatapoint p = new CPUDatapoint(point);
			cpuSpan += p.cpuSpan;
			span += p.span;
			usage = 100 * cpuSpan.TotalSeconds / span.TotalSeconds;
		}
	}
}
