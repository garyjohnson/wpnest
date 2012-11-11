using System.Windows;
using System.Windows.Controls;

namespace WPNest.Login {

	public partial class LoginControl : UserControl {

		public LoginControl() {
			InitializeComponent();
		}

		private async void OnLoginPressed(object sender, RoutedEventArgs e) {
			await ViewModel.LoginAsync();
		}

		private MainPageViewModel ViewModel {
			get { return DataContext as MainPageViewModel; }
		}
	}
}
