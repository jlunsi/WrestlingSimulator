using System;

namespace WrestlingSimulator {
	
	public class Wrestler {

		private WrestlerData myData;
		private float health, maxHealth, headHealth, torsoHealth, armHealth, legHealth;
		private float stamina;
		private float momentum;
		private float actionTimer, stunnedTimer;
		private WrestlerPosition currentPosition;
		private Wrestler targettingWrestler;
		private Environment currentEnvironment;
		private NodeIndex currentNodeIndex;
		private byte collapseCounter; //The more you collapse, the longer you stay on the floor for

		private const float MAX_HEALTH_POINTS = 1000, MAX_STAMINA = 200;
		private const float MOVE_BASE_STAMINA_COST = 5;
		private const float FINISHER_MOMENTUM_COST = 100, SIGNATURE_MOMENTUM_COST = 50;
		private const float QUICK_RECOVER_MOMENTUM_COST = 25;
		private const float MOVE_BASE_MOMENTUM_GAIN = 5;
		private const float QUICK_RECOVER_CHANCE_SCALER = 0.2f; //Couldn't come up with a better name
		private const float MOVE_STUN_MULTIPLIER = 0.4f;
		private const float PIN_MULTIPLIER = 5;

		public Wrestler (WrestlerData myData) {
			this.myData     = myData;
			health          = MAX_HEALTH_POINTS;
			maxHealth       = MAX_HEALTH_POINTS;
			headHealth      = MAX_HEALTH_POINTS;
			torsoHealth     = MAX_HEALTH_POINTS;
			armHealth       = MAX_HEALTH_POINTS;
			legHealth       = MAX_HEALTH_POINTS;
			stamina         = MAX_STAMINA;
			momentum        = 100;
			actionTimer     = 0;
			stunnedTimer    = 0;
			collapseCounter = 0;
			currentPosition = WrestlerPosition.STANDING;
		}

		public void TakeDamage (float value, BodyPart bodyPart) {
			float damageTaken = value;
			damageTaken *= CalculateResistanceDamageReduction (myData.health);
			float damageReduction;

			switch (bodyPart) {
				case BodyPart.HEAD:
					damageReduction = CalculateResistanceDamageReduction (myData.headResistance);
					damageTaken *= damageReduction;
					headHealth -= damageTaken;
					headHealth = (headHealth < 0) ? 0 : headHealth;
					damageTaken *= CalculateDamageToHealth(headHealth);
					break;

				case BodyPart.TORSO:
					damageReduction = CalculateResistanceDamageReduction (myData.torsoResistance);
					damageTaken *= damageReduction;
					torsoHealth -= damageTaken;
					torsoHealth = (torsoHealth < 0) ? 0 : torsoHealth;
					damageTaken *= CalculateDamageToHealth(torsoHealth);
					break;

				case BodyPart.ARMS:
					damageReduction = CalculateResistanceDamageReduction (myData.armResistance);
					damageTaken *= damageReduction;
					armHealth -= damageTaken;
					armHealth = (armHealth < 0) ? 0 : armHealth;
					damageTaken *= CalculateDamageToHealth(armHealth);
					break;

				case BodyPart.LEGS:
					damageReduction = CalculateResistanceDamageReduction (myData.legResistance);
					damageTaken *= damageReduction;
					legHealth -= damageTaken;
					legHealth = (legHealth < 0) ? 0 : legHealth;
					damageTaken *= CalculateDamageToHealth(legHealth);
					break;
			}

			health -= damageTaken;
		}

		public void ConsumeStamina (float value) {
			float totalStaminaConsumed = value / Match.instance.GetRecoveryMultiplier ();
			totalStaminaConsumed /= CalculateStaminaCostReduction ();

			if (totalStaminaConsumed > stamina) {
				Collapse ();
				totalStaminaConsumed = stamina;
			}

			stamina -= totalStaminaConsumed;
		}

		public float GetMoveDamage (float staminaCost, MoveData move) {
			float damage = GetDamageByOffenceType (move);
			Random rand = new Random ();

			damage /= 2;
			damage = damage + (float)rand.NextDouble () * damage;
			damage *= staminaCost; //Moves with a higher stamina cost deal more damage
			damage *= GetStaminaAsAPercentage ()* 1.25f; //Moves deal less damage if you have less stamina

			return damage;
		}

		public bool CanUseFinisher () {
			if (momentum < FINISHER_MOMENTUM_COST)
				return false;
			return myData.myMoves.CanUseFinisher (GetCurrentPositionLayout ());
		}

		public bool CanUseSignature () {
			if (momentum < SIGNATURE_MOMENTUM_COST)
				return false;
			return myData.myMoves.CanUseSignature (GetCurrentPositionLayout ());
		}

		public bool CanUseMove () {
			return myData.myMoves.CanUseMove (GetCurrentPositionLayout ());
		}

		private PositionLayout GetCurrentPositionLayout () {
			return new PositionLayout (currentPosition, targettingWrestler.GetPosition ());
		}

		public SpecialMove GetFinisher () {
			PositionLayout currentLayout = GetCurrentPositionLayout ();
			return myData.myMoves.GetFinisher (currentLayout);
		}

		public SpecialMove GetSignature () {
			PositionLayout currentLayout = GetCurrentPositionLayout ();
			return myData.myMoves.GetSignature (currentLayout);
		}

		public MoveData GetMove () {
			PositionLayout currentLayout = GetCurrentPositionLayout ();
			return myData.myMoves.GetRandomMove (currentLayout);
		}

		//Same as AttemptMove, but 4 times the damage (and risk)
		public MoveResult AttemptFinisher (Wrestler receivingWrestler, SpecialMove move) {
			UseMomentum (FINISHER_MOMENTUM_COST);
			return AttemptMove (receivingWrestler, move, 4);
		}

		//Same as AttemptMove, but 2 times the damage
		public MoveResult AttemptSignature (Wrestler receivingWrestler, SpecialMove move) {
			//PositionLayout currentLayout = GetCurrentPositionLayout ();
			UseMomentum (SIGNATURE_MOMENTUM_COST);
			return AttemptMove (receivingWrestler, move, 2);
		}

		public MoveResult AttemptMove (Wrestler receivingWrestler, float costMultiplier = 1) {
			PositionLayout currentLayout = GetCurrentPositionLayout ();
			return AttemptMove (receivingWrestler, myData.myMoves.GetRandomMove (currentLayout), costMultiplier);
		}

		//Left public because in the future there may be match specific moves
		public MoveResult AttemptMove (Wrestler receivingWrestler, IMove move, float costMultiplier = 1) {
			float staminaCost = 0.4f * (byte)move.GetStaminaCost() * costMultiplier;
			float maxSuccessChance = myData.technique / staminaCost;

			//Without increasing this value, people with less technique than someone's counter stat can never ever land a move
			maxSuccessChance *= UsefulActions.ClampValue (GetStaminaAsAPercentage ()) * 3;

			MoveData moveData = move.GetMove ();
			float damage = GetDamageByOffenceType (moveData) * staminaCost;
			damage = UsefulActions.RandomiseNumber (damage);

			ConsumeStamina (MOVE_BASE_STAMINA_COST * staminaCost);
			MoveResult result = receivingWrestler.ReceiveMove (maxSuccessChance, moveData, damage);
			receivingWrestler.ChangeTarget (this); //Without this, multi-man matches get kinda silly with someone getting beat up for free

			if (result == MoveResult.COUNTERED) {
				AddStun (receivingWrestler.GetCounterStunLength ());
				ChangePosition (moveData.reversalPosition);
				return result;
			}

			if (result == MoveResult.NORMAL) {
				AddMomentum (MOVE_BASE_MOMENTUM_GAIN + damage / 50); //No charisma stat, simply made it so that wrestlers who hit harder gain more momentum
			}

			float moveTime = UsefulActions.RandomiseNumber (moveData.lowerMoveTime, moveData.upperMoveTime); //How long the move took
			AddCooldown (moveTime);
			receivingWrestler.AddStun (moveTime);

			return result;
		}

		public MoveResult ReceiveMove (float moveMaxChance, MoveData move, float damage) {
			if (CanAvoidMove (moveMaxChance, myData.counter)) {
				ConsumeStamina (damage / 10);
				stunnedTimer = 0;
				actionTimer = 0;
				AddCooldown (0.4f); //Cooldown added as not all counters lead to an opportunity for the person who countered
				return MoveResult.COUNTERED;
			}

			if (CanAvoidMove (moveMaxChance, myData.speed)) {
				AddCooldown (0.2f); //Cooldown added, sometimes when someone ducks under a clothesline they get hit by the next, for example
				return MoveResult.DODGED;
			}
			
			if (CanAvoidMove (moveMaxChance, myData.block)) {
				HitByMove (move, damage / 10);
				return MoveResult.BLOCKED;
			}

			HitByMove (move, damage);
			ChangePosition (move.finishedPosition);
			return MoveResult.NORMAL;
		}

		private void HitByMove (MoveData move, float damage) {
			AddStun (damage * MOVE_STUN_MULTIPLIER);
			float finalDamage = damage / move.damagedBodyParts.Length;
			for (byte i = 0; i < move.damagedBodyParts.Length; i++)
				TakeDamage (finalDamage, move.damagedBodyParts [i]);
		}

		public void ReduceMaxHealth () {
			maxHealth = (maxHealth + health) / 2;
		}

		public bool IsStunned () {
			return stunnedTimer > 0.01f;
		}

		//Time remaining for the next move. Decided to allow the ability for stunned opponents to attack next too, but non-stunned opponents have priority.
		public float GetActionTimer () {
			return actionTimer + (stunnedTimer * 2);
		}

		public float GetCounterStunLength () {
			return myData.counter / 50.0f;
		}

		public WrestlerPosition GetPosition () {
			return currentPosition;
		}

		public void ChangePosition (WrestlerPosition newPosition) {
			currentPosition = newPosition;
			if (!IsStunned ())
				AddCooldown (0.4f);
		}

		public void AddCooldown (float value) {
			float valueToAdd = value;

			//In real wrestling, wrestlers tend to perform slower as they take more damage and run out of breath. Due to that, that increases the cooldown
			float temp = UsefulActions.ClampValue (GetStaminaAsAPercentage ());
			valueToAdd *= (1 - temp) + 1; //Lower stamina leads to a longer cooldown
			temp = UsefulActions.ClampValue (GetHealthAsAPercentage ());
			valueToAdd *= (1 - temp) + 1; //Lower health also does

			//Add ambient action time depending on stamina, so the wrestler doesn't attack instantly, which doesn't always happen in wrestling.
			valueToAdd += UsefulActions.RandomiseNumber (0.1f, 6.0f - GetStaminaAsAPercentage () * 4.0f);

			actionTimer += valueToAdd;
		}




		public void AddStun (float value) {
			float valueToAdd = UsefulActions.RandomiseNumber (value);

			float clampedValue = GetHealthAsAPercentage ();
			clampedValue = UsefulActions.ClampValue(clampedValue); //Allowing the value to equal 0 leads to infinity, which bugs the game
			valueToAdd /= clampedValue; //Recover slower the lower your health is

			clampedValue = GetStaminaAsAPercentage ();
			clampedValue = UsefulActions.ClampValue(clampedValue); //Allowing the value to equal 0 leads to infinity, which bugs the game
			valueToAdd /= clampedValue; //Recover slower the lower your stamina is

			valueToAdd /= Match.instance.GetRecoveryMultiplier ();
			valueToAdd *= Match.instance.StunnedMultiplier ();

			valueToAdd = (IsStunned ()) ? valueToAdd / 2 : valueToAdd; //Reduce stun added to already stunned targets. Essentially a softcap
			if (UsefulActions.RandomiseChance (momentum * QUICK_RECOVER_CHANCE_SCALER)) {
				valueToAdd /= 10; //Recover quicker. Reduce the length of time you'll be stunned for.
				UseMomentum (QUICK_RECOVER_MOMENTUM_COST);
			}

			stunnedTimer += valueToAdd;
			actionTimer = 0; //Wouldn't make sense to be put on cooldown from a move whilst you're on the ground.

			if (stunnedTimer > 300) //Don't allow wrestlers to be stunned for too long (but don't limit it entirely. doing so can ruin multi-man matches).
				stunnedTimer = 300;
		}

		//Wrestler enters a self-stun
		private void Collapse () {
			float maxStun = 10f * 0.01f * ++collapseCounter;
			//maxStun must be reduced to prevent it from getting out of hand as it will be divided by 0.01 due to low stamina
			//For example, if the base stun is 5, the stun will be increased to 500 if we do not multiply by 0.01 beforehand

			Output.AddToOutput (myData.name + " collapses onto the floor"); //This appears before the move as of right now. Bug to be fixed
			maxStun /= CalculateStaminaCostReduction ();
			AddStun (maxStun);
			ChangePosition (WrestlerPosition.GROUNDED);
		}

		protected bool CanAvoidMove (float moveMaxChance, float avoidMaxChance) {
			float moveChance = UsefulActions.RandomiseNumber (0.0f, moveMaxChance);
			float avoidChance = UsefulActions.RandomiseNumber (0.0f, avoidMaxChance);

			avoidChance *= GetStaminaAsAPercentage (); //Lower chance to avoid if you have low stamina (not health, that will snowball too hard)
			avoidChance = (IsStunned ()) ? avoidChance * 0.6f : avoidChance; //Lower chance to avoid if you're stunned

			return moveChance < avoidChance;
		}

		private float GetDamageByOffenceType (MoveData move) {
			switch (move.offenceType) {
			case OffenceType.GRAPPLE:
				return myData.grappleStrength;

			case OffenceType.RUNNING:
				return myData.runningStrength;

			case OffenceType.FLYING:
				return myData.divingStrength;
			}

			return myData.strikingStrength;
		}

		private float CalculateDamageToHealth (float limbHealth) {
			return 4.0f - (limbHealth / MAX_HEALTH_POINTS) * 4.0f;
		}

		private float CalculateStaminaCostReduction () {
			return myData.stamina / 20.0f;
		}

		private float CalculateResistanceDamageReduction (float value) {
			return 1 - (value / 100.0f) * 0.75f;
		}

		private float CalculateRecovery (float value) {
			return value / 50.0f;
		}

		public bool AttemptPinEscape (float chanceMultiplier) {
			float chance = GetHealthAsAPercentage () * myData.heart;

			//Wrestlers sometimes lose simply through being gassed, not through being beat up a lot.
			//There is a threshold so that the wrestler doesn't solely lose due to them being tired. It's just a factor.
			chance *= 0.25f + GetStaminaAsAPercentage () * 0.75f;

			//Multiplier below is used to help wrestlers kick out. Without it, wrestlers with low heart (i.e. 40) will never kickout, losing often in < 30 seconds
			chance *= PIN_MULTIPLIER * chanceMultiplier * Match.instance.GetRecoveryMultiplier (); //It can technically go over 100 at this point. That simply means you will kick out

			//Wrestlers stunned for longer (i.e. from a big move, or in this simulator, multiple moves) should have a lower chance of kicking out
			chance -= stunnedTimer / 10;

			bool kickedOut = UsefulActions.RandomiseChance (chance);
			if (kickedOut) {
				Output.AddToOutput (myData.name + " kicked out");
				ReduceMaxHealth ();
			}

			return kickedOut;
		}

		public void PassTime (float value) {
			actionTimer -= value;
			if (actionTimer < 0)
				actionTimer = 0;
			
			if (IsStunned ()) {
				float elapsedValue = value;
				stunnedTimer -= value;
				if (stunnedTimer < 0) {
					elapsedValue += stunnedTimer;
					stunnedTimer = 0;
				}
				stamina += elapsedValue * Match.instance.GetRecoveryMultiplier () * CalculateRecovery(myData.staminaRecovery); //Stamina recovers faster for those that are stunned
			}

			stamina += value * Match.instance.GetRecoveryMultiplier () * CalculateRecovery (myData.staminaRecovery) / 20.0f; //Stamina recovers slower for those that aren't
			stamina = (stamina > MAX_STAMINA) ? MAX_STAMINA : stamina;

			health += value * CalculateRecovery (myData.healthRecovery) * Match.instance.GetRecoveryMultiplier ();
			health = (health > maxHealth) ? maxHealth : health;
		}

		public bool MomentumLargerThan (float value) {
			return momentum > value;
		}

		public void AddMomentum (float value) {
			momentum += Match.instance.ExtractFromMomentumReserve (value);
		}

		public void UseMomentum (float value) {
			momentum -= value;
			Match.instance.AddToMomentumReserve (value);
		}

		public void ChangeTarget (Wrestler wrestler) {
			targettingWrestler = wrestler;
		}

		public void ChangeTargetToSelection (Wrestler[] selectableWrestlers) {
			targettingWrestler = selectableWrestlers [0];
			for (byte i = 1; i < selectableWrestlers.Length; i++) {
				byte chance = 20;

				if (selectableWrestlers [i].IsStunned ())
					chance = 5;
				if (selectableWrestlers [i].IsTargettingWrestler (this))
					chance = 50;
				
				if (UsefulActions.RandomiseChance (chance)) {
					targettingWrestler = selectableWrestlers [i];
					break;
				}
			}
		}

		public void SetEnvironment (Environment newEnvironment, NodeIndex newIndex) {
			currentEnvironment = newEnvironment;
			currentNodeIndex = newIndex;
		}

		public NodeIndex GetNodeIndex () {
			return currentNodeIndex;
		}

		public float GetStaminaAsAPercentage () {
			return stamina / MAX_STAMINA;
		}

		public float GetHealthAsAPercentage () {
			return health / MAX_HEALTH_POINTS;
		}

		public string GetName () {
			return myData.name;
		}

		public Wrestler GetTargettingWrestler () {
			return targettingWrestler;
		}

		public bool IsTargettingWrestler (Wrestler wrestler) {
			return targettingWrestler == wrestler;
		}

		public bool IsWrestlerInSameNode (Wrestler wrestler) {
			return currentEnvironment.ContainsWrestler (wrestler, currentNodeIndex);
		}
	}
}

