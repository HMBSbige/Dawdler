using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Dawdler
{
	public class DawdlerHostedService : IHostedService
	{
		private readonly ILogger _logger;

		public DawdlerHostedService(
			ILogger<DawdlerHostedService> logger)
		{
			_logger = logger;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogDebug(@"Start DawdlerHostedService");

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogDebug(@"Stop DawdlerHostedService");

			return Task.CompletedTask;
		}
	}
}
