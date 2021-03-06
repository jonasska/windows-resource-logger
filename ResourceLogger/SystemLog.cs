﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms.DataVisualization.Charting;
using ResourceLogger;
using SQLite;

namespace ResourceLogger
{
	public class SystemLog
	{
		public List<AnInstance> instances;
		public AnInstance selectedInstance;
		protected DateTime t1;

		Thread nextValuesThread;
		private bool stopThreads = false;

        public string dbPath { get; set; } = Properties.Settings.Default.SystemDirectory + "LocalResourceRecords.db";


        public SystemLog()
		{
            instances = new List<AnInstance>();
			instances.Add(new CpuInstance("CPU"));
			instances.Add(new MemoryInstance("Memory"));

			t1 = DateTime.Now;

			// disks
			PerformanceCounterCategory cat = new PerformanceCounterCategory("PhysicalDisk");
			List<string> checkinstances = cat.GetInstanceNames().ToList();
			checkinstances.Sort();
			//checkinstances.Remove("_Total");
			foreach (string instance in checkinstances)
			{
				instances.Add(new DiskInstance(instance));
			}

            // networks
            cat = new PerformanceCounterCategory("Network Interface");
			checkinstances = cat.GetInstanceNames().ToList();
			foreach (string instance in checkinstances)
			{
				instances.Add(new NetworkInstance(instance));
			}

            foreach (var instance in instances)
            {
                instance.dbPath = dbPath;
            }

            
        }

		public void nextValues()
		{
			DateTime now = DateTime.Now;
			TimeSpan span = now - t1;

            using (SQLiteConnection db = new SQLiteConnection(dbPath))
            {
                db.BeginTransaction();
                foreach (var instance in instances)
                {
                    instance.nextValues(span, db);
                }
                db.Commit();
            }

			t1 = now;
		}

		public void killThreads()
		{
			stopThreads = true;
		}

		public string[] getAllInstanceNames()
		{
			List<string> lStr = new List<string>();
			foreach (var instance in instances)
			{
				lStr.Add(instance.DisplayName);
			}
			return lStr.ToArray();
		}

		public bool setSelectedInstance(string name)
		{
			foreach (var i in instances)
			{
				i.isSelected = false;
				i.SetSeriesVisible(false);
			}
			foreach (var i in instances)
			{
				if (i.DisplayName == name)
				{
					i.isSelected = true;
					i.SetSeriesVisible(true);
					selectedInstance = i;
					return true;
				}
			}

			return false;
		}

		public int getSelectedInstanceIndex()
		{
			for(int i=0;i<instances.Count;i++)
			{
				if (instances[i].isSelected)
				{
					return i;
				}
			}
			return -1;
		}
		public AnInstance getSelectedInstance()
		{
			for (int i = 0; i < instances.Count; i++)
			{
				if (instances[i].isSelected)
				{
					return instances[i];
				}
			}
			return null;
		}
	}
}
