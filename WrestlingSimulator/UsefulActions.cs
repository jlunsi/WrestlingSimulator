using System;

namespace WrestlingSimulator
{
	public static class UsefulActions
	{
		private static Random rand; //In the future, random will be changed, as it's not truly random.

		public static string GetDataFromUnparsedFile (string unparsedString, string label) {
			int startIndex = label.Length;
			startIndex += unparsedString.IndexOf (label);
			int endIndex = unparsedString.IndexOf ('\n', startIndex);
			int length = endIndex - startIndex;
			if (length == 0)
				throw new Exception ("The field: '" + label + "' is empty within a file.");
			string finalString = unparsedString.Substring (startIndex, length);
			finalString = finalString.Replace("\n", "");
			return finalString;
		}

		//Not a clamp per se, but stops me from dividing by 0
		public static float ClampValue (float value) {
			if (value < 0.01f)
				return 0.01f;
			return value;
		}

		//In many parts of the code, I'd divide a number by 2, then add some portion of that number
		//back to the number through using Random (i.e. if I have 10, I'd divide by 2, giving me 5,
		//then I'd multiply that 5 by a random number from 0.0 to 1.0)
		public static float RandomiseNumber (float value) {
			float halvedValue = value / 2;
			return halvedValue + halvedValue * (float)rand.NextDouble ();
		}

		//Randomise a floating point number between two values
		public static float RandomiseNumber (float low, float high) {
			if (high < low) { //Swap high and low if low is larger than high
				float temp = high;
				high = low;
				low = temp;
			}
			float difference = high - low;
			return low + difference * (float)rand.NextDouble ();
		}

		public static int RandomiseNumber (int low, int high) {
			if (high < low) { //Swap high and low if low is larger than high
				int temp = high;
				high = low;
				low = temp;
			}
			return rand.Next (low, high);
		}

		//Returns true if the generated number between 0-100 is lower than the value provided
		public static bool RandomiseChance (float value) {
			int chance = rand.Next (0, 100);
			return chance < value;
		}

		//Function that initialises random if it's null.
		public static void InitialiseRandom () {
			if (rand == null)
				rand = new Random ();
		}
	}
}

