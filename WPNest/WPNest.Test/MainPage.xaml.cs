using Microsoft.Phone.Controls;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.Core;
using vstest_executionengine_platformbridge;
using Microsoft.VisualStudio.TestPlatform.TestExecutor;

namespace WPNest.Test {

	public partial class MainPage : PhoneApplicationPage {

		public MainPage() {
			InitializeComponent();
			var wrapper = new TestExecutorServiceWrapper();
			new Thread(new ServiceMain((param0, param1) => wrapper.SendMessage((ContractName)param0, param1)).Run).Start();
		}
	}
}