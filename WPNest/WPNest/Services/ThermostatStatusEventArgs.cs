using System;

namespace WPNest.Services {
	public class StatusEventArgs : EventArgs {

		public GetStatusResult Status { get; private set; }
		public StatusEventArgs(GetStatusResult status) {
			Status = status;
		}

	}
}
