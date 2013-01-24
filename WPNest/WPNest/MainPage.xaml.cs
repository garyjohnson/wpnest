using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Testing;

namespace WPNest {

	public partial class MainPage : PhoneApplicationPage {

		private bool isSettingsOpen;

		public MainPage() {
			InitializeComponent();

			if (App.IsTest) {
				Content = UnitTestSystem.CreateTestPage();
				return;
			}

			SetValue(SystemTray.IsVisibleProperty, true);
			SetBinding(IsLoggedInProperty, new Binding("IsLoggedIn"));
			SetBinding(IsLoggingInProperty, new Binding("IsLoggingIn"));
//			SetBinding(SelectedHvacModeProperty, new Binding("SelectedHvacMode"));

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

//		public static readonly DependencyProperty SelectedHvacModeProperty =
//			DependencyProperty.Register("SelectedHvacMode", typeof(HvacMode), typeof(MainPage), new PropertyMetadata(HvacMode.Off, OnSelectedHvacModeChanged));
//
//		public HvacMode SelectedHvacMode {
//			get { return (HvacMode)GetValue(SelectedHvacModeProperty); }
//			set { SetValue(SelectedHvacModeProperty, value); }
//		}

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

//		private void OnHvacModeSelectionChanged(object sender, SelectionChangedEventArgs e) {
//			if (e.AddedItems.Count == 0)
//				return;
//
//			var hvacModeControl = (HvacModeControl)e.AddedItems[0];
//			ViewModel.SelectedHvacMode = hvacModeControl.HvacMode;
//		}

//		private static void OnSelectedHvacModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
//			var self = (MainPage)sender;
//			var hvacMode = (HvacMode)args.NewValue;
//			self.SelectHvacMode(hvacMode);
//		}
//
//		private void SelectHvacMode(HvacMode hvacMode) {
//			foreach (HvacModeControl hvacControl in hvacModePicker.Items)
//				if (hvacControl.HvacMode == hvacMode)
//					hvacModePicker.SelectedItem = hvacControl;
//		}
	}
}