using System.Collections.Generic;

namespace WPNest.Services {

	public interface ISettingsProvider : IDictionary<string, object> {

		void Save();
	}
}
