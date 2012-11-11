using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace WPNest {

	public partial class MainPage : PhoneApplicationPage {

		public MainPage() {
			InitializeComponent();
			Loaded += OnLoaded;
		}

		private MainPageViewModel ViewModel {
			get { return DataContext as MainPageViewModel; }
		}

		private async void OnLoaded(object sender, RoutedEventArgs e) {
			await ViewModel.InitializeAsync();
		}


	}
}