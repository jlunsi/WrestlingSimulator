using System;
using System.Collections.Generic;

namespace WrestlingSimulator
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			UsefulActions.InitialiseRandom ();
			MoveDictionary.Initialise ();
			WrestlerDictionary.Initialise ();

			Console.WriteLine ("Welcome to my Wrestling Simulator!");
			Console.WriteLine ("Currently you can only do pin-fall only matches which require at least 2 wrestlers (can do multi-man matches).");
			Console.WriteLine ("\nHere's the list of all available wrestlers:");
			WrestlerDictionary.ListAllWrestlers ();
			Console.WriteLine ("\nList all wrestlers you wish to add into the match, then type START when you're done.");

			List<Wrestler> wrestlersInMatch = new List<Wrestler> ();
			string userInput = "";
			while (!userInput.Equals ("START")) {
				userInput = Console.ReadLine ();
				WrestlerData wrestlerDetails = WrestlerDictionary.GetWrestler (userInput);
				if (wrestlerDetails == null) {
					Console.WriteLine ("The wrestler: '{0}' does not exist.", userInput);
					continue;
				}
				Wrestler wrestler = new Wrestler (wrestlerDetails);
				wrestlersInMatch.Add (wrestler);

				Console.WriteLine ("{0} has been added.", userInput);
			}

			/*Wrestler wrestlerA = new Wrestler (WrestlerDictionary.GetWrestler ("Bob Dylan"));
			Wrestler wrestlerB = new Wrestler (WrestlerDictionary.GetWrestler ("Jake Matthews"));
			Wrestler wrestlerC = new Wrestler (WrestlerDictionary.GetWrestler ("Michael Lopez"));
			Wrestler[] wrestlerList = new Wrestler[] { wrestlerA, wrestlerB, wrestlerC };*/

			Match.instance = new NormalMatch (wrestlersInMatch.ToArray());
			Match.instance.ProcessMatch ();

			Console.WriteLine ("The program will end after you press enter.");
			Console.ReadLine ();
		}
	}
}
