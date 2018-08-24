using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceLogger
{
	class Config
	{
		public static string systemDirectory = "../../logs/system/";
		public static int nDatapointsInOneFile = 100000;
	}

	public struct AxisRange
	{
		public double lower;
		public double higher;
	}
}
