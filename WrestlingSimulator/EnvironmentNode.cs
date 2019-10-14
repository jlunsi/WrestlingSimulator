using System;
using System.Collections.Generic;

namespace WrestlingSimulator
{
	public class EnvironmentNode
	{
		private List<Wrestler> containingWrestlers;

		public EnvironmentNode () {
			containingWrestlers = new List<Wrestler> ();
		}

		public void AddWrestlerToNode (Wrestler wrestler) {
			containingWrestlers.Add (wrestler);
		}

		public void RemoveWrestlerFromNode (Wrestler wrestler) {
			containingWrestlers.Remove (wrestler);
		}

		public bool ContainsWrestler (Wrestler wrestler) {
			return containingWrestlers.Contains (wrestler);
		}
	}
}

