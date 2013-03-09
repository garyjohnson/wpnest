using System;
using System.Threading;
using System.Threading.Tasks;

namespace WPNest.Services {

	internal class StatusUpdaterService : IStatusUpdaterService {

		private readonly ITimer _updateStatusTimer;
		private readonly IStatusProvider _delayedStatusProvider;
		private readonly INestWebService _nestWebService;

		public Structure CurrentStructure { get; set; }

		public StatusUpdaterService() {
			_nestWebService = ServiceContainer.GetService<INestWebService>();
			_delayedStatusProvider = ServiceContainer.GetService<IStatusProvider>();
			_updateStatusTimer = ServiceContainer.GetService<ITimer>();
			_updateStatusTimer.SetCallback(OnTimerTick);
		}

		public void Start() {
			_updateStatusTimer.Change(2000, 5000);
		}

		public void Stop() {
			_updateStatusTimer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		public async Task UpdateStatusAsync() {
			await UpdateStatusAsync(CurrentStructure);
		}

		private async Task UpdateStatusAsync(Structure structure) {
			_delayedStatusProvider.Reset();

			GetStatusResult result = await _nestWebService.GetStructureAndDeviceStatusAsync(structure);
			if (result.Exception != null)
				Stop();

			_delayedStatusProvider.CacheStatus(result);
		}

		private async void OnTimerTick(object state) {
			await UpdateStatusAsync(CurrentStructure);
		}
	}
}
