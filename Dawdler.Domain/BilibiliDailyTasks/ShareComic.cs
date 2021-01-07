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
	[ExposeServices(typeof(IBilibiliDailyTask), typeof(IDailyTask))]
	public class ShareComic : BilibiliDailyTask
	{
		public ShareComic(ILogger<ShareComic> logger, BilibiliUserManager manager) : base(logger, manager)
		{
		}

		public override async ValueTask RunAsync(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();

			if (User is null)
			{
				throw new ArgumentNullException(nameof(User));
			}

			Logger.LogInformation(@"[{0}] 开始每日漫画分享", User.Username);
			try
			{
				if (await Manager.ShareComicAsync(token) is false)
				{
					throw new Exception(@"每日漫画分享失败，未知错误");
				}
			}
			catch (HttpRequestException ex) when (ex.Message.Contains(@"must login"))
			{
				throw new BilibiliNoLoginException(ex.Message);
			}

			Logger.LogInformation(@"[{0}] 每日漫画分享成功", User.Username);
		}
	}
}
