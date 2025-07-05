namespace HttpLogger.Server.Model
{
	public static class Utils
	{
		public const string ConfigKey = "vmatt-httplogger-config";

		public static string DataFolder(string hostName)
		{
			string folderPath = "C:\\content\\web-apps\\HttpLogger\\data";
			EnsureDirectoryExists(folderPath);
			return folderPath;
		}

		private static void EnsureDirectoryExists(string folderPath)
		{
			// Check if the directory exists
			if (!Directory.Exists(folderPath))
			{
				// Create the directory (and all necessary parent directories)
				Directory.CreateDirectory(folderPath);
			}
		}

		public static string StatsFile(string hostName)
		{
			var folderPath = DataFolder(hostName);
			return Path.Combine(folderPath, "statistics.json");
		}
	}
}
