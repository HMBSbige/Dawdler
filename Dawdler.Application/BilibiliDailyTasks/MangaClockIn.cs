using Dawdler.Bilibili;
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
	[ExposeServices(typeof(IBilibiliDailyTask), typeof(IDailyTask))]
	public class MangaClockIn : BilibiliDailyTask
	{
		public MangaClockIn(ILogger<MangaClockIn> logger, BilibiliUserManager manager) : base(logger, manager)
		{
		}

		public override async ValueTask RunAsync(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();

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
				throw new BilibiliNoLoginException(ex.Message);
			}
			Logger.LogInformation(@"[{0}] 每日漫画签到成功", User.Username);

			try
			{
				var message = await Manager.GetMangaClockInInfoAsync(token);
				var status = message.data!.status switch
				{
					0 => @"未签到",
					1 => @"已签到",
					_ => @"未知"
				};
				Logger.LogInformation(@"[{0}] 漫画 [{1}] [{2}] 天", User.Username, status, message.data.day_count);
			}
			catch
			{
				// ignored
			}
		}
	}
}
