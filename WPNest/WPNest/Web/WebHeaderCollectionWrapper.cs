using System;
using System.Net;

namespace WPNest.Web {

	internal class WebHeaderCollectionWrapper : IWebHeaderCollection {

		private readonly WebHeaderCollection _headers;

		public WebHeaderCollectionWrapper(WebHeaderCollection headers) {
			if(headers == null) throw new ArgumentNullException("headers");
			_headers = headers;
		}

		public string[] AllKeys {
			get { return _headers.AllKeys; }
		}
		public int Count {
			get { return _headers.Count; }
		}

		string IWebHeaderCollection.this[HttpRequestHeader header] {
			get { return _headers[header]; }
			set { _headers[header] = value; }
		}

		string IWebHeaderCollection.this[string header] {
			get { return _headers[header]; }
			set { _headers[header] = value; }
		}
	}
}
