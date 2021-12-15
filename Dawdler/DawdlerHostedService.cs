using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

namespace Dawdler;

public class DawdlerHostedService : IHostedService
{
	private readonly ILogger _logger;
	private readonly IEnumerable<IAppService> _services;
	private readonly CancellationTokenSource _cts = new();

	public DawdlerHostedService(
		ILogger<DawdlerHostedService> logger,
		IEnumerable<IAppService> services)
	{
		_logger = logger;
		_services = services;
	}

	public Task StartAsync(CancellationToken token)
	{
		_logger.LogDebug(@"Start DawdlerHostedService");

		foreach (IAppService service in _services)
		{
			service.StartAsync(_cts.Token).Forget();
		}

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken token)
	{
		_logger.LogDebug(@"Stop DawdlerHostedService");
		_cts.Cancel();

		return Task.CompletedTask;
	}
}
