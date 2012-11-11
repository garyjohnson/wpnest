using System.Collections.Generic;

namespace WPNest.Services {

	interface ISettingsProvider : IDictionary<string, object> {

		void Save();
	}
}
