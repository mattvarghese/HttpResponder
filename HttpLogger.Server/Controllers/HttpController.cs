using System.Text.Json;
using HttpLogger.Server.Model;
using HttpLogger.Server.Statistics;
using HttpLogger.Server.ViewModel;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace HttpLogger.Server.Controllers
{
	[Route("api")]
	[ApiController]
	public class HttpController(StatsManager statsManager) : ControllerBase
	{
		private readonly StatsManager _statsManager = statsManager;

		// GET/POST/PUT/DELETE/PATCH to / or any path
		[HttpGet("{*path}")]
		[HttpPost("{*path}")]
		[HttpPut("{*path}")]
		[HttpDelete("{*path}")]
		[HttpPatch("{*path}")]
		public async Task<IActionResult> LogRequest()
		{
			try
			{
				string? configGuid = null;

				// First, check header
				if (Request.Headers.TryGetValue(Utils.ConfigKey, out var headerValue))
				{
					configGuid = headerValue.ToString();
				}

				// If not found in header, check query param
				if (string.IsNullOrWhiteSpace(configGuid) &&
					Request.Query.TryGetValue(Utils.ConfigKey, out var queryValue))
				{
					configGuid = queryValue.ToString();
				}

				// Prepare response values
				int statusCode = 200;
				string responseBody = "";
				int responseDelay = 0;
				var responseHeaders = new Dictionary<string, string>
				{
					["Content-Type"] = "application/json"
				};

				var host = HttpContext?.Request?.Host.Host ?? "";
				var baseFolder = Utils.DataFolder(host);
				string configPath = Path.Combine(baseFolder, "config");

				var request = HttpContext?.Request;
				var appBaseUrl = request is not null
					? $"{request.Scheme}://{request.Host}{request.PathBase}/"
					: "https://[hostname]/HttpLogger/"; // fallback


				if (!string.IsNullOrWhiteSpace(configGuid))
				{
					var configFile = Path.Combine(configPath, $"{configGuid}.json");
					if (System.IO.File.Exists(configFile))
					{
						try
						{
							var configText = await System.IO.File.ReadAllTextAsync(configFile);
							var config = JsonSerializer.Deserialize<ConfigModel>(configText);

							if (config is null)
							{
								throw new Exception("Deserialized config is null");
							}

							// Touch the file to mark it as recently used
							System.IO.File.SetLastWriteTime(configFile, DateTime.Now);

							statusCode = config.StatusCode;
							responseBody = config.Body ?? "";
							responseDelay = config.ResponseDelay;

							if (config.ResponseHeaders != null)
							{
								foreach (var kvp in config.ResponseHeaders)
								{
									responseHeaders[kvp.Key] = kvp.Value;
								}
							}
						}
						catch (Exception ex)
						{
							statusCode = 500;
							responseBody = JsonSerializer.Serialize(new
							{
								message = $"Error reading config file. Go to the app URL to learn how to use this endpoint.\n\n{ex.Message}\n\n{ex.InnerException}",
								appUrl = appBaseUrl
							});
						}
					}
					else
					{
						statusCode = 500;
						responseBody = JsonSerializer.Serialize(new
						{
							message = "Config file not found. Go to the app URL to learn how to use this endpoint.",
							appUrl = appBaseUrl
						});
					}
				}
				else
				{
					statusCode = 200;
					responseBody = JsonSerializer.Serialize(new
					{
						message = "You did not include a config GUID. Go to the app URL to learn how to use this endpoint.",
						appUrl = appBaseUrl
					});
				}

				// Add response headers
				foreach (KeyValuePair<string, string> header in responseHeaders)
				{
					Response.Headers[header.Key] = header.Value;
				}

				// Read request headers
				var requestHeaders = new Dictionary<string, string>();
				foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> header in Request.Headers)
				{
					requestHeaders[header.Key] = header.Value.ToString();
				}

				// Read request body
				string requestBody;
				using (var reader = new StreamReader(Request.Body))
				{
					requestBody = await reader.ReadToEndAsync();
				}

				// Construct log entry
				var logEntry = new
				{
					Timestamp = DateTime.Now.ToString("o"),
					HttpMethod = Request.Method,
					Url = Request.GetDisplayUrl(),
					RequestHeaders = requestHeaders,
					RequestBody = requestBody,
					ResponseHeaders = responseHeaders,
					ResponseBody = responseBody,
					HttpStatusCode = statusCode,
					ConfigGuid = configGuid
				};

				var today = DateTime.Now.ToString("yyyy-MM-dd");
				var folderPath = Path.Combine(baseFolder, today);
				Directory.CreateDirectory(folderPath);
				var logFile = Path.Combine(folderPath, $"{Guid.NewGuid()}.json");

				var logJson = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions
				{
					WriteIndented = true
				});

				await System.IO.File.WriteAllTextAsync(logFile, logJson);

				// Apply response delay if configured
				if (responseDelay > 0)
				{
					await Task.Delay(responseDelay);
				}

				_statsManager.UpdateStatsFile(Utils.StatsFile(host), StatType.ApiInvoked);

				return new ContentResult
				{
					StatusCode = statusCode,
					Content = responseBody,
					ContentType = responseHeaders.TryGetValue("Content-Type", out var ct) ? ct : "application/json"
				};
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Unhandled error: {ex.Message}\n\n{ex.InnerException}");
			}
		}

	}
}
