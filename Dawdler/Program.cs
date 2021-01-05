using Dawdler;
using Dawdler.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;

Log.Logger = new LoggerConfiguration()
#if DEBUG
		.MinimumLevel.Debug()
#else
		.MinimumLevel.Information()
		.MinimumLevel.Override(@"System.Net.Http.HttpClient", LogEventLevel.Warning)
#endif
		.MinimumLevel.Override(@"Microsoft", LogEventLevel.Information)
		.MinimumLevel.Override(@"Volo.Abp", LogEventLevel.Warning)
		.Enrich.FromLogContext()
		.WriteTo.Async(c => c.File(Constants.LogFile,
				outputTemplate: Constants.OutputTemplate,
				rollingInterval: RollingInterval.Day,
				fileSizeLimitBytes: Constants.MaxLogFileSize))
		.WriteTo.Async(c => c.Console(outputTemplate: Constants.OutputTemplate))
		.CreateLogger();
try
{
	Log.Information(@"开始运行...");
	await CreateHostBuilder(args).RunConsoleAsync();
	return 0;
}
catch (Exception ex)
{
	Log.Fatal(ex, @"未捕获异常！");
	return 1;
}
finally
{
	Log.CloseAndFlush();
}

static IHostBuilder CreateHostBuilder(string[] args)
{
	return Host.CreateDefaultBuilder(args)
			.UseAutofac()
			.UseSerilog()
			.ConfigureServices((_, services) =>
			{
				services.AddApplication<DawdlerModule>();
			});
}
