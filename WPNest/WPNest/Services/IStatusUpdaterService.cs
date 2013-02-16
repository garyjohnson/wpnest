using System.Threading.Tasks;

namespace WPNest.Services {

	public interface IStatusUpdaterService {
		Thermostat CurrentThermostat { get; set; }
		void Start();
		void Stop();
		Task UpdateStatusAsync();
	}
}
