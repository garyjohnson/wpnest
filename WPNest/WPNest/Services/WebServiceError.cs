namespace WPNest.Services {

	public enum WebServiceError {
		None,
		InvalidCredentials,
		SessionTokenExpired,
		ServerNotFound,
		Cancelled,
		Unknown
	}
}
