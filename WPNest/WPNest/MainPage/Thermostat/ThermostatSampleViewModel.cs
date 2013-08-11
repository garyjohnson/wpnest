using System.ComponentModel;
using System.Runtime.CompilerServices;
using WPNest.Annotations;

namespace WPNest {

	public class ThermostatSampleViewModel : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
