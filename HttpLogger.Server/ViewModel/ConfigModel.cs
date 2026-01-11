namespace HttpLogger.Server.ViewModel
{
	public class PathResponseModel
	{
		public int StatusCode { get; set; } = 200;
		public Dictionary<string, string>? ResponseHeaders { get; set; }
		public string? Body { get; set; }
	}

	public class ConfigModel : PathResponseModel
	{
		public string? Guid { get; set; }
		public string? Name { get; set; }

		public int ResponseDelay { get; set; } = 0;

		// Path-specific overrides (in order; first match wins).
		public List<PathSpecificRule>? PathSpecificResponse { get; set; }
	}

	public class PathSpecificRule
	{
		// Pattern is matched against the request path relative to /api/ (no query string).
		// Ex: for URL https://example.com/HttpLogger/api/FHIR/R4/Patient/xyz
		//     only "FHIR/R4/Patient/xyz" constitutes pattern

		// If IsRegularExpression is false, Pattern is treated as a literal string.
		//    Trailing '/' is also removed from both pattern and actual path for comparison.
		// If IsRegularExpression is true, Pattern is treated as a .NET regular expression.
		//    Trailing '/' if present will be retained - regular expression must handle that as desired.

		// The Pattern string
		public string Pattern { get; set; } = "";

		// Default is false so simple literal matches are easy.
		public bool IsRegularExpression { get; set; } = false;

		// When using regex, controls case sensitivity. (Ignored for literal patterns.)
		public bool IgnoreCase { get; set; } = true;

		// What to override if matched
		public PathResponseModel Response { get; set; } = new();
	}
}
