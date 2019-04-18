using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Data;

namespace ResourceLogger
{
	public class Logger
	{

		public DataTable table { set; get; }

		public SystemLog systemLog { get; private set; }
		bool closeProgram = false;

		public Logger()
		{
            System.IO.Directory.CreateDirectory(Properties.Settings.Default.SystemDirectory);
            systemLog = new SystemLog();
		}


		public void killThreads()
		{
			systemLog.killThreads();
		}

	}
};
