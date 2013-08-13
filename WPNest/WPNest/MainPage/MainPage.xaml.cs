using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace WPNest {

	public partial class MainPage : PhoneApplicationPage {

		private bool isThermostatSettingsOpen;
		private bool isSettingsOpen;

		public MainPage() {
			InitializeComponent();
			ShowCorrectProgressBarDependingOnOSVersion();

			SetValue(SystemTray.IsVisibleProperty, true);
			SetBinding(StateProperty, new Binding("State"));
			SetBinding(HvacModeProperty, new Binding("HvacMode") { Mode = BindingMode.TwoWay });

			ResetZoom.Completed += OnResetZoomCompleted;
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
			GoToDefaultVisualState(false);
		}

		public static readonly DependencyProperty StateProperty =
			DependencyProperty.Register("State", typeof(NestViewModelState), typeof(MainPage), new PropertyMetadata(NestViewModelState.Loading, OnStateChanged));

		public NestViewModelState State {
			get { return (NestViewModelState)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
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

		private void OnLogoutButtonPress(object sender, RoutedEventArgs args) {
			CloseSettingsPanel();
			ViewModel.LogOut();
		}

		private static void OnStateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var mainPage = (MainPage)sender;
			var state = (NestViewModelState)args.NewValue;

			mainPage.RefreshVisualState(state);
		}

		private void RefreshVisualState(NestViewModelState state) {
			if (state == NestViewModelState.Error) {
				GoToErrorVisualState();
			}
			else if (state == NestViewModelState.LoggedIn) {
				GoToLoggedInVisualState();
				ResetZoom.Begin();
			}
			else if (state == NestViewModelState.LoggingIn) {
				GoToPromptingLoginVisualState();
			}
			else {
				GoToDefaultVisualState();
			}
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
			if (isThermostatSettingsOpen) {
				e.Cancel = true;
				CloseThermostatSettingsPanel();
			}
			else if (isSettingsOpen) {
				e.Cancel = true;
				CloseSettingsPanel();
			}
		}

		private void OnThermostatSettingsButtonPressed(object sender, RoutedEventArgs args) {
			OpenThermostatSettingsPanel();
		}

		private void OnClickedOutsideOfSettings(object sender, MouseButtonEventArgs args) {
			CloseSettingsPanel();
		}

		private void CloseSettingsPanel() {
			isSettingsOpen = false;
			VisualStateManager.GoToState(this, "SettingsClosed", true);
		}

		private void OnClickedOutsideOfThermostatSettings(object sender, MouseButtonEventArgs args) {
			CloseThermostatSettingsPanel();
		}

		private void OpenThermostatSettingsPanel() {
			isThermostatSettingsOpen = true;
			MoveThermostatToBackground.Begin();
			VisualStateManager.GoToState(this, "ThermostatSettingsOpen", true);
		}

		private void CloseThermostatSettingsPanel() {
			isThermostatSettingsOpen = false;
			MoveThermostatToForeground.Begin();
			VisualStateManager.GoToState(this, "ThermostatSettingsClosed", true);
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

		private void OnSettingsButtonPressed(object sender, RoutedEventArgs e) {
			OpenSettingsPanel();
		}

		private void OpenSettingsPanel() {
			isSettingsOpen = true;
			VisualStateManager.GoToState(this, "SettingsOpen", true);
		}

		private void OnReviewPressed(object sender, RoutedEventArgs args) {
			new MarketplaceReviewTask().Show();
		}
	}
}