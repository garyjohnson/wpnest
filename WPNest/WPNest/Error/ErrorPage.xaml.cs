using System.Windows;
using Microsoft.Phone.Controls;

namespace WPNest {

	public partial class ErrorPage : PhoneApplicationPage {

		public ErrorPage() {
			InitializeComponent();
		}

		protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e) {
			if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
				e.Cancel = true;
		}

		private void closeButton_Click(object sender, RoutedEventArgs e) {
			App.Close();
		}
	}
}
