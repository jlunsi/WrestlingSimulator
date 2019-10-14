using System;
using System.Collections.Generic;

namespace WrestlingSimulator
{
	public abstract class Match
	{
		public static Match instance; //Current match type.
		protected Wrestler[] wrestlers;
		protected Wrestler[] winner;
		protected Environment[] availableLocations; //Locations available in the match. For more complex matches, to allow for the ability to exit the ring etc.
		protected float elapsedTime;
		protected float momentumReserve;

		private ushort minutesPassed;

		private const float MAX_RECOVERY_TIME = 30, RECOVERY_FALLOFF_TIME = 570; //These are private because I may use these in subclasses
		private const float MOMENTUM_SPEED = 0.25f;

		public Match () {
			winner = null;
			elapsedTime = 0;
			minutesPassed = 0;
		}

		public void ProcessMatch () {
			while (winner == null) {
				MatchStep ();
				//To make it easier to guage how much time has passed, the program will tell you every time a minute has passed
				if ((minutesPassed + 1) * 60 < elapsedTime) {
					minutesPassed++;
					if (minutesPassed == 1)
						Output.AddToOutput ("\n1 minute has passed...\n");
					else
						Output.AddToOutput("\n"+minutesPassed+" minutes have passed...\n");
				}
			}
			Output.DisplayMatch ();
		}

		public abstract void MatchStep ();

		//Finds the wrestler who will have an action first.
		protected Wrestler GetNextActiveWrestler () {
			Wrestler activeWrestler = wrestlers [0];
			float minimumTimer = activeWrestler.GetActionTimer ();
			//Output.AddToOutput (wrestlers [0].GetName () + "'s timer: " + minimumTimer);

			for (byte i = 1; i < wrestlers.Length; i++) {
				float currentTimer = wrestlers [i].GetActionTimer ();
				//Output.AddToOutput (wrestlers [i].GetName () + "'s timer: " + currentTimer);

				if (currentTimer < minimumTimer) {
					activeWrestler = wrestlers [i];
					minimumTimer = currentTimer;
					if (wrestlers [i].IsStunned ())
						minimumTimer /= 2;
				}
			}

			PassTime (minimumTimer);
			return activeWrestler;
		}

		protected List<Wrestler> GetOtherWrestlers (Wrestler ignoreWrestler) {
			List<Wrestler> result = new List<Wrestler> ();
			for (byte i = 0; i < wrestlers.Length; i++) {
				if (wrestlers [i] != ignoreWrestler)
					result.Add (wrestlers [i]);
			}
			return result;
		}

		protected bool CanUseFinisher (Wrestler attackingWrestler, Wrestler receivingWrestler) {
			return CanUseSpecialMove (attackingWrestler, receivingWrestler, 1);

		}

		protected bool CanUseSignature (Wrestler attackingWrestler, Wrestler receivingWrestler) {
			return CanUseSpecialMove (attackingWrestler, receivingWrestler, 2);
		}

		protected bool CanUseSpecialMove (Wrestler attackingWrestler, Wrestler receivingWrestler, float bonusChance) {
			float attackerStamina = attackingWrestler.GetStaminaAsAPercentage ();
			float receiverHealth = receivingWrestler.GetHealthAsAPercentage ();
			float chance = 5 + receiverHealth * 20;
			chance -= (1-attackerStamina) * 15;
			chance *= bonusChance;

			return UsefulActions.RandomiseChance (chance);
		}

		protected bool CanUseMove (Wrestler attackingWrestler, Wrestler defendingWrestler) {
			//Check to see if you can use a move in that position
			return true;
		}

		protected void ChangePosition (Wrestler wrestler, WrestlerPosition newPosition) {
			string outputText = null;
			switch (newPosition) {
			case WrestlerPosition.GROGGY:
				outputText = "{Attacker} gets up off of the mat";
				break;

			case WrestlerPosition.STANDING:
				outputText = "{Attacker} regains their bearings";
				break;

			case WrestlerPosition.GROUNDED:
				outputText = "{Attacker} drops down to the mat";
				break;

			case WrestlerPosition.CORNER:
				outputText = "{Attacker} moves to the corner";
				break;

			case WrestlerPosition.RUNNING:
				outputText = "{Attacker} runs and bounces off of the ropes";
				break;
			}

			outputText = FormatText (outputText, wrestler);
			Output.AddToOutput (outputText);

			wrestler.AddCooldown(0.5f);
			wrestler.ChangePosition(newPosition);
		}

		protected void ChangeReceiverPosition (Wrestler attackingWrestler, Wrestler receivingWrestler, WrestlerPosition newPosition) {
			string outputText = null;
			switch (newPosition) {
			case WrestlerPosition.GROGGY:
				outputText = "{Attacker} picks {Receiver} up off of the mat";
				break;

			case WrestlerPosition.STANDING:
				outputText = "{Attacker} stands {Receiver} back up";
				break;

			case WrestlerPosition.GROUNDED:
				outputText = "{Attacker} puts {Receiver} down onto the floor";
				break;

			case WrestlerPosition.CORNER:
				outputText = "{Attacker} whips {Receiver} into the corner";
				break;

			case WrestlerPosition.RUNNING:
				outputText = "{Attacker} irish whip's {Receiver}";
				break;
			}

			outputText = FormatText (outputText, attackingWrestler, receivingWrestler);
			Output.AddToOutput (outputText);

			attackingWrestler.AddCooldown (0.5f);
			receivingWrestler.AddStun (0.25f);
			receivingWrestler.ChangePosition (newPosition);
		}

		protected MoveResult AttemptMove (Wrestler attackingWrestler, Wrestler receivingWrestler, IMove move, MoveType moveType) {
			MoveResult result = MoveResult.NORMAL;
			switch (moveType) {
			case MoveType.NORMAL:
				result = attackingWrestler.AttemptMove (receivingWrestler, (MoveData)move);
				break;

			case MoveType.SIGNATURE:
				result = attackingWrestler.AttemptSignature (receivingWrestler, (SpecialMove)move);
				break;

			case MoveType.FINISHER:
				result = attackingWrestler.AttemptFinisher (receivingWrestler, (SpecialMove)move);
				break;
			}

			string textToBuffer = null;
			MoveData moveData = move.GetMove ();

			switch (result) {
			case MoveResult.NORMAL:
				textToBuffer = moveData.description;
				if (move is SpecialMove)
					textToBuffer += " executing the {Move}";
				break;

			case MoveResult.BLOCKED:
				textToBuffer = "{Receiver} blocked the {Move} attempt from {Attacker}";
				break;

			case MoveResult.COUNTERED:
				textToBuffer = "{Receiver} countered {Attacker}'s {Move} attempt";
				break;

			case MoveResult.DODGED:
				textToBuffer = "{Receiver} dodged {Attacker}'s {Move} attempt";
				break;
			}

			textToBuffer = FormatText (textToBuffer, attackingWrestler, receivingWrestler, move);
			Output.AddToOutput (textToBuffer);

			return result; //MoveResult is used as a return type so a subclass can decide what happens next
		}

		public float ExtractFromMomentumReserve (float value) {
			float totalExtracted = (value > momentumReserve) ? momentumReserve : value;
			momentumReserve -= totalExtracted;
			return totalExtracted;
		}

		public void AddToMomentumReserve (float value) {
			momentumReserve += value;
		}

		protected void PassTime (float value) {
			elapsedTime += value;
			momentumReserve += value * MOMENTUM_SPEED;
			for (byte i = 0; i < wrestlers.Length; i++) {
				wrestlers [i].PassTime (value);
			}
		}

		protected void UseMomentum (Wrestler wrestler, float value) {
			wrestler.UseMomentum (value);
		}

		protected string FormatText (string text, Wrestler wrestler) {
			text = text.Replace ("{Attacker}", wrestler.GetName ());

			return text;
		}

		//Replace words in move descriptions
		protected string FormatText (string text, Wrestler attackingWrestler, Wrestler receivingWrestler) {
			text = FormatText (text, attackingWrestler);
			text = text.Replace ("{Receiver}", receivingWrestler.GetName ());

			return text;
		}

		//Replace words in move descriptions
		protected string FormatText (string text, Wrestler attackingWrestler, Wrestler receivingWrestler, IMove move) {
			text = FormatText (text, attackingWrestler, receivingWrestler);
			text = text.Replace ("{Move}", move.GetName());

			return text;
		}

		//Wrestlers which won. Multiple wrestlers can win in some match types (i.e. tag match)
		protected void Win (Wrestler[] winningWrestlers) {
			Output.AddToOutput ("The match is over.");
			Output.AddToOutput (winningWrestlers [0].GetName() + " wins the match!");
			TimeSpan time = TimeSpan.FromSeconds (elapsedTime);
			string timeText = time.ToString ("mm':'ss");
			if (time.Hours > 0)
				timeText = time.ToString ("hh':'mm':'ss");
			Output.AddToOutput ("Time: " + timeText);
			winner = winningWrestlers;
		}

		public virtual float GetRecoveryMultiplier () {
			if (elapsedTime < MAX_RECOVERY_TIME)
				return 3;
			float value = elapsedTime - MAX_RECOVERY_TIME;
			if (value > RECOVERY_FALLOFF_TIME)
				return 0.5f;
			value /= RECOVERY_FALLOFF_TIME;
			return 3 - value * 2.5f;
		}

		//Mostly used to have wrestlers stay down for longer during multi-man matches, like in wrestling
		public virtual float StunnedMultiplier () {
			if (wrestlers.Length == 2)
				return 1;
			if (UsefulActions.RandomiseChance (wrestlers.Length * 5))
				return 2;
			return 1;
		}
	}
}

