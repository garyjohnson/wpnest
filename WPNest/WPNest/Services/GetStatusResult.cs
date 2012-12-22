using System;
using System.Collections.Generic;

namespace WPNest.Services {

	public class GetStatusResult : WebServiceResult {

		public GetStatusResult(IEnumerable<Structure> structures) {
			Structures = structures;
		}

		public GetStatusResult(WebServiceError error, Exception exception) {
			Error = error;
			Exception = exception;
		}

		public IEnumerable<Structure> Structures { get; set; }
	}
}
