using Dawdler.Configs;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dawdler.Bilibili
{
	[UsedImplicitly]
	public class BilibiliAppService : IAppService
	{
		private readonly ILogger _logger;
		private readonly BilibiliUsersConfig _usersConfig;
		private readonly IEnumerable<IBilibiliDailyTask> _dailyTasks;

		public BilibiliAppService(
			ILogger<BilibiliAppService> logger,
			BilibiliUsersConfig usersConfig,
			IEnumerable<IBilibiliDailyTask> dailyTasks)
		{
			_logger = logger;
			_usersConfig = usersConfig;
			_dailyTasks = dailyTasks;
		}

		public async ValueTask StartAsync(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				await _usersConfig.LoadAsync(token);

				var day = RunDailyTaskAsync(token);

				await day;

				var wait = TimeSpan.FromMinutes(1);
				_logger.LogInformation($@"等待 {wait} 后执行");
				await Task.Delay(wait, token);
			}
		}

		private async ValueTask RunDailyTaskAsync(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				var countdown = await IAppService.GetNextDayCountdownAsync();
				foreach (var user in _usersConfig.Users)
				{
					foreach (var task in _dailyTasks)
					{
						task.User = user;
						await task.RunAsync(token);
					}
				}

				_logger.LogInformation($@"每日任务完成，等待 {countdown} 后执行");
				await Task.Delay(countdown, token);
			}
		}
	}
}
