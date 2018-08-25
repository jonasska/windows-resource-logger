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

		public void addDatapoint(string point)
		{
			CPUDatapoint p = new CPUDatapoint(point);
			cpuSpan += p.cpuSpan;
			span += p.span;
			usage = 100 * cpuSpan.TotalSeconds / span.TotalSeconds;
		}
	}
}
