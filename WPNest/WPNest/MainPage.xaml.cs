using System;
using System.Windows;
using System.Windows.Data;
using Microsoft.Phone.Controls;

namespace WPNest {

	public partial class MainPage : PhoneApplicationPage {

		public MainPage() {
			InitializeComponent();
			SetBinding(IsLoggedInProperty, new Binding("IsLoggedIn"));
			SetBinding(IsLoggingInProperty, new Binding("IsLoggingIn"));

			Loaded += OnLoaded;
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

		private async void OnLoaded(object sender, RoutedEventArgs e) {
			await ViewModel.InitializeAsync();
		}

		private void OnLoginPressed(object sender, EventArgs e) {
			VisualStateManager.GoToState(this, "Default", true);
		}

		private static void OnIsLoggedInChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			MainPage mainPage = (MainPage) sender;
			bool isLoggedIn = (bool) args.NewValue;

			if (isLoggedIn)
				VisualStateManager.GoToState(mainPage, "LoggedIn", true);
			else
				VisualStateManager.GoToState(mainPage, "Default", true);
		}

		private static void OnIsLoggingInChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			MainPage mainPage = (MainPage) sender;
			bool isLoggingIn = (bool) args.NewValue;

			if (isLoggingIn)
				VisualStateManager.GoToState(mainPage, "PromptingLogIn", true);
			else
				VisualStateManager.GoToState(mainPage, "Default", true);
		}

	}
}