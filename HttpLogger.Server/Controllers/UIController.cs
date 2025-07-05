using System.Text.Json;
using HttpLogger.Server.Model;
using HttpLogger.Server.Statistics;
using HttpLogger.Server.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace HttpLogger.Server.Controllers
{
	[Route("ui")]
	[ApiController]
	public class UIController(StatsManager statsManager) : ControllerBase
	{
		private readonly StatsManager _statsManager = statsManager;

		// GET /?date=yyyy-MM-dd&offset=0&count=1
		[HttpGet]
		public IActionResult GetLogs(
	[FromQuery] string? date,
	[FromQuery] int offset = 0,
	[FromQuery] int count = 1,
	[FromQuery] string? guid = null)
		{
			try
			{
				// Use today's date if not provided
				var targetDate = string.IsNullOrWhiteSpace(date)
					? DateTime.Now.ToString("yyyy-MM-dd")
					: date;

				var host = HttpContext?.Request?.Host.Host ?? "";
				var folderPath = Path.Combine(Utils.DataFolder(host), targetDate);

				if (!Directory.Exists(folderPath))
					return NotFound($"No log folder found for date {targetDate}");

				// Get .json files sorted by creation time descending
				var files = Directory.GetFiles(folderPath, "*.json")
					.Select(path => new FileInfo(path))
					.OrderByDescending(f => f.CreationTimeUtc)
					.ToList();

				var results = new List<JsonElement>();

				foreach (var file in files)
				{
					var jsonText = System.IO.File.ReadAllText(file.FullName);
					using var doc = JsonDocument.Parse(jsonText);

					// If guid filter is specified, check configGuid
					if (!string.IsNullOrWhiteSpace(guid))
					{
						if (!doc.RootElement.TryGetProperty("ConfigGuid", out var configGuidElement) ||
							!string.Equals(configGuidElement.GetString(), guid, StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}
					}

					results.Add(doc.RootElement.Clone());
				}

				_statsManager.UpdateStatsFile(Utils.StatsFile(host), StatType.LogsViewed);

				// Apply offset and count after filtering
				var sliced = results
					.Skip(offset)
					.Take(count)
					.ToList();

				return Ok(sliced);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error reading logs: {ex.Message}");
			}
		}

		[HttpGet("configs")]
		public IActionResult GetConfigs()
		{
			try
			{
				var host = HttpContext?.Request?.Host.Host ?? "";
				var baseFolder = Utils.DataFolder(host);
				var configFolder = Path.Combine(baseFolder, "config");

				if (!Directory.Exists(configFolder))
				{
					return Ok(new List<object>()); // Return empty array if folder doesn't exist
				}

				var results = new List<object>();

				foreach (var file in Directory.GetFiles(configFolder, "*.json"))
				{
					var guid = Path.GetFileNameWithoutExtension(file);
					var lastUpdate = System.IO.File.GetLastWriteTime(file); // Local timestamp

					string name = "";
					try
					{
						var json = System.IO.File.ReadAllText(file);
						var config = JsonSerializer.Deserialize<ConfigModel>(json);
						name = config?.Name ?? "";
					}
					catch
					{
						// If invalid JSON or error, skip name but still include file
					}

					results.Add(new
					{
						GUID = guid,
						Name = name,
						LastUpdate = lastUpdate
					});
				}

				var sorted = results
					.OrderByDescending(x => ((DateTime)x.GetType().GetProperty("LastUpdate")!.GetValue(x)!))
					.ToList();

				_statsManager.UpdateStatsFile(Utils.StatsFile(host), StatType.ConfigListFetched);

				return Ok(sorted);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error reading configs: {ex.Message}");
			}
		}

		[HttpGet("get-config")]
		public IActionResult GetConfig([FromQuery(Name = "config-guid")] string configGuid)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(configGuid))
					return BadRequest("Missing config-guid parameter");

				var host = HttpContext?.Request?.Host.Host ?? "";
				var baseFolder = Utils.DataFolder(host);
				var configFolder = Path.Combine(baseFolder, "config");
				var configFile = Path.Combine(configFolder, $"{configGuid}.json");

				if (!System.IO.File.Exists(configFile))
					return NotFound("Config file not found");

				var jsonText = System.IO.File.ReadAllText(configFile);
				var config = JsonSerializer.Deserialize<ConfigModel>(jsonText);

				if (config == null)
					return StatusCode(500, "Error parsing config file");

				_statsManager.UpdateStatsFile(Utils.StatsFile(host), StatType.ConfigRead);

				return Ok(config);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error reading config file: {ex.Message}");
			}
		}

		[HttpPut("set-config")]
		public async Task<IActionResult> SetConfig(
			[FromQuery(Name = "config-guid")] string configGuid,
			[FromBody] ConfigModel config)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(configGuid))
					return BadRequest("Missing config-guid parameter");

				var host = HttpContext?.Request?.Host.Host ?? "";
				var baseFolder = Utils.DataFolder(host);
				var configFolder = Path.Combine(baseFolder, "config");

				Directory.CreateDirectory(configFolder); // ensure directory exists

				var configFile = Path.Combine(configFolder, $"{configGuid}.json");

				var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
				{
					WriteIndented = true
				});

				await System.IO.File.WriteAllTextAsync(configFile, json);

				_statsManager.UpdateStatsFile(Utils.StatsFile(host), StatType.ConfigSaved);

				return Ok(new { message = "Config saved", guid = configGuid });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error writing config file: {ex.Message}");
			}
		}

		[HttpDelete("delete-config")]
		public IActionResult DeleteConfig([FromQuery(Name = "config-guid")] string configGuid)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(configGuid))
					return BadRequest("Missing config-guid parameter");

				var host = HttpContext?.Request?.Host.Host ?? "";
				var baseFolder = Utils.DataFolder(host);
				var configFolder = Path.Combine(baseFolder, "config");
				var configFile = Path.Combine(configFolder, $"{configGuid}.json");

				if (!System.IO.File.Exists(configFile))
					return NotFound("Config file not found");

				System.IO.File.Delete(configFile);

				_statsManager.UpdateStatsFile(Utils.StatsFile(host), StatType.ConfigDeleted);

				return Ok(new { message = "Config deleted", guid = configGuid });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error deleting config file: {ex.Message}");
			}
		}

		[HttpGet("get-key")]
		public IActionResult GetConfigKey()
		{
			try
			{
				return Ok(Utils.ConfigKey);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error retrieving config key: {ex.Message}");
			}
		}

		[HttpGet("stats")]
		public IActionResult GetStats()
		{
			try
			{
				var host = HttpContext?.Request?.Host.Host ?? "";
				var statsFilePath = Utils.StatsFile(host);

				var stats = _statsManager.LoadStatsFile(statsFilePath);

				_statsManager.UpdateStatsFile(Utils.StatsFile(host), StatType.StatsViewed);

				return new JsonResult(stats)
				{
					StatusCode = 200,
					ContentType = "application/json"
				};
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error reading stats file: {ex.Message}");
			}
		}
	}
}
