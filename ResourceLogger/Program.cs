using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;


namespace ResourceLogger
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(true);
			Application.Run(new Form1());




		}




	}


}
