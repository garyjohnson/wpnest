using System;
using System.Collections.Generic;

namespace WPNest {

	public class NanoContainer {

		private readonly Dictionary<Type, object> registeredDependencies = new Dictionary<Type, object>();
		private readonly Dictionary<Type, Type> registeredTypes = new Dictionary<Type, Type>();

		public T GetDependency<T>() where T : class {
			if (registeredDependencies.ContainsKey(typeof(T)))
				return (T)registeredDependencies[typeof(T)];

			if (registeredTypes.ContainsKey(typeof(T))) {
				Type type = registeredTypes[typeof(T)];
				return (T)Activator.CreateInstance(type);
			}

			System.Diagnostics.Debug.WriteLine("Unknown dependency requested: {0}", typeof(T).Name);
			return null;
		}

		public void RegisterDependency<T>(T value) where T : class {
			if (registeredDependencies.ContainsKey(typeof(T))) {
				registeredDependencies[typeof(T)] = value;
			}
			else {
				registeredDependencies.Add(typeof(T), value);
			}
		}

		public void RegisterDependency<T, U>()
			where T : class
			where U : class {
			if (registeredTypes.ContainsKey(typeof(T))) {
				registeredTypes[typeof(T)] = typeof(U);
			}
			else {
				registeredTypes.Add(typeof(T), typeof(U));
			}
		}
	}
}
