using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WPNest.Services;
using WPNest.Annotations;

namespace WPNest {

	public class NestViewModel : IExceptionHandler, INotifyPropertyChanged {

		private readonly IStatusProvider _statusProvider;
		private readonly ISessionProvider _sessionProvider;
		private readonly INestWebService _nestWebService;
		private readonly IStatusUpdaterService _statusUpdater;
		private readonly IAnalyticsService _analyticsService;
		private readonly IDialogProvider _dialogProvider;
		private GetStatusResult _getStatusResult;

		private string _userName = "";
		public string UserName {
			get { return _userName; }
			set {
				_userName = value;
				OnPropertyChanged();
			}
		}

		private string _password = "";
		public string Password {
			get { return _password; }
			set {
				_password = value;
				OnPropertyChanged();
			}
		}

		private bool _isLoggingIn;
		public bool IsLoggingIn {
			get { return _isLoggingIn; }
			set {
				_isLoggingIn = value;
				OnPropertyChanged();
			}
		}

		private bool _isLoggedIn;
		public bool IsLoggedIn {
			get { return _isLoggedIn; }
			set {
				_isLoggedIn = value;
				OnPropertyChanged();
			}
		}

		private ThermostatViewModel _selectedThermostat;
		public ThermostatViewModel SelectedThermostat {
			get { return _selectedThermostat; }
			set {
				_selectedThermostat = value;
				OnPropertyChanged();
			}
		}

		private WebServiceError _currentError = WebServiceError.None;
		public WebServiceError CurrentError {
			get { return _currentError; }
			set {
				_currentError = value;
				OnPropertyChanged();
			}
		}

		private readonly ObservableCollection<ThermostatViewModel> _thermostats = new ObservableCollection<ThermostatViewModel>();
		public ObservableCollection<ThermostatViewModel> Thermostats {
			get { return _thermostats; }
		}

		public NestViewModel() {
			if (DesignerProperties.IsInDesignTool)
				return;

			ServiceContainer.RegisterService<IExceptionHandler>(this);
			_statusProvider = ServiceContainer.GetService<IStatusProvider>();
			_sessionProvider = ServiceContainer.GetService<ISessionProvider>();
			_nestWebService = ServiceContainer.GetService<INestWebService>();
			_statusUpdater = ServiceContainer.GetService<IStatusUpdaterService>();
			_analyticsService = ServiceContainer.GetService<IAnalyticsService>();
			_dialogProvider = ServiceContainer.GetService<IDialogProvider>();
			_statusProvider.StatusUpdated += OnStatusUpdated;
		}

		private void OnStatusUpdated(object sender, StatusEventArgs e) {
			if (IsErrorHandled(e.Status.Error, e.Status.Exception))
				return;

			UpdateViewModelFromGetStatusResult(e.Status);
		}

		private void UpdateViewModelFromGetStatusResult(GetStatusResult statusResult) {
			_getStatusResult = statusResult;
		}

		public async Task InitializeAsync() {
			if (_sessionProvider.IsSessionExpired) {
				IsLoggingIn = true;
				return;
			}

			await OnLoggedIn();
		}

		public async Task LoginAsync() {
			ResetCurrentError();
			string userName = UserName;
			string password = Password;
			ClearLoginFields();

			if (_sessionProvider.IsSessionExpired) {
				var loginResult = await _nestWebService.LoginAsync(userName, password);
				if (IsErrorHandled(loginResult.Error, loginResult.Exception))
					return;
			}

			await OnLoggedIn();
		}

		private void ResetCurrentError() {
			CurrentError = WebServiceError.None;
		}

		private void ClearLoginFields() {
			Password = string.Empty;
		}

		private async Task OnLoggedIn() {
			IsLoggingIn = false;

			var result = await _nestWebService.UpdateTransportUrlAsync();
			if (IsErrorHandled(result.Error, result.Exception))
				return;

			_getStatusResult = await _nestWebService.GetFullStatusAsync();
			if (IsErrorHandled(_getStatusResult.Error, _getStatusResult.Exception))
				return;

			Structure firstStructure = _getStatusResult.Structures.ElementAt(0);
			foreach (Thermostat thermostat in firstStructure.Thermostats)
				Thermostats.Add(new ThermostatViewModel(thermostat));

			IsLoggedIn = true;

			UpdateViewModelFromGetStatusResult(_getStatusResult);

			_statusUpdater.CurrentStructure = firstStructure;
			_statusUpdater.Start();
		}

		public void Teardown() {
			_statusUpdater.Stop();
		}

		public bool IsErrorHandled(WebServiceError error, Exception exception) {
			if (error == WebServiceError.InvalidCredentials ||
				error == WebServiceError.SessionTokenExpired)
				HandleLoginException(error);
			else if (error == WebServiceError.Cancelled)
				HandleException();
			else if (error == WebServiceError.ServerNotFound)
				HandleException("Server was not found. Please check your network connection and press OK to retry.");
			else if (exception != null)
				HandleException("An unknown error occurred. Press OK to retry.");

			if (exception != null)
				_analyticsService.LogError(exception);

			return exception != null;
		}

		private void HandleException(string message = null) {
			IsLoggingIn = false;
			if (message != null)
				_dialogProvider.ShowMessageBox(message);
			OnLoggedIn();
		}

		private void HandleLoginException(WebServiceError error) {
			IsLoggedIn = false;
			// Missing test coverage. Set to false before true so UI updates. 
			// How to test this? Or refactor out so not needed?
			IsLoggingIn = false;
			_sessionProvider.ClearSession();
			CurrentError = error;
			IsLoggingIn = true;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
