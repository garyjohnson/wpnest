using System;

namespace WPNest {

	public class HvacModeChangedArgs : EventArgs {

		public HvacModeChangedArgs(HvacMode hvacMode) {
			HvacMode = hvacMode;
		}

		public HvacMode HvacMode { get; private set; }
	}
}
