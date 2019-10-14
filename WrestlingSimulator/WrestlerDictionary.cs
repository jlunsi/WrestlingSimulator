using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace WrestlingSimulator
{
	public static class WrestlerDictionary
	{
		private static Dictionary<string, WrestlerData> allWrestlers;

		public static void Initialise () {
			allWrestlers = new Dictionary<string, WrestlerData> ();
			//Read a file and add moves to dictionary

			//Note: I tested Directiory.GetCurrentDirectory() sometimes it gives you the wrong file path
			string myDirectory = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);
			Console.WriteLine (myDirectory);
			myDirectory += "/Wrestlers";
			string[] files = Directory.GetFiles (myDirectory, "*.wrestler");
			for (ushort i = 0; i < files.Length; i++) {
				string allTextInFile = File.ReadAllText (files [i]);
				if (allTextInFile.Length > 0)
					AddToDictionary (allTextInFile);
			}
		}

		private static void AddToDictionary (string unparsedWrestlerString) {
			string key = UsefulActions.GetDataFromUnparsedFile (unparsedWrestlerString, "Name: ");

			WrestlerData wrestlerData = new WrestlerData (unparsedWrestlerString);
			if (allWrestlers.ContainsKey (key))
				throw new Exception ("The wrestler '"+key+"' already exists.");
			allWrestlers.Add (key, wrestlerData);
		}

		public static WrestlerData GetWrestler (string name) {
			if (!allWrestlers.ContainsKey (name)) //Do not want the game to crash when an incorrect name is given
				return null;
			return allWrestlers [name];
		}

		//For now, this function is terminal only. In the future it'll be adjusted to suit fitting into a GUI application
		public static void ListAllWrestlers () {
			foreach (string key in allWrestlers.Keys) {
				Console.WriteLine (key);
			}
		}
	}
}

