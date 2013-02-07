using System.Windows;

namespace WPNest.Services {

	public class DialogProvider : IDialogProvider {

		public void ShowMessageBox(string message) {
			MessageBox.Show(message);
		}
	}
}
