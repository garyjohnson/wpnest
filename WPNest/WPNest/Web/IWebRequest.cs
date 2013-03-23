using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WPNest.Web {

	public interface IWebRequest {
		string Method { get; set; }
		string ContentType { get; set; }
		IWebHeaderCollection Headers { get; }
		Task<Stream> GetRequestStreamAsync();
		Task<IWebResponse> GetResponseAsync();
		Task SetRequestStringAsync(string requestString);
	}
}
