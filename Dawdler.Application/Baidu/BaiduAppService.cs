using Dawdler.BaiduDailyTasks;
using Dawdler.Configs;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Dawdler.Baidu
{
	[UsedImplicitly]
	public class BaiduAppService : IAppService
	{
		private readonly ILogger _logger;
		private readonly BaiduUsersConfig _usersConfig;
		private readonly IEnumerable<IBaiduDailyTask> _dailyTasks;

		private const int MaxDailyTaskRetryTimes = 5;

		public BaiduAppService(
			ILogger<BaiduAppService> logger,
			BaiduUsersConfig usersConfig,
			IEnumerable<IBaiduDailyTask> dailyTasks)
		{
			_logger = logger;
			_usersConfig = usersConfig;
			_dailyTasks = dailyTasks;
		}

		public async ValueTask StartAsync(CancellationToken token)
		{
			_logger.LogInformation(@"[Baidu] 启动...");
			try
			{
				if (!File.Exists(_usersConfig.FilePath))
				{
					_logger.LogInformation(@"不存在 {0}", Path.GetFullPath(_usersConfig.FilePath));
					return;
				}
				while (!token.IsCancellationRequested)
				{
					await _usersConfig.LoadAsync(token);

					var day = RunDailyTaskAsync(token);

					await day;

					var wait = TimeSpan.FromMinutes(1);
					_logger.LogInformation($@"[Baidu] 等待 {wait} 后再次执行");
					await Task.Delay(wait, token);
				}
			}
			finally
			{
				_logger.LogInformation(@"[Baidu] 已停止");
			}
		}

		private async ValueTask RunDailyTaskAsync(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				foreach (var user in _usersConfig.Users)
				{
					token.ThrowIfCancellationRequested();
					foreach (var task in _dailyTasks)
					{
						token.ThrowIfCancellationRequested();
						await RunDailyTaskAsync(task, user, token);
					}
				}

				var countdown = await IAppService.GetNextDayCountdownAsync();
				_logger.LogInformation($@"[Baidu] 每日任务完成，等待 {countdown} 后执行");
				await Task.Delay(countdown, token);
			}
		}

		private async ValueTask RunDailyTaskAsync(IBaiduDailyTask task, BaiduUser user, CancellationToken token)
		{
			task.User = user;
			var i = 0;
			do
			{
				token.ThrowIfCancellationRequested();
				try
				{
					await task.RunAsync(token);
				}
				catch (TaskCanceledException)
				{
					_logger.LogWarning(@"[Baidu] 取消每日任务执行");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, @"[Baidu] [{0}] 每日任务执行出错，重试 {1}", user, ++i);
					await Task.Delay(TimeSpan.FromSeconds(i), token);
					continue;
				}
				break;
			} while (i < MaxDailyTaskRetryTimes);
		}
	}
}
