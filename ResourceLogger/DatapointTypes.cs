using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace ResourceLogger
{
	public abstract class Datapoint
	{
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime time { get; set; }
        public TimeSpan span { get; set; }
        //[Indexed]
        public string instanceName { get; set; }

        //public abstract T aggregateDatapoints(List<T> points);
    }

	public class MemoryDatapoint : Datapoint
	{
		public int mem { get; set; }

        public MemoryDatapoint(string point)
		{
			var data = point.Split(',');
			time = new DateTime(long.Parse(data[0]) * 10000);
			mem = int.Parse(data[1]);
			span = new TimeSpan(long.Parse(data[2]) * 10000);
		}

        public MemoryDatapoint(DateTime t, TimeSpan s, int m)
        {
            time = t;
            span = s;
            mem = m;
        }
        public MemoryDatapoint()
        {
            
        }

		public void addDatapoint(string point)
		{
			MemoryDatapoint p = new MemoryDatapoint(point);
			span += p.span;
		}
	}

	public class DiskDatapoint : Datapoint
	{
		public double read { get; set; }
        public double write { get; set; }

        public DiskDatapoint(string point)
		{
			var data = point.Split(',');
			time = new DateTime(long.Parse(data[0]) * 10000);
			read = double.Parse(data[1]);
			write = double.Parse(data[2]);
			span = new TimeSpan(long.Parse(data[3]) * 10000);
		}

        public DiskDatapoint(DateTime t, TimeSpan s, double r, double w, string name)
        {
            time = t;
            span = s;
            read = r;
            write = w;
            instanceName = name;
        }
        public DiskDatapoint()
        {
            
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
		public double read { get; set; }
        public double write { get; set; }

        public NetworkDatapoint(string point)
		{
			var data = point.Split(',');
			time = new DateTime(long.Parse(data[0]) * 10000);
			read = double.Parse(data[1]);
			write = double.Parse(data[2]);
			span = new TimeSpan(long.Parse(data[3]) * 10000);
		}

        public NetworkDatapoint(DateTime t, TimeSpan s, double r, double w, string name)
        {
            time = t;
            span = s;
            read = r;
            write = w;
            instanceName = name;
        }
        public NetworkDatapoint()
        {
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
		public TimeSpan cpuSpan { get; set; }
        public double usage { get; set; }

        public CPUDatapoint(string point)
		{
			var data = point.Split(',');
			time = new DateTime(long.Parse(data[0]) * 10000);
			cpuSpan = new TimeSpan(long.Parse(data[1]) * 10000);
			span = new TimeSpan(long.Parse(data[2]) * 10000);
			usage = cpuSpan.TotalSeconds / span.TotalSeconds * 100.0;
		}
        public CPUDatapoint(DateTime t, TimeSpan cpuS, TimeSpan s)
        {
            time = t;
            cpuSpan = cpuS;
            span = s;
            usage = cpuSpan.TotalSeconds / span.TotalSeconds * 100.0;
        }
        public CPUDatapoint()
        {
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
