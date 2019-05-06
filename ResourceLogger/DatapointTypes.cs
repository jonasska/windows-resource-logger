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
	public abstract class Datapoint : ICloneable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime time { get; set; }
        public TimeSpan span { get; set; } = TimeSpan.Zero;
        //[Indexed]
        public string instanceName { get; set; }

        public abstract void addDatapoint(Datapoint point);

        public object Clone()
        {
            Datapoint d = (Datapoint)this.MemberwiseClone();
            return d;
        }
    }

	public class MemoryDatapoint : Datapoint
    {
        public int mem { get; set; } = 0;

        public MemoryDatapoint(DateTime t, TimeSpan s, int m)
        {
            time = t;
            span = s;
            mem = m;
        }

        public MemoryDatapoint(){}

        public override void addDatapoint(Datapoint point)
        {
            var p = (MemoryDatapoint) point;
            span += p.span;
        }
    }

	public class DiskDatapoint : Datapoint
    {
        public double read { get; set; } = 0;
        public double write { get; set; } = 0;

        public DiskDatapoint(DateTime t, TimeSpan s, double r, double w, string name)
        {
            time = t;
            span = s;
            read = r;
            write = w;
            instanceName = name;
        }
        public DiskDatapoint(){}

        public override void addDatapoint(Datapoint point)
        {
            var p = (DiskDatapoint)point;
            read += p.read;
            write += p.write;
            span += p.span;
        }
    }

	public class NetworkDatapoint : Datapoint
    {
        public double read { get; set; } = 0;
        public double write { get; set; } = 0;

        public NetworkDatapoint(DateTime t, TimeSpan s, double r, double w, string name)
        {
            time = t;
            span = s;
            read = r;
            write = w;
            instanceName = name;
        }
        public NetworkDatapoint(){}

        public override void addDatapoint(Datapoint point)
        {
            var p = (NetworkDatapoint)point;
            read += p.read;
            write += p.write;
            span += p.span;
        }
    }

	public class CPUDatapoint : Datapoint
	{
		public TimeSpan cpuSpan { get; set; } = TimeSpan.Zero;
        public double usage { get; set; } = 0;

        public CPUDatapoint(DateTime t, TimeSpan cpuS, TimeSpan s)
        {
            time = t;
            cpuSpan = cpuS;
            span = s;
            usage = cpuSpan.TotalSeconds / span.TotalSeconds * 100.0;
        }
        public CPUDatapoint(){}

        public override void addDatapoint(Datapoint point)
        {
            var p = (CPUDatapoint)point;
            cpuSpan += p.cpuSpan;
            span += p.span;
            usage = 100 * cpuSpan.TotalSeconds / span.TotalSeconds;
        }
    }
}
