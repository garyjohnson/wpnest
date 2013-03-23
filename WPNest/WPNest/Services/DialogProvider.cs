using System.Windows;

namespace WPNest.Services {

	internal class DialogProvider : IDialogProvider {

		public void ShowMessageBox(string message) {
			MessageBox.Show(message);
		}
	}
}
