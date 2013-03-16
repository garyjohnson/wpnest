using System;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WPNest.Error;
using WPNest.Services;

namespace WPNest {

	public partial class App : Application {

		private bool _isInitialized;
		private readonly IAnalyticsService _analyticsService = new AnalyticsService();
		public PhoneApplicationFrame RootFrame { get; private set; }

		public App() {
			UnhandledException += OnUnhandledException;

			if (Environment.OSVersion.Version.Major > 7) {
				WebRequest.RegisterPrefix("http://", SharpGIS.WebRequestCreator.GZip);
				WebRequest.RegisterPrefix("https://", SharpGIS.WebRequestCreator.GZip);
			}

			InitializeComponent();
			InitializeServices();
			InitializePhoneApplication();

			if (System.Diagnostics.Debugger.IsAttached) {
				Current.Host.Settings.EnableFrameRateCounter = true;
				PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
			}
		}

		private void InitializeServices() {
			ServiceContainer.RegisterService<ITimestampProvider>(new TimestampProvider());
			ServiceContainer.RegisterService<ITimer, TimerWrapper>();
			ServiceContainer.RegisterService<INestWebServiceDeserializer>(new NestWebServiceDeserializer());
			ServiceContainer.RegisterService<IWebRequestProvider>(new WebRequestProvider());
			ServiceContainer.RegisterService<IAnalyticsService>(_analyticsService);
			ServiceContainer.RegisterService<ISettingsProvider>(new SettingsProvider());
			ServiceContainer.RegisterService<ISessionProvider>(new SessionProvider());
			ServiceContainer.RegisterService<INestWebService>(new NestWebService());
			ServiceContainer.RegisterService<IStatusProvider>(new DelayedStatusProvider());
			ServiceContainer.RegisterService<IStatusUpdaterService>(new StatusUpdaterService());
			ServiceContainer.RegisterService<IDialogProvider>(new DialogProvider());
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
			_analyticsService.LogEvent("Navigation Failed");
			BreakIfDebuggerAttached();
		}

		private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e) {
			if (RootVisual != RootFrame)
				RootVisual = RootFrame;

			RootFrame.Navigated -= CompleteInitializePhoneApplication;
		}

		private void OnApplicationLaunching(object sender, LaunchingEventArgs e) {
			_analyticsService.StartSession();
		}

		private void OnApplicationActivated(object sender, ActivatedEventArgs e) {
			_analyticsService.StartSession();
		}

		private void OnApplicationDeactivated(object sender, DeactivatedEventArgs e) {
			_analyticsService.EndSession();
		}

		private void OnApplicationClosing(object sender, ClosingEventArgs e) {
			_analyticsService.EndSession();
		}

		private void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e) {
			if (e.ExceptionObject is CloseApplicationException)
				return;

			_analyticsService.LogError(e.ExceptionObject);
			BreakIfDebuggerAttached();
		}

		public static void Close() {
			throw new CloseApplicationException();
		}

		private static void BreakIfDebuggerAttached() {
			if (System.Diagnostics.Debugger.IsAttached)
				System.Diagnostics.Debugger.Break();
		}
	}
}