using System.Text.Json;
using System.Text.RegularExpressions;
using HttpLogger.Server.Model;
using HttpLogger.Server.Statistics;
using HttpLogger.Server.ViewModel;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

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
				if (Request.Headers.TryGetValue(Utils.ConfigKey, out StringValues headerValue))
				{
					configGuid = headerValue.ToString();
				}

				// If not found in header, check query param
				if (string.IsNullOrWhiteSpace(configGuid) &&
					Request.Query.TryGetValue(Utils.ConfigKey, out StringValues queryValue))
				{
					configGuid = queryValue.ToString();
				}

				// Prepare response values
				int statusCode = 200;
				string responseBody = "";
				int responseDelay = 0;
				Dictionary<string, string> responseHeaders = new()
				{
					["Content-Type"] = "application/json"
				};

				string host = HttpContext?.Request?.Host.Host ?? "";
				string baseFolder = Utils.DataFolder(host);
				string configPath = Path.Combine(baseFolder, "config");

				HttpRequest? request = HttpContext?.Request;
				string appBaseUrl = request is not null
					? $"{request.Scheme}://{request.Host}{request.PathBase}/"
					: "https://[hostname]/HttpLogger/"; // fallback

				if (!string.IsNullOrWhiteSpace(configGuid))
				{
					string configFile = Path.Combine(configPath, $"{configGuid}.json");
					if (System.IO.File.Exists(configFile))
					{
						try
						{
							string configText = await System.IO.File.ReadAllTextAsync(configFile);
							ConfigModel? config = JsonSerializer.Deserialize<ConfigModel>(configText);

							if (config is null)
							{
								throw new Exception("Deserialized config is null");
							}

							// Touch the file to mark it as recently used
							System.IO.File.SetLastWriteTime(configFile, DateTime.Now);

							// Defaults from config
							statusCode = config.StatusCode;
							responseBody = config.Body ?? "";
							responseDelay = config.ResponseDelay;

							if (config.ResponseHeaders != null)
							{
								foreach (KeyValuePair<string, string> kvp in config.ResponseHeaders)
								{
									responseHeaders[kvp.Key] = kvp.Value;
								}
							}

							// Path-specific override (replace semantics; first match wins)
							PathResponseModel? matchedResponse = null;

							if (config.PathSpecificResponse != null)
							{

								// Build path relative to /api/ (no query string)
								string relativePath = "";

								string? pathValue = Request.Path.Value;
								if (!string.IsNullOrEmpty(pathValue))
								{
									const string apiSegment = "/api";

									int apiPos = pathValue.IndexOf(apiSegment, StringComparison.OrdinalIgnoreCase);
									if (apiPos >= 0)
									{
										// Move past "/api"
										int start = apiPos + apiSegment.Length;

										// Extract everything after "/api"
										relativePath = pathValue.Substring(start);
									}
								}

								relativePath = relativePath.TrimStart('/'); // no leading '/'
								foreach (PathSpecificRule rule in config.PathSpecificResponse)
								{
									if (rule == null)
									{
										continue;
									}

									if (rule.IsRegularExpression)
									{
										if (string.IsNullOrWhiteSpace(rule.Pattern))
										{
											continue;
										}

										RegexOptions options = RegexOptions.CultureInvariant;
										if (true == rule.IgnoreCase)
										{
											options |= RegexOptions.IgnoreCase;
										}

										// Regex author controls trailing slash behavior; use timeout for safety
										Regex regex = new(rule.Pattern, options, matchTimeout: TimeSpan.FromMilliseconds(100));

										if (regex.IsMatch(relativePath))
										{
											matchedResponse = rule.Response;
											break;
										}
									}
									else
									{
										// Literal match: trim trailing '/' from BOTH pattern and actual path
										string patternLiteral = (rule.Pattern ?? "").TrimStart('/').TrimEnd('/');
										string pathLiteral = relativePath.TrimEnd('/');

										if (patternLiteral == pathLiteral)
										{
											matchedResponse = rule.Response;
											break;
										}
									}
								}
							}

							if (matchedResponse != null)
							{
								// Replace (no union/merge)
								statusCode = matchedResponse.StatusCode;
								responseBody = matchedResponse.Body ?? "";

								responseHeaders = new Dictionary<string, string>()
								{
									["Content-Type"] = "application/json"
								};
								if (matchedResponse.ResponseHeaders != null)
								{
									foreach (KeyValuePair<string, string> kvp in matchedResponse.ResponseHeaders)
									{
										responseHeaders[kvp.Key] = kvp.Value;
									}
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
				Dictionary<string, string> requestHeaders = [];
				foreach (KeyValuePair<string, StringValues> header in Request.Headers)
				{
					requestHeaders[header.Key] = header.Value.ToString();
				}

				// Read request body
				string requestBody;
				using (StreamReader reader = new(Request.Body))
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
					ConfigGuid = configGuid,
					ResponseDelay = responseDelay
				};

				string today = DateTime.Now.ToString("yyyy-MM-dd");
				string folderPath = Path.Combine(baseFolder, today);
				Directory.CreateDirectory(folderPath);
				string logFile = Path.Combine(folderPath, $"{Guid.NewGuid()}.json");

				string logJson = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions
				{
					WriteIndented = true
				});

				await System.IO.File.WriteAllTextAsync(logFile, logJson);

				// Apply response delay if configured (global)
				if (responseDelay > 0)
				{
					await Task.Delay(responseDelay);
				}

				_statsManager.UpdateStatsFile(Utils.StatsFile(host), StatType.ApiInvoked);

				return new ContentResult
				{
					StatusCode = statusCode,
					Content = responseBody,
					ContentType = responseHeaders.TryGetValue("Content-Type", out string? ct) ? ct : "application/json"
				};
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Unhandled error: {ex.Message}\n\n{ex.InnerException}");
			}
		}
	}
}
