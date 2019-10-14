using System;

namespace WrestlingSimulator
{
	//I originally wanted SpecialMove to inherit from MoveData but that led to some complications
	//(I wanted SpecialMove to just be a modified version of an original move)
	//So instead I used an interface to bridge the two together
	public interface IMove 
	{
		StaminaCost GetStaminaCost();
		string GetName();
		MoveData GetMove();
	}
}

