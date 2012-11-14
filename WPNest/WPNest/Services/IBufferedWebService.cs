using System.Threading.Tasks;

namespace WPNest.Services {

	public interface IBufferedWebService {
		Task<GetThermostatStatusResult> GetThermostatStatusAsync(Thermostat thermostat);
	}
}
