using System;

namespace WPNest.Services {

	internal class TimestampProvider : ITimestampProvider {

		public double GetTimestamp() {
			var unixTime = DateTime.Now.ToUniversalTime() -
				new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			return unixTime.TotalSeconds;
		}
	}
}
