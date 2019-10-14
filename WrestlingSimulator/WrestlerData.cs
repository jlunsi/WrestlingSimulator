using System;
using System.IO;

namespace WrestlingSimulator {
	
	public class WrestlerData {

		//Description
		public readonly string name;
		public readonly WrestlingStyle wrestlingStyle;
		public readonly ushort weight; //Wouldn't make sense for it to be a float, and it probably won't go over 500, hence ushort
		//Main Stats
		public readonly float grappleStrength, strikingStrength, runningStrength, divingStrength;
		public readonly float heart, health, healthRecovery;
		public readonly float stamina, staminaRecovery;
		public readonly float headResistance, torsoResistance, armResistance, legResistance;
		public readonly float technique, speed, block, counter;
		public readonly float submission, awareness;
		//Moves
		public readonly MoveList myMoves;

		public WrestlerData (string textToParse) {
			/*string textToParse = File.ReadAllText (filePath);
			if (textToParse.Length == 0)
				throw new Exception ("File with path '" + filePath + "' is empty.");*/

			string textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Name: ");
			name = textFromFile;

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Wrestling Style: ");
			wrestlingStyle = GetWrestlingStyle(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Weight: ");
			weight = ushort.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Grapple Strength: ");
			grappleStrength = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Striking Strength: ");
			strikingStrength = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Running Strength: ");
			runningStrength = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Diving Strength: ");
			divingStrength = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Heart: ");
			heart = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Health: ");
			health = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Health Recovery: ");
			healthRecovery = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Stamina: ");
			stamina = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Stamina Recovery: ");
			staminaRecovery = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Head Resistance: ");
			headResistance = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Torso Resistance: ");
			torsoResistance = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Arms Resistance: ");
			armResistance = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Legs Resistance: ");
			legResistance = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Technique: ");
			technique = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Speed: ");
			speed = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Block: ");
			block = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Counter: ");
			counter = float.Parse(textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Submission: ");
			submission = float.Parse(textFromFile);

			myMoves = new MoveList ();

			//Add moves to the moves list
			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Finishers: ");
			while (textFromFile.Length > 1) {
				int indexOfComma = textFromFile.IndexOf (',');
				int indexOfIDStart = indexOfComma + 2;
				int indexOfEnd = textFromFile.IndexOf (']');

				string specialMoveName = textFromFile.Substring (1, indexOfComma - 1);
				string moveIDText = textFromFile.Substring (indexOfIDStart, indexOfEnd - indexOfIDStart);

				ushort moveID = ushort.Parse (moveIDText);
				MoveData originalMove = MoveDictionary.GetMove (moveID);

				PositionLayout requiredPosition = new PositionLayout (originalMove.requiredPosition, originalMove.requiredOpponentPosition);
				myMoves.AddToFinishers (requiredPosition, moveID, specialMoveName);
				textFromFile = textFromFile.Remove (0, indexOfEnd + 1);
				if (textFromFile.Length != 0)
					textFromFile = textFromFile.Remove (0, 2);
			}

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Signatures: ");
			while (textFromFile.Length > 1) {
				int indexOfComma = textFromFile.IndexOf (',');
				int indexOfIDStart = indexOfComma + 2;
				int indexOfEnd = textFromFile.IndexOf (']');

				string specialMoveName = textFromFile.Substring (1, indexOfComma - 1);
				string moveIDText = textFromFile.Substring (indexOfIDStart, indexOfEnd - indexOfIDStart);

				ushort moveID = ushort.Parse (moveIDText);
				MoveData originalMove = MoveDictionary.GetMove (moveID);

				PositionLayout requiredPosition = new PositionLayout (originalMove.requiredPosition, originalMove.requiredOpponentPosition);
				myMoves.AddToSignatures (requiredPosition, moveID, specialMoveName);
				textFromFile = textFromFile.Remove (0, indexOfEnd + 1);
				if (textFromFile.Length != 0)
					textFromFile = textFromFile.Remove (0, 2);
			}

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Favourite Moves: ");
			while (textFromFile.Length > 1) {
				int indexOfComma = textFromFile.IndexOf (',');
				if (indexOfComma == -1)
					indexOfComma = textFromFile.Length - 1;

				string moveIDText = textFromFile.Substring (0, indexOfComma);
				ushort moveID = ushort.Parse (moveIDText);
				MoveData originalMove = MoveDictionary.GetMove (moveID);

				PositionLayout requiredPosition = new PositionLayout (originalMove.requiredPosition, originalMove.requiredOpponentPosition);
				myMoves.AddToFavouriteMoves (requiredPosition, moveID);
				if (indexOfComma + 2 > textFromFile.Length)
					break;
				textFromFile = textFromFile.Remove (0, indexOfComma + 2);
			}

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Normal Moves: ");
			while (textFromFile.Length > 1) {
				int indexOfComma = textFromFile.IndexOf (',');
				if (indexOfComma == -1)
					indexOfComma = textFromFile.Length - 1;

				string moveIDText = textFromFile.Substring (0, indexOfComma);

				ushort moveID = ushort.Parse (moveIDText);
				MoveData originalMove = MoveDictionary.GetMove (moveID);

				PositionLayout requiredPosition = new PositionLayout (originalMove.requiredPosition, originalMove.requiredOpponentPosition);
				myMoves.AddToMoves (requiredPosition, moveID);
				if (indexOfComma + 2 > textFromFile.Length)
					break;
				textFromFile = textFromFile.Remove (0, indexOfComma + 2);
			}
		}

		private WrestlingStyle GetWrestlingStyle (string text) {
			switch (text.ToUpper ()) {
			case "GIANT":
				return WrestlingStyle.GIANT;
			}
			return WrestlingStyle.TECHNICAL;
		}
			
	}
}

