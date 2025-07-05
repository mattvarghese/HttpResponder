using System.Text.Json;
using System.Text.Json.Serialization;
using HttpLogger.Server.Statistics;

namespace DaVinciTester.Server
{
	public class Program
	{
		public static void Main(string[] args)
		{
			WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllers();

			// Add cors.
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("DynamicCors", policy =>
				{
					policy
						.SetIsOriginAllowed(origin => !string.IsNullOrWhiteSpace(origin)) // Allow any non-empty origin
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials(); // Only use this if you need it
				});
			});


			// Options to use for JSON serialization / deserialization
			JsonSerializerOptions jsonOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = true
			};
			jsonOptions.Converters.Add(new JsonStringEnumConverter());
			builder.Services.AddSingleton(jsonOptions);

			StatsManager statsManager = new StatsManager(jsonOptions);
			builder.Services.AddSingleton(statsManager);

			WebApplication app = builder.Build();

			// These are required when running published
			// so that the static files are served up correctly
			app.UseDefaultFiles();
			app.UseStaticFiles();

			// Configure the HTTP request pipeline.

			app.UseHttpsRedirection();

			//app.UseAuthorization();

			app.UseCors("DynamicCors");

			app.MapControllers();

			// This would be required if we use react-router-dom
			//app.MapFallbackToFile("/index.html");

			app.Run();
		}
	}
}