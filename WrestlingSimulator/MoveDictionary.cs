using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace WrestlingSimulator
{
	public static class MoveDictionary
	{
		private static Dictionary<ushort, MoveData> allMoves;

		public static void Initialise () {
			allMoves = new Dictionary<ushort, MoveData> ();
			//Read a file and add moves to dictionary

			//Note: I tested Directiory.GetCurrentDirectory() sometimes it gives you the wrong file path
			string myDirectory = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);
			Console.WriteLine (myDirectory);
			myDirectory += "/Moves";
			string[] files = Directory.GetFiles (myDirectory, "*.move");
			for (ushort i = 0; i < files.Length; i++) {
				string allTextInFile = File.ReadAllText (files [i]);
				while (allTextInFile.Length > 0) {
					AddToDictionary (allTextInFile);
					int lastIndex = allTextInFile.IndexOf ("Upper Move Time:");
					lastIndex = allTextInFile.IndexOf ('\n', lastIndex);
					allTextInFile = allTextInFile.Remove (0, lastIndex);
					allTextInFile = allTextInFile.TrimStart ('\n');
				}
			}
			//Directory.GetF
		}

		private static void AddToDictionary (string unparsedMoveString) {
			string textFromFile = UsefulActions.GetDataFromUnparsedFile (unparsedMoveString, "ID: ");
			ushort moveID = ushort.Parse(textFromFile);

			//Remove the ID from the string
			MoveData newMove = new MoveData(unparsedMoveString);
			if (allMoves.ContainsKey (moveID))
				throw new Exception ("The ID: "+moveID+" already exists.");
			allMoves.Add (moveID, newMove);
		}

		public static MoveData GetMove (ushort moveID) {
			return allMoves [moveID];
		}
	}
}

