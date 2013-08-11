using System;

namespace WPNest {

	public class FanModeChangedArgs : EventArgs {

		public FanModeChangedArgs(FanMode fanMode) {
			FanMode = fanMode;
		}

		public FanMode FanMode { get; private set; }
	}
}
