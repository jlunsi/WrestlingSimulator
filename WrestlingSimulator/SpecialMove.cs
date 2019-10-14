using System;

namespace WrestlingSimulator
{
	public class SpecialMove : IMove
	{
		public readonly string specialName;
		public readonly MoveData originalMove;


		public SpecialMove (MoveData originalMove, string specialName)
		{
			//Set the new move name
			this.specialName = specialName;
			this.originalMove = originalMove;
		}

		public MoveData GetMove () {
			return originalMove;
		}

		public string GetName () {
			return specialName;
		}

		//Overridden, special moves cost more stamina (if possible)
		public StaminaCost GetStaminaCost () {
			byte staminaCost = (byte)(originalMove.GetStaminaCost() + 1);
			staminaCost = (staminaCost > 5) ? (byte)5 : staminaCost;
			return (StaminaCost)staminaCost;
		}
	}
}

