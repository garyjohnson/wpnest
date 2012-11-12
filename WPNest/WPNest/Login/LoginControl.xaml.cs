using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPNest.Login {

	public partial class LoginControl : UserControl {

		public LoginControl() {
			InitializeComponent();
		}

		private MainPageViewModel ViewModel {
			get { return DataContext as MainPageViewModel; }
		}

		private async void OnLoginPressed(object sender, RoutedEventArgs e) {
			await ViewModel.LoginAsync();
		}

		private void OnPasswordChangedFocus(object sender, RoutedEventArgs e) {
			RefreshPasswordHintVisibility();
		}

		private void RefreshPasswordHintVisibility() {
			if (string.IsNullOrEmpty(password.Password) && FocusManager.GetFocusedElement() != password)
				passwordHint.Visibility = Visibility.Visible;
			else
				passwordHint.Visibility = Visibility.Collapsed;
		}
	}
}
