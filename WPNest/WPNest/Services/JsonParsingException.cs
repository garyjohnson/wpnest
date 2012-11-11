using System;

namespace WPNest.Services {

	public class JsonParsingException : Exception {
		
		public JsonParsingException(Exception innerException) : base("", innerException) {
		}
	}
}
