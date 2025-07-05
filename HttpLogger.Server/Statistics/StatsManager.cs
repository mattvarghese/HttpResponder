using System.Text.Json;

namespace HttpLogger.Server.Statistics
{
	public class StatsManager(JsonSerializerOptions jsonOptions)
	{
		private readonly JsonSerializerOptions _jsonOptions = jsonOptions;

		public void UpdateStatsFile(string filePath, StatType type)
		{
			// Open or create the file with exclusive access
			using (FileStream fileStream = TryOpenFileExclusively(filePath))
			{
				StatsModel stats;

				using (var reader = new StreamReader(fileStream, leaveOpen: true))
				{
					fileStream.Seek(0, SeekOrigin.Begin);
					var content = reader.ReadToEnd();
					stats = string.IsNullOrWhiteSpace(content)
						? new StatsModel()
						: JsonSerializer.Deserialize<StatsModel>(content, _jsonOptions) ?? new StatsModel();
				}

				stats.RegisterStatistic(type);

				// Truncate and write updated content
				fileStream.SetLength(0); // clear file
				fileStream.Seek(0, SeekOrigin.Begin);
				using var writer = new StreamWriter(fileStream);
				writer.Write(JsonSerializer.Serialize(stats, _jsonOptions));
			}
		}

		private FileStream TryOpenFileExclusively(string path)
		{
			const int RETRIES = 20;
			const int DELAY = 50; //milliseconds

			for (int i = 0; i < RETRIES; i++)
			{
				try
				{
					return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
				}
				catch (IOException)
				{
					if (i == RETRIES - 1)
					{
						throw new IOException("Unable to obtain exclusive access to " + path);
					}

					Thread.Sleep(DELAY);
				}
			}

			throw new IOException("Unable to obtain exclusive access to " + path);
		}

		public StatsModel LoadStatsFile(string filePath)
		{
			if (!File.Exists(filePath))
			{
				return new StatsModel();
			}

			string json = File.ReadAllText(filePath);
			if (string.IsNullOrWhiteSpace(json))
			{
				return new StatsModel();
			}

			return JsonSerializer.Deserialize<StatsModel>(json, _jsonOptions) ?? new StatsModel();
		}
	}
}
