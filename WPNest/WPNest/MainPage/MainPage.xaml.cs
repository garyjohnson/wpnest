using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WPNest {

	public partial class MainPage : PhoneApplicationPage {

		private bool isSettingsOpen;

		public MainPage() {
			InitializeComponent();
			ShowCorrectProgressBarDependingOnOSVersion();

			SetValue(SystemTray.IsVisibleProperty, true);
			SetBinding(IsLoggedInProperty, new Binding("IsLoggedIn"));
			SetBinding(IsLoggingInProperty, new Binding("IsLoggingIn"));
			SetBinding(IsInErrorStateProperty, new Binding("IsInErrorState"));
			SetBinding(HvacModeProperty, new Binding("HvacMode") { Mode = BindingMode.TwoWay });

			ResetZoom.Completed += OnResetZoomCompleted;
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
			GoToDefaultVisualState(false);
		}

		public static readonly DependencyProperty IsInErrorStateProperty =
			DependencyProperty.Register("IsInErrorState", typeof(bool), typeof(MainPage), new PropertyMetadata(false, OnIsInErrorStateChanged));

		public bool IsInErrorState {
			get { return (bool)GetValue(IsInErrorStateProperty); }
			set { SetValue(IsInErrorStateProperty, value); }
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

		public static readonly DependencyProperty HvacModeProperty =
			DependencyProperty.Register("HvacMode", typeof(HvacMode), typeof(MainPage), new PropertyMetadata(HvacMode.Off, OnHvacModeChanged));

		public HvacMode HvacMode {
			get { return (HvacMode)GetValue(HvacModeProperty); }
			set { SetValue(HvacModeProperty, value); }
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

		private static void OnIsInErrorStateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var mainPage = (MainPage)sender;
			var isInErrorState = (bool)args.NewValue;

			mainPage.RefreshVisualState(mainPage.IsLoggedIn, mainPage.IsLoggingIn, isInErrorState);
		}

		private void RefreshVisualState(bool isLoggedIn, bool isLoggingIn, bool isInErrorState) {
			if (isInErrorState) {
				GoToErrorVisualState();
			}
			else if (isLoggedIn) {
				GoToLoggedInVisualState();
				ResetZoom.Begin();
			}
			else if (isLoggingIn) {
				GoToPromptingLoginVisualState();
			}
			else {
				GoToDefaultVisualState();
			}
		}

		private static void OnIsLoggedInChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var mainPage = (MainPage)sender;
			var isLoggedIn = (bool)args.NewValue;

			mainPage.RefreshVisualState(isLoggedIn, mainPage.IsLoggingIn, mainPage.IsInErrorState);
		}

		private static void OnIsLoggingInChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var mainPage = (MainPage)sender;
			var isLoggingIn = (bool)args.NewValue;

			mainPage.RefreshVisualState(mainPage.IsLoggedIn, isLoggingIn, mainPage.IsInErrorState);
		}

		private static void OnHvacModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var mainPage = (MainPage)sender;
			var newValue = (HvacMode)args.NewValue;
			mainPage.SelectHvacControl(newValue);
		}

		private void GoToErrorVisualState() {
			VisualStateManager.GoToState(this, "Error", true);
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

		private void OnSettingsButtonPress(object sender, RoutedEventArgs args) {
			OpenSettingsPanel();
		}

		private void OnClickedOutsideOfSettings(object sender, MouseButtonEventArgs args) {
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

		private void OnHvacSelectionChanged(object sender, SelectionChangedEventArgs args) {
			HvacMode? hvacMode = GetSelectedHvacMode();
			if (hvacMode.HasValue)
				HvacMode = hvacMode.Value;
		}

		private HvacMode? GetSelectedHvacMode() {
			if (hvacPicker != null)
				return ((HvacModeControl)hvacPicker.SelectedItem).HvacMode;

			return null;
		}

		private void SelectHvacControl(HvacMode hvacMode) {
			hvacPicker.SelectedItem = hvacPicker.Items.Cast<HvacModeControl>().First(h => h.HvacMode == hvacMode);
		}

		private void OnTapToRetryAfterError(object sender, System.Windows.Input.GestureEventArgs e) {
			ViewModel.RetryAfterErrorAsync();
		}

		private void ShowCorrectProgressBarDependingOnOSVersion() {
			if (Environment.OSVersion.Version.Major > 7) {
				wp7ProgressBar.Visibility = Visibility.Collapsed;
				wp8ProgressBar.Visibility = Visibility.Visible;
			}
		}
	}
}