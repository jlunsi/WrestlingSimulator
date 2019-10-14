using System;
using System.Threading;
using System.Collections.Generic;

namespace WrestlingSimulator
{
	//This class exists because I may turn this project into a GUI application in the future. This class allows me to have
	//that along with keeping the terminal version.
	//It also allows me to save the match result to a file instead.
	public class Output 
	{
		protected static Output instance;
		protected List<string> linesToOutput;

		public Output () {
			linesToOutput = new List<String>();
		}

		public static void DisplayMatch () {
			if (instance == null)
				throw new Exception ("No lines to output.");
			instance.OutputLines ();
		}

		public static void AddToOutput (string line) {
			CreateInstance ();
			instance.linesToOutput.Add (line);
		}

		protected static void CreateInstance () {
			if (instance == null)
				instance = new Output ();
		}

		public void OutputLines () {
			Console.WriteLine ("Press any key to read the next line (arrow keys recommended)");
			for (ushort i = 0; i < linesToOutput.Count; i++) {
				Console.WriteLine (linesToOutput [i]);
				Console.Read();
			}
		}
	}
}

