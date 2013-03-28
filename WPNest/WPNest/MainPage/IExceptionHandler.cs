using System;
using WPNest.Services;

namespace WPNest {

	internal interface IExceptionHandler {
		bool IsErrorHandled(WebServiceError error, Exception exception);
	}
}
