using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Testing;
using WPNest.Services;

namespace WPNest {

	public partial class App : Application {

#if TEST
		public const bool IsTest = true;
#else
		public const bool IsTest = false;
#endif

		private bool _isInitialized;
		public PhoneApplicationFrame RootFrame { get; private set; }

		public App() {
			UnhandledException += OnUnhandledException;
			InitializeComponent();
			InitializeServices();
			InitializePhoneApplication();

			if (System.Diagnostics.Debugger.IsAttached) {
				Current.Host.Settings.EnableFrameRateCounter = true;
				PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
			}
		}

		private void InitializeServices() {
			InitializeServicesCommonToTestAndRuntime();
			if (IsTest)
				InitializeServicesForTest();
			else
				InitializeServicesForRuntime();
		}

		private void InitializeServicesCommonToTestAndRuntime() {
		}

		private void InitializeServicesForRuntime() {
			ServiceContainer.RegisterService<ISettingsProvider>(new SettingsProvider());
			ServiceContainer.RegisterService<ISessionProvider>(new SessionProvider());
			ServiceContainer.RegisterService<INestWebService>(new NestWebService());
		}

		private void InitializeServicesForTest() {
		}

		private void InitializePhoneApplication() {
			if (_isInitialized)
				return;

			RootFrame = new PhoneApplicationFrame();
			RootFrame.Navigated += CompleteInitializePhoneApplication;
			RootFrame.NavigationFailed += OnNavigationFailed;
			_isInitialized = true;

		}

		private void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
			BreakIfDebuggerAttached();
		}

		private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e) {
			if (RootVisual != RootFrame)
				RootVisual = RootFrame;

			RootFrame.Navigated -= CompleteInitializePhoneApplication;
		}

		private void OnApplicationLaunching(object sender, LaunchingEventArgs e) {
		}

		private void OnApplicationActivated(object sender, ActivatedEventArgs e) {
		}

		private void OnApplicationDeactivated(object sender, DeactivatedEventArgs e) {
		}

		private void OnApplicationClosing(object sender, ClosingEventArgs e) {
		}

		private void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e) {
			BreakIfDebuggerAttached();
		}

		private static void BreakIfDebuggerAttached() {
			if (System.Diagnostics.Debugger.IsAttached)
				System.Diagnostics.Debugger.Break();
		}
	}
}