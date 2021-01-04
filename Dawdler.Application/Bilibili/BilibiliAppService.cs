using Dawdler.BilibiliDailyTasks;
using Dawdler.BilibiliUsers;
using Dawdler.Configs;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
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
		private readonly BilibiliUserManager _manager;

		private const int MaxDailyTaskRetryTimes = 5;

		private readonly AsyncSemaphore _locker = new(1);

		public BilibiliAppService(
			ILogger<BilibiliAppService> logger,
			BilibiliUsersConfig usersConfig,
			IEnumerable<IBilibiliDailyTask> dailyTasks,
			BilibiliUserManager manager)
		{
			_logger = logger;
			_usersConfig = usersConfig;
			_dailyTasks = dailyTasks;
			_manager = manager;
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
				foreach (var user in _usersConfig.Users)
				{
					await LoginAsync(user, token);
				}

				foreach (var user in _usersConfig.Users.Where(user => user.IsLogin is true))
				{
					foreach (var task in _dailyTasks)
					{
						await RunDailyTaskAsync(task, user, token);
					}
				}

				var countdown = await IAppService.GetNextDayCountdownAsync();
				_logger.LogInformation($@"每日任务完成，等待 {countdown} 后执行");
				await Task.Delay(countdown, token);
			}
		}

		private async ValueTask RunDailyTaskAsync(IBilibiliDailyTask task, BilibiliUser user, CancellationToken token)
		{
			task.User = user;
			var i = 0;
			do
			{
				try
				{
					await task.RunAsync(token);
				}
				catch (BilibiliNoLoginException)
				{
					_logger.LogError(@"[{0}] 账号未登录", user.Username);
				}
				catch (TaskCanceledException)
				{
					_logger.LogError(@"[{0}] 取消每日任务执行", user.Username);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, @"[{0}] 每日任务执行出错，重试 {1}", user.Username, ++i);
					await Task.Delay(TimeSpan.FromSeconds(i), token);
					continue;
				}
				break;
			} while (i < MaxDailyTaskRetryTimes);
		}

		private async ValueTask LoginAsync(BilibiliUser user, CancellationToken token)
		{
			using var _ = await _locker.EnterAsync(token);
			_manager.User = user;

			try
			{
				await _manager.CheckLoginStatusAsync(token);
				if (user.IsLogin is true)
				{
					return;
				}

				_logger.LogInformation(@"[{0}] 登录中...", user.Username);
				await _manager.LoginAsync(token);
				_logger.LogInformation(@"[{0}] 登录成功", user.Username);
			}
			catch (TaskCanceledException)
			{
				_logger.LogError(@"[{0}] 取消登录", user.Username);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, @"{0} 登录异常", user);
			}
		}
	}
}
