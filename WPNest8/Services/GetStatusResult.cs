using System;
using System.Collections.Generic;

namespace WPNest.Services {

	public class GetStatusResult : WebServiceResult {

		public GetStatusResult(IEnumerable<Structure> structures) {
			Structures = structures;
		}

		public GetStatusResult(Exception error) {
			Error = error;
		}

		public IEnumerable<Structure> Structures { get; set; }
	}
}
