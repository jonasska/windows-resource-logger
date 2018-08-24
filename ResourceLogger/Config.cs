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
		public static int DatapointsInOneFile = 100000;

		public static int HistoryDatapointLines = 2000;
		public static int LivePoints = 1800;
		public static int SamplingInterval = 1; //number of seconds
	}

	public struct AxisRange
	{
		public double lower;
		public double higher;
	}
}
