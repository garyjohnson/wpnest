using System.Net;

namespace WPNest.Web {

	public interface IWebHeaderCollection {
		string[] AllKeys { get; }
		int Count { get; }
		string this[HttpRequestHeader header] { get; set; }
		string this[string header] { get; set; }
	}
}
