namespace HttpLogger.Server.Statistics
{
	public enum StatType
	{
		ApiInvoked,
		LogsViewed,
		ConfigListFetched,
		ConfigRead,
		ConfigSaved,
		ConfigDeleted,
		StatsViewed,
	}

	public class StatsModel
	{
		public DateTime CreationTimeStamp { get; set; } = DateTime.Now;
		public DateTime UpdateTimeStamp { get; set; } = DateTime.Now;

		public Dictionary<string, ulong> Counts { get; set; } = new();

		public void RegisterStatistic(StatType type)
		{
			UpdateTimeStamp = DateTime.Now;
			string key = type.ToString();

			if (Counts.ContainsKey(key))
			{
				Counts[key]++;
			}
			else
			{
				Counts[key] = 1;
			}
		}
	}
}
