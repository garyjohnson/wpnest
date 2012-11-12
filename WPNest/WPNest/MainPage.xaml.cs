using System.Windows;
using Microsoft.Phone.Controls;

namespace WPNest {

	public partial class MainPage : PhoneApplicationPage {

		public MainPage() {
			InitializeComponent();
			Loaded += OnLoaded;
		}

		private NestViewModel ViewModel {
			get { return DataContext as NestViewModel; }
		}

		private async void OnLoaded(object sender, RoutedEventArgs e) {
			await ViewModel.InitializeAsync();
		}
	}
}