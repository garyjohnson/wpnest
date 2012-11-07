using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using Microsoft.Phone.Controls;

namespace WPNest {

	public partial class MainPage : PhoneApplicationPage {

		public MainPage() {
			InitializeComponent();
		}

		private MainPageViewModel ViewModel {
			get { return DataContext as MainPageViewModel; }
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
			ViewModel.Login();
		}

		private void OnUpClick(object sender, RoutedEventArgs e) {
			ViewModel.Up();
		}

		private void OnDownClick(object sender, RoutedEventArgs e) {
			ViewModel.Down();
		}

	}
}