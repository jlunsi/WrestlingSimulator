using System;

namespace WrestlingSimulator {
	
	public struct PositionLayout {

		public WrestlerPosition currentPosition, currentOpponentPosition;

		public PositionLayout (WrestlerPosition currentPosition, WrestlerPosition currentOpponentPosition) {
			this.currentPosition = currentPosition;
			this.currentOpponentPosition = currentOpponentPosition;
		}
	}
}

