namespace HttpLogger.Server.ViewModel
{
	public class ConfigModel
	{
		public string? Guid { get; set; }
		public string? Name { get; set; }
		public int StatusCode { get; set; }
		public Dictionary<string, string>? ResponseHeaders { get; set; }
		public string? Body { get; set; }
		public int ResponseDelay { get; set; }
	}

}
