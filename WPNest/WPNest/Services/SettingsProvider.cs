using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;

namespace WPNest.Services {

	internal class SettingsProvider : ISettingsProvider {

		private IDictionary<string, object> SettingsDictionary {
			get { return IsolatedStorageSettings.ApplicationSettings; }
		}

		private IsolatedStorageSettings Settings {
			get { return IsolatedStorageSettings.ApplicationSettings; }
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
			return SettingsDictionary.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public void Add(KeyValuePair<string, object> item) {
			SettingsDictionary.Add(item);
		}

		public void Clear() {
			SettingsDictionary.Clear();
		}

		public bool Contains(KeyValuePair<string, object> item) {
			return SettingsDictionary.Contains(item);
		}

		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
			SettingsDictionary.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<string, object> item) {
			return SettingsDictionary.Remove(item);
		}

		public int Count {
			get { return SettingsDictionary.Count; }
		}

		public bool IsReadOnly { get { return SettingsDictionary.IsReadOnly; } }
		public bool ContainsKey(string key) {
			return SettingsDictionary.ContainsKey(key);
		}

		public void Add(string key, object value) {
			SettingsDictionary.Add(key, value);
		}

		public bool Remove(string key) {
			return SettingsDictionary.Remove(key);
		}

		public bool TryGetValue(string key, out object value) {
			return SettingsDictionary.TryGetValue(key, out value);
		}

		public object this[string key] {
			get { return SettingsDictionary[key]; }
			set { SettingsDictionary[key] = value; }
		}

		public ICollection<string> Keys { get { return SettingsDictionary.Keys; } }
		public ICollection<object> Values { get { return SettingsDictionary.Values; } }

		public void Save() {
			Settings.Save();
		}
	}
}
