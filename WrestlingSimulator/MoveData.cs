using System;

namespace WrestlingSimulator
{
	public class MoveData : IMove
	{
		public readonly string name;
		public readonly string description;
		public readonly StaminaCost staminaCost;
		public readonly OffenceType offenceType;
		public readonly WrestlerPosition requiredPosition, requiredOpponentPosition;
		public readonly WrestlerPosition finishedPosition, reversalPosition;
		public readonly BodyPart[] damagedBodyParts;
		public readonly float lowerMoveTime, upperMoveTime; //length of time it takes to perform the move

		public MoveData (string textToParse) {
			string textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Name: ");
			name = textFromFile;

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Description: ");
			description = textFromFile;

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Stamina Cost: ");
			switch (textFromFile.ToUpper ()) {
			case "VERY LOW":
				staminaCost = StaminaCost.VERY_LOW;
				break;
			case "LOW":
				staminaCost = StaminaCost.LOW;
				break;
			case "NORMAL":
				staminaCost = StaminaCost.NORMAL;
				break;
			case "HIGH":
				staminaCost = StaminaCost.HIGH;
				break;
			case "VERY HIGH":
				staminaCost = StaminaCost.VERY_HIGH;
				break;
			}

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Offence Type: ");
			switch (textFromFile.ToUpper ()) {
			case "GRAPPLE":
				offenceType = OffenceType.GRAPPLE;
				break;
			case "STRIKE":
				offenceType = OffenceType.STRIKE;
				break;
			case "DIVE":
				offenceType = OffenceType.FLYING;
				break;
			case "RUNNING":
				offenceType = OffenceType.RUNNING;
				break;
			}

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Required Position: ");
			requiredPosition = StringToPosition (textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Required Opponent Position: ");
			requiredOpponentPosition = StringToPosition (textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Finished Position: ");
			finishedPosition = StringToPosition (textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Reversal Position: ");
			reversalPosition = StringToPosition (textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Damaged Body Parts: ");
			string[] bodyParts = textFromFile.Split (new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			damagedBodyParts = new BodyPart[bodyParts.Length];
			for (byte i = 0; i < bodyParts.Length; i++) {
				switch (bodyParts [i].ToUpper ()) {
				case "HEAD":
					damagedBodyParts [i] = BodyPart.HEAD;
					break;
				case "TORSO":
					damagedBodyParts [i] = BodyPart.TORSO;
					break;
				case "ARMS":
					damagedBodyParts [i] = BodyPart.ARMS;
					break;
				case "LEGS":
					damagedBodyParts [i] = BodyPart.LEGS;
					break;
				default:
					throw new Exception ("Not a valid body part.");
				}
			}

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Lower Move Time: ");
			lowerMoveTime = float.Parse (textFromFile);

			textFromFile = UsefulActions.GetDataFromUnparsedFile (textToParse, "Upper Move Time: ");
			upperMoveTime = float.Parse (textFromFile);
		}

		public MoveData GetMove () {
			return this;
		}

		public string GetName () {
			return name;
		}

		public StaminaCost GetStaminaCost () {
			return staminaCost;
		}

		private WrestlerPosition StringToPosition (string text) {
			switch (text.ToUpper ()) {
			case "STANDING":
				return WrestlerPosition.STANDING;
			case "GROGGY":
				return WrestlerPosition.GROGGY;
			case "GROUNDED":
				return WrestlerPosition.GROUNDED;
			case "CORNER":
				return WrestlerPosition.CORNER;
			case "RUNNING":
				return WrestlerPosition.RUNNING;
			}

			if (text.ToUpper () != "TOP ROPE")
				throw new Exception ("Not a valid position.");
			return WrestlerPosition.TOP_ROPE;
		}
	}
}

