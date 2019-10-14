using System;
using System.Collections.Generic;

namespace WrestlingSimulator
{
	public class NormalMatch : Match
	{
		public NormalMatch (Wrestler wrestler1, Wrestler wrestler2) {
			availableLocations = new Environment[1];
			availableLocations [0] = new Environment ();

			wrestlers = new Wrestler[2];
			wrestlers [0] = wrestler1;
			wrestlers [1] = wrestler2;
			wrestler1.ChangeTarget (wrestler2);
			wrestler2.ChangeTarget (wrestler1);

			NodeIndex defaultPosition = new NodeIndex ((sbyte)1, (sbyte)1);
			availableLocations [0].SetWrestlerNode (wrestler1, defaultPosition);
			availableLocations [0].SetWrestlerNode (wrestler2, defaultPosition);

			momentumReserve = 0;
		}

		public NormalMatch (Wrestler[] wrestlers) {
			availableLocations = new Environment[1];
			availableLocations [0] = new Environment ();

			if (wrestlers.Length < 2)
				throw new Exception ("Not enough wrestlers for a match.");
			this.wrestlers = wrestlers;

			NodeIndex defaultPosition = new NodeIndex ((sbyte)1, (sbyte)1);
			for (ushort i = 0; i < wrestlers.Length; i++) {
				availableLocations [0].SetWrestlerNode (wrestlers [i], defaultPosition);
				List<Wrestler> otherWrestlers = GetOtherWrestlers (wrestlers [i]);
				wrestlers [i].ChangeTargetToSelection (otherWrestlers.ToArray ());
			}

			momentumReserve = 0;
		}

		protected void PinAttempt (Wrestler attackingWrestler, Wrestler receivingWrestler, float chanceMultiplier) {
			Output.AddToOutput (FormatText ("{Attacker} gets into a pinning position", attackingWrestler, receivingWrestler));
			Output.AddToOutput ("The referee starts counting");

			if (receivingWrestler.AttemptPinEscape (0.125f * chanceMultiplier))
				return;
			if (CanWrestlerBreakPin (attackingWrestler, receivingWrestler))
				return;

			Output.AddToOutput ("...1...");
			PassTime (1);
			if (receivingWrestler.AttemptPinEscape (0.5f * chanceMultiplier))
				return;
			if (CanWrestlerBreakPin (attackingWrestler, receivingWrestler))
				return;

			Output.AddToOutput ("...2...");
			PassTime (1);
			if (receivingWrestler.AttemptPinEscape (2 * chanceMultiplier))
				return;
			if (CanWrestlerBreakPin (attackingWrestler, receivingWrestler))
				return;

			Output.AddToOutput ("3!");
			PassTime (1);
			Win (new Wrestler[] { attackingWrestler });
		}

		private bool CanWrestlerBreakPin (Wrestler attackingWrestler, Wrestler receivingWrestler) {
			if (wrestlers.Length == 2)
				return false; //one-on-one match, no one else can break up the pin
			
			for (ushort i = 0; i < wrestlers.Length; i++) {
				if (wrestlers [i] == attackingWrestler || wrestlers [i] == receivingWrestler)
					continue;
				if (wrestlers [i].GetActionTimer () < 0.01f) {
					if (wrestlers [i].IsWrestlerInSameNode (attackingWrestler)) {
						string textToOutput = "{Attacker} has broken up {Receiver}'s pin";
						wrestlers [i].ChangeTarget (attackingWrestler);
						attackingWrestler.ChangeTarget (wrestlers [i]);
						textToOutput = FormatText (textToOutput, wrestlers [i], attackingWrestler);
						Output.AddToOutput (textToOutput);
						return true;
					}
				}
			}

			return false;
		}

		//This will be more complex in the future. It's mostly set like this right now to show that it all works.
		public override void MatchStep () {
			//Output.AddToOutput (elapsedTime.ToString());
			Wrestler attackingWrestler = GetNextActiveWrestler ();
			Wrestler receivingWrestler = attackingWrestler.GetTargettingWrestler ();
			float pinMultiplier = 4;

			//In multiman matches, randomly change targets if your target is stunned
			if (wrestlers.Length > 2 && receivingWrestler.IsStunned () && UsefulActions.RandomiseChance (25)) {
				Wrestler[] otherWrestlers = GetOtherWrestlers (attackingWrestler).ToArray ();
				attackingWrestler.ChangeTargetToSelection (otherWrestlers);
			}

			if (attackingWrestler.GetPosition () != WrestlerPosition.STANDING) {
				if (attackingWrestler.GetPosition () == WrestlerPosition.GROUNDED)
					ChangePosition (attackingWrestler, WrestlerPosition.GROGGY);
				else
					ChangePosition (attackingWrestler, WrestlerPosition.STANDING);
				return;
			}

			IMove attackToUse = null;
			MoveType moveType = MoveType.NORMAL;
			if (CanUseFinisher (attackingWrestler, receivingWrestler)) {
				attackToUse = attackingWrestler.GetFinisher ();
				moveType = MoveType.FINISHER;
				pinMultiplier = 0.5f;
			}
			else if (CanUseSignature (attackingWrestler, receivingWrestler)) {
				attackToUse = attackingWrestler.GetSignature ();
				moveType = MoveType.SIGNATURE;
				pinMultiplier = 1;
			}
			else if (CanUseMove (attackingWrestler, receivingWrestler)) {
				attackToUse = attackingWrestler.GetMove ();
				moveType = MoveType.NORMAL;
				pinMultiplier = 2;
			}

			if (attackToUse != null) {
				MoveResult moveResult = AttemptMove (attackingWrestler, receivingWrestler, attackToUse, moveType);
				if (moveResult != MoveResult.NORMAL) //Stop the turn immediately if our move didn't go through
					return;
			}

			if (attackingWrestler.IsStunned ()) //If the wrestler collapsed by any chance
				return;

			if (receivingWrestler.GetPosition () == WrestlerPosition.GROUNDED) {
				if (UsefulActions.RandomiseChance (49.0f / pinMultiplier) && attackToUse != null || UsefulActions.RandomiseChance(10)) {
					PinAttempt (attackingWrestler, receivingWrestler, pinMultiplier);
					return;
				}
				if (UsefulActions.RandomiseChance (40)) {
					ChangeReceiverPosition (attackingWrestler, receivingWrestler, WrestlerPosition.GROGGY);
					return;
				}
			}
		}
	}
}

