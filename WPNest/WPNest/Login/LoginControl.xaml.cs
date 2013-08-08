using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPNest.Login {

	public partial class LoginControl : UserControl {

		public LoginControl() {
			InitializeComponent();
		}

		private NestViewModel ViewModel {
			get { return DataContext as NestViewModel; }
		}

		private async void OnLoginPressed(object sender, RoutedEventArgs args) {
			if (LoginPressed != null)
				LoginPressed(this, EventArgs.Empty);

			await ViewModel.LoginAsync();
			RefreshPasswordHintVisibility();
		}

		private void OnPasswordChangedFocus(object sender, RoutedEventArgs args) {
			RefreshPasswordHintVisibility();
		}

		private void RefreshPasswordHintVisibility() {
			if (string.IsNullOrEmpty(password.Password) && FocusManager.GetFocusedElement() != password)
				passwordHint.Visibility = Visibility.Visible;
			else
				passwordHint.Visibility = Visibility.Collapsed;
		}

		private void OnUserNameKeyUp(object sender, KeyEventArgs args) {
			if (args.Key == Key.Enter)
				password.Focus();
		}

		private void OnPasswordKeyUp(object sender, KeyEventArgs args) {
			if (args.Key == Key.Enter) {
				ChangeFocusSoThatKeyboardGoesAway();
				ManuallyUpdatePasswordBinding();
				OnLoginPressed(sender, null);
			}
		}

		private void ChangeFocusSoThatKeyboardGoesAway() {
			loginButton.Focus();
		}

		private void ManuallyUpdatePasswordBinding() {
			password.GetBindingExpression(PasswordBox.PasswordProperty).UpdateSource();
		}

		public event EventHandler LoginPressed;
	}
}
