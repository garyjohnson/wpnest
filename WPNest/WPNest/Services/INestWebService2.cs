using System;

namespace WPNest.Services {
	public interface INestWebService2 {
		void BeginLogin(string userName, string password, Action<WebServiceResult> completedCallback);
		void BeginGetStatus(Action<GetStatusResult> completedCallback);
		void BeginSetFanMode(Thermostat thermostat, FanMode fanMode, Action<WebServiceResult> completedCallback);
		void BeginChangeTemperature(Thermostat thermostat, double desiredTemperature, Action<WebServiceResult> completedCallback);
		void BeginSendPutRequest(string url, string requestJson, Action<WebServiceResult> completedCallback);
		void BeginGetThermostatStatus(Thermostat thermostat, Action<GetThermostatStatusResult> completedCallback);
		void BeginUpdateTransportUrl(Action<WebServiceResult> onCompleted);
	}
}