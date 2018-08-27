using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace ResourceLogger
{
	//class Config
	//{
	//	// default config
	//	public static string LogsDirectory = "../../logs/";
	//	public static string SystemDirectory = LogsDirectory + "system/";
	//	public static int DatapointsInOneFile = 100000;

	//	public static int HistoryPoints = 2000;
	//	public static int LivePoints = 1800;
	//	public static int SamplingInterval = 1; //number of seconds
	//}

	public struct AxisRange
	{
		public double lower;
		public double higher;
	}
}
