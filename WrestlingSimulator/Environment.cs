using System;

namespace WrestlingSimulator {
	
	public class Environment {
		
		private EnvironmentNode[,] allNodes;

		public Environment () {
			allNodes = new EnvironmentNode[3, 3];
			for (byte x = 0; x < 3; x++) {
				for (byte y = 0; y < 3; y++) {
					allNodes [y, x] = new EnvironmentNode ();
				}
			}
		}

		public void SetWrestlerNode (Wrestler wrestler, NodeIndex index) {
			if (!IsValidNode (index))
				throw new Exception ("Node is not valid.");
			wrestler.SetEnvironment (this, index);
			allNodes [index.y, index.x].AddWrestlerToNode (wrestler);
		}

		//Move without a destination
		public void MoveToAdjacentNode (Wrestler wrestler, NodeIndex start) {
			NodeIndex destination;
			do {
				destination = start + new NodeIndex((sbyte)UsefulActions.RandomiseNumber(-1,2), (sbyte)UsefulActions.RandomiseNumber(-1,2));
			} while (!IsValidNode(destination));
			MoveToAdjacentNode (wrestler, start, destination);
		}

		//Move with a destination
		public void MoveToAdjacentNode (Wrestler wrestler, NodeIndex start, NodeIndex destination) {
			if (!IsValidNode (start) || !IsValidNode (destination))
				throw new Exception ("The path you requested cannot be done.");
			NodeIndex difference = start - destination;

			if (Math.Abs (start.x) > 1) {
				NodeIndex newDestination = start + new NodeIndex ((sbyte)Math.Sign (difference.x), (sbyte)0);
				MoveToAdjacentNode (wrestler, start, newDestination);
				MoveToAdjacentNode (wrestler, newDestination, destination);
			}

			if (Math.Abs (start.y) > 1) {
				NodeIndex newDestination = start + new NodeIndex ((sbyte)0, (sbyte)Math.Sign (difference.y));
				MoveToAdjacentNode (wrestler, start, newDestination);
				MoveToAdjacentNode (wrestler, newDestination, destination);
			}

			allNodes [start.y, start.x].RemoveWrestlerFromNode (wrestler);
			allNodes [destination.y, destination.x].AddWrestlerToNode (wrestler);
			wrestler.SetEnvironment (this, destination);
			wrestler.AddCooldown (0.5f);
		}

		public bool ContainsWrestler (Wrestler wrestler, NodeIndex nodePosition) {
			return allNodes [nodePosition.y, nodePosition.x].ContainsWrestler (wrestler);
		}

		private bool IsValidNode (NodeIndex index) {
			return index.x >= 0 && index.x < allNodes.GetLength (1) &&
				index.y >= 0 && index.y < allNodes.GetLength (0);
		}
	}
}

