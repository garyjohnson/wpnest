using System.Threading;

namespace WPNest.Services {

	public interface ITimer {
		void SetCallback(TimerCallback callback);
		bool Change(int dueTime, int period);
	}
}
