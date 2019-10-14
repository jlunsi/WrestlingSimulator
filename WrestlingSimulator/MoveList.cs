using System;
using System.Collections.Generic;

namespace WrestlingSimulator
{
	public class MoveList
	{
		private Dictionary<PositionLayout, List<SpecialMove>> finishers;
		private Dictionary<PositionLayout, List<SpecialMove>> signatures;
		private Dictionary<PositionLayout, List<MoveData>> favouriteMoves;
		private Dictionary<PositionLayout, List<MoveData>> normalMoves;

		public MoveList () {
			finishers      = new Dictionary<PositionLayout, List<SpecialMove>> ();
			signatures     = new Dictionary<PositionLayout, List<SpecialMove>> ();
			favouriteMoves = new Dictionary<PositionLayout, List<MoveData>> ();
			normalMoves    = new Dictionary<PositionLayout, List<MoveData>> ();
		}

		public void AddToFinishers (PositionLayout requiredPosition, ushort moveID, string finisherName) {
			AddToSpecialMoveDictionary (finishers, requiredPosition, moveID, finisherName);
		}

		public void AddToSignatures (PositionLayout requiredPosition, ushort moveID, string signatureName) {
			AddToSpecialMoveDictionary (signatures, requiredPosition, moveID, signatureName);
		}

		public void AddToFavouriteMoves (PositionLayout requiredPosition, ushort moveID) {
			AddToDictionary (favouriteMoves, requiredPosition, moveID);
		}

		public void AddToMoves (PositionLayout requiredPosition, ushort moveID) {
			AddToDictionary (normalMoves, requiredPosition, moveID);
		}

		public bool CanUseFinisher (PositionLayout requiredPosition) {
			if (finishers.ContainsKey (requiredPosition)) {
				if (finishers [requiredPosition].Count > 0)
					return true;
			}
			return false;
		}

		public bool CanUseSignature (PositionLayout requiredPosition) {
			if (signatures.ContainsKey (requiredPosition)) {
				if (signatures [requiredPosition].Count > 0)
					return true;
			}
			return false;
		}

		public bool CanUseMove (PositionLayout requiredPosition) {
			if (favouriteMoves.ContainsKey (requiredPosition)) {
				if (favouriteMoves [requiredPosition].Count > 0)
					return true;
			}
			if (normalMoves.ContainsKey (requiredPosition)) {
				if (normalMoves [requiredPosition].Count > 0)
					return true;
			}
			return false;
		}

		private void AddToDictionary (Dictionary<PositionLayout, List<MoveData>> myDictionary, PositionLayout requiredPosition, ushort moveID) {
			MoveData move = MoveDictionary.GetMove (moveID);
			List<MoveData> moves = null;

			if (myDictionary.ContainsKey (requiredPosition)) {
				moves = myDictionary [requiredPosition];
				moves.Add (move); //We're adding to a reference, so we do not have to readd the list to the dictionary
				return;
			}

			moves = new List<MoveData> ();
			moves.Add (move);
			myDictionary.Add (requiredPosition, moves);
		}

		private void AddToSpecialMoveDictionary (Dictionary<PositionLayout, List<SpecialMove>> myDictionary, PositionLayout requiredPosition, ushort moveID, string specialName) {
			MoveData move = MoveDictionary.GetMove (moveID);
			SpecialMove specialMove = new SpecialMove (move, specialName);
			List<SpecialMove> moves = null;

			if (myDictionary.ContainsKey (requiredPosition)) {
				moves = myDictionary [requiredPosition];
				moves.Add (specialMove); //We're adding to a reference, so we do not have to readd the list to the dictionary
				return;
			}

			moves = new List<SpecialMove> ();
			moves.Add (specialMove);
			myDictionary.Add (requiredPosition, moves);
		}

		public MoveData GetRandomMove (PositionLayout requiredPosition) {
			List<MoveData> moves = null;
			if (favouriteMoves.ContainsKey (requiredPosition) && UsefulActions.RandomiseChance (80)) {
				moves = favouriteMoves [requiredPosition];
			}

			if (normalMoves.ContainsKey (requiredPosition)) {
				moves = normalMoves [requiredPosition];
			}

			if (moves == null || moves.Count == 0)
				return null;
			return moves [UsefulActions.RandomiseNumber (0, moves.Count)];
		}

		public SpecialMove GetFinisher (PositionLayout requiredPosition) {
			if (finishers.ContainsKey (requiredPosition)) {
				List<SpecialMove> moveIDs = finishers [requiredPosition];
				return moveIDs [UsefulActions.RandomiseNumber (0, moveIDs.Count)];
			}
			return null;
		}

		public SpecialMove GetSignature (PositionLayout requiredPosition) {
			if (signatures.ContainsKey (requiredPosition)) {
				List<SpecialMove> moveIDs = signatures [requiredPosition];
				return moveIDs [UsefulActions.RandomiseNumber (0, moveIDs.Count)];
			}
			return null;
		}
	}
}

