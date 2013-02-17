using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WPNest.Web {

	public interface IWebResponse {
		HttpStatusCode StatusCode { get; }
		Stream GetResponseStream();
	}
}
