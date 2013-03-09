using System;
using System.Threading;
using System.Threading.Tasks;

namespace WPNest.Services {

	internal class StatusUpdaterService : IStatusUpdaterService {

		private readonly ITimer _updateStatusTimer;
		private readonly IStatusProvider _delayedStatusProvider;
		private readonly INestWebService _nestWebService;

		public Thermostat CurrentThermostat { get; set; }

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
			await UpdateStatusAsync(CurrentThermostat);
		}

		private async Task UpdateStatusAsync(Thermostat thermostat) {
			_delayedStatusProvider.Reset();

			GetStatusResult result = await _nestWebService.GetStructureAndDeviceStatusAsync(new Structure(""));

			_delayedStatusProvider.CacheStatus(result);

//			GetThermostatStatusResult temperatureResult = await _nestWebService.GetThermostatStatusAsync(thermostat);
//			if(temperatureResult.Exception != null)
//				Stop();
			
//			_delayedStatusProvider.CacheStatus(temperatureResult);
		}

		private async void OnTimerTick(object state) {
			await UpdateStatusAsync(CurrentThermostat);
		}
	}
}
