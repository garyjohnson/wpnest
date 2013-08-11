using System;

namespace WPNest {

	public class IsAwayChangedArgs : EventArgs {

		public IsAwayChangedArgs(bool isAway) {
			IsAway = isAway;
		}

		public bool IsAway { get; private set; }
	}
}
