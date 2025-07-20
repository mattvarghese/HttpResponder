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

			JsonSerializerOptions jsonOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = true
			};
			jsonOptions.Converters.Add(new JsonStringEnumConverter());
			builder.Services.AddSingleton(jsonOptions);

			builder.Services.AddControllers().AddJsonOptions(opts =>
			{
				opts.JsonSerializerOptions.PropertyNamingPolicy = jsonOptions.PropertyNamingPolicy;
				opts.JsonSerializerOptions.WriteIndented = jsonOptions.WriteIndented;

				foreach (JsonConverter converter in jsonOptions.Converters)
				{
					opts.JsonSerializerOptions.Converters.Add(converter);
				}
			});

			builder.Services.AddCors(options =>
			{
				options.AddPolicy("DynamicCors", policy =>
				{
					policy
						.SetIsOriginAllowed(origin => !string.IsNullOrWhiteSpace(origin))
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials();
				});
			});

			StatsManager statsManager = new StatsManager(jsonOptions);
			builder.Services.AddSingleton(statsManager);

			WebApplication app = builder.Build();

			app.UseDefaultFiles();
			app.UseStaticFiles();

			// Configure the HTTP request pipeline.

			app.UseHttpsRedirection();

			//app.UseAuthorization();

			app.UseCors("DynamicCors");

			app.MapControllers();

			//app.MapFallbackToFile("/index.html");

			app.Run();
		}
	}
}