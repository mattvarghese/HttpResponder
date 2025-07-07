namespace HttpLogger.Server.Model
{
	public static class Utils
	{
		public const string ConfigKey = "vmatt-httplogger-config";

		public static string DataFolder(string hostName)
		{
			string folderPath;
			/* This is aanother way to do this if you want to do this at compile time
			 * 
				#if WINDOWS
					string folderPath = "C:\\content\\web-apps\\HttpLogger\\data";
				#else
					string folderPath = "./data";
				#endif
			 */
			if (OperatingSystem.IsWindows())
			{
				folderPath = "C:\\content\\web-apps\\HttpLogger\\data";
			}
			else
			{
				folderPath = "./data";
			}

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
