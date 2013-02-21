using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WPNest.Web {
	public interface IWebRequest {
		string Method { get; set; }
		string ContentType { get; set; }
		WebHeaderCollection Headers { get; set; }
		Task<Stream> GetRequestStreamAsync();
		Task<IWebResponse> GetResponseAsync();
		Task SetRequestStringAsync(string requestString);
	}
}
