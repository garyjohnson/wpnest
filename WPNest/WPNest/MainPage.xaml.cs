using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Testing;
using Microsoft.Phone.Testing.Harness;

namespace WPNest {

	public partial class MainPage : PhoneApplicationPage {

		public MainPage() {
			InitializeComponent();

			if (App.IsTest)
				Content = UnitTestSystem.CreateTestPage();
		}

		private MainPageViewModel ViewModel {
			get { return DataContext as MainPageViewModel; }
		}

		private async void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
			await ViewModel.LoginAsync();
		}

		private async void OnUpClick(object sender, RoutedEventArgs e) {
			await ViewModel.RaiseTemperatureAsync();
		}

		private async void OnDownClick(object sender, RoutedEventArgs e) {
			await ViewModel.LowerTemperatureAsync();
		}

	}
}