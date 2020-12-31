using Dawdler.BilibiliUsers;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Dawdler.BilibiliDailyTasks
{
	[UsedImplicitly]
	[ExposeServices(typeof(MangaClockIn), typeof(IBilibiliDailyTask), typeof(IDailyTask))]
	public class MangaClockIn : IBilibiliDailyTask
	{
		private readonly ILogger _logger;
		private readonly BilibiliUserManager _manager;

		private BilibiliUser? _user;
		public BilibiliUser? User
		{
			get => _user;
			set
			{
				_user = value;
				_manager.User = User;
			}
		}

		public MangaClockIn(
			ILogger<MangaClockIn> logger,
			BilibiliUserManager manager)
		{
			_logger = logger;
			_manager = manager;
		}

		public async ValueTask RunAsync(CancellationToken token)
		{
			if (User is null)
			{
				throw new ArgumentNullException(nameof(User));
			}

			_logger.LogInformation(@"{0} 开始每日漫画签到", User.Username);
			try
			{
				if (await _manager.MangaClockInAsync(token) is false)
				{
					throw new Exception(@"签到失败，未知错误");
				}
			}
			catch (HttpRequestException ex) when (ex.Message.Contains(@"uid must > 0"))
			{
				throw new BilibiliNoLoginException();
			}
		}
	}
}
