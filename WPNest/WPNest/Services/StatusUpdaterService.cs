using System;
using System.Threading;
using System.Threading.Tasks;

namespace WPNest.Services {
	internal interface IStatusUpdaterService {
		Thermostat CurrentThermostat { get; set; }
		void Start();
		void Stop();
		Task UpdateStatusAsync();
	}

	internal class StatusUpdaterService : IStatusUpdaterService {

		private readonly Timer _updateStatusTimer;
		private readonly IStatusProvider _delayedStatusProvider;

		public Thermostat CurrentThermostat { get; set; }

		public StatusUpdaterService() {
			_delayedStatusProvider = ServiceContainer.GetService<IStatusProvider>();
			_updateStatusTimer = new Timer(OnTimerTick);
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

			var nestWebService = ServiceContainer.GetService<INestWebService>();
			GetThermostatStatusResult temperatureResult = await nestWebService.GetThermostatStatusAsync(thermostat);
			if(temperatureResult.Exception != null)
				Stop();
			
			_delayedStatusProvider.CacheThermostatStatus(temperatureResult);
		}

		private async void OnTimerTick(object state) {
			await UpdateStatusAsync(CurrentThermostat);
		}
	}
}
