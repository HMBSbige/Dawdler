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
	public class MangaClockIn : BilibiliDailyTask
	{
		public MangaClockIn(ILogger<BilibiliDailyTask> logger, BilibiliUserManager manager) : base(logger, manager)
		{
		}

		public override async ValueTask RunAsync(CancellationToken token)
		{
			if (User is null)
			{
				throw new ArgumentNullException(nameof(User));
			}

			Logger.LogInformation(@"[{0}] 开始每日漫画签到", User.Username);
			try
			{
				if (await Manager.MangaClockInAsync(token) is false)
				{
					throw new Exception(@"每日漫画签到失败，未知错误");
				}
			}
			catch (HttpRequestException ex) when (ex.Message.Contains(@"uid must > 0"))
			{
				throw new BilibiliNoLoginException();
			}
			Logger.LogInformation(@"[{0}] 每日漫画签到成功", User.Username);
		}
	}
}
