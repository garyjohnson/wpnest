using System;
using System.Threading;

namespace WPNest.Services {

	public class TimerWrapper : ITimer {

		private Timer _timer;

		public void SetCallback(TimerCallback callback) {
			if (_timer != null)
				_timer.Dispose();

			_timer = new Timer(callback);	
		}

		public bool Change(int dueTime, int period) {
			if(_timer == null)
				throw new InvalidOperationException("SetCallback must be called first.");

			return _timer.Change(dueTime, period);
		}
	}
}
