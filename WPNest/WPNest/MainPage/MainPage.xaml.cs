using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WPNest {

	public partial class MainPage : PhoneApplicationPage {

		private bool isSettingsOpen;

		public MainPage() {
			InitializeComponent();

			SetValue(SystemTray.IsVisibleProperty, true);
			SetBinding(IsLoggedInProperty, new Binding("IsLoggedIn"));
			SetBinding(IsLoggingInProperty, new Binding("IsLoggingIn"));

			ResetZoom.Completed += OnResetZoomCompleted;
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
			GoToDefaultVisualState(false);
		}

		public static readonly DependencyProperty IsLoggedInProperty =
			DependencyProperty.Register("IsLoggedIn", typeof(bool), typeof(MainPage), new PropertyMetadata(false, OnIsLoggedInChanged));

		public bool IsLoggedIn {
			get { return (bool)GetValue(IsLoggedInProperty); }
			set { SetValue(IsLoggedInProperty, value); }
		}

		public static readonly DependencyProperty IsLoggingInProperty =
			DependencyProperty.Register("IsLoggingIn", typeof(bool), typeof(MainPage), new PropertyMetadata(false, OnIsLoggingInChanged));

		public bool IsLoggingIn {
			get { return (bool)GetValue(IsLoggingInProperty); }
			set { SetValue(IsLoggingInProperty, value); }
		}

		private NestViewModel ViewModel {
			get { return DataContext as NestViewModel; }
		}

		private void OnUnloaded(object sender, RoutedEventArgs args) {
			ViewModel.Teardown();
		}

		private void OnResetZoomCompleted(object sender, EventArgs eventArgs) {
			ZoomIn.Begin();
		}

		private async void OnLoaded(object sender, RoutedEventArgs e) {
			await ViewModel.InitializeAsync();
		}

		private void OnLoginPressed(object sender, EventArgs e) {
			GoToDefaultVisualState();
		}

		private static void OnIsLoggedInChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var mainPage = (MainPage)sender;
			var isLoggedIn = (bool)args.NewValue;

			if (isLoggedIn) {
				mainPage.GoToLoggedInVisualState();
				mainPage.ResetZoom.Begin();
			}
			else {
				mainPage.GoToDefaultVisualState();
			}
		}

		private static void OnIsLoggingInChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var mainPage = (MainPage)sender;
			var isLoggingIn = (bool)args.NewValue;

			if (isLoggingIn)
				mainPage.GoToPromptingLoginVisualState();
			else
				mainPage.GoToDefaultVisualState();
		}

		private void GoToPromptingLoginVisualState() {
			VisualStateManager.GoToState(this, "PromptingLogIn", true);
		}

		private void GoToLoggedInVisualState() {
			VisualStateManager.GoToState(this, "LoggedIn", true);
		}

		private void GoToDefaultVisualState(bool useTransition = true) {
			VisualStateManager.GoToState(this, "Default", useTransition);
		}

		protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e) {
			base.OnBackKeyPress(e);
			if (isSettingsOpen) {
				e.Cancel = true;
				CloseSettingsPanel();
			}
		}

		private void OnSettingsButtonPress(object sender, RoutedEventArgs e) {
			OpenSettingsPanel();
		}

		private void OnClickedOutsideOfSettings(object sender, MouseButtonEventArgs e) {
			CloseSettingsPanel();
		}

		private void OpenSettingsPanel() {
			isSettingsOpen = true;
			MoveThermostatToBackground.Begin();
			VisualStateManager.GoToState(this, "SettingsOpen", true);
		}

		private void CloseSettingsPanel() {
			isSettingsOpen = false;
			MoveThermostatToForeground.Begin();
			VisualStateManager.GoToState(this, "SettingsClosed", true);
		}
	}
}