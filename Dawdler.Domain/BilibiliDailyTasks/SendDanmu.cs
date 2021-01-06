using Dawdler.BilibiliUsers;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Dawdler.BilibiliDailyTasks
{
	[UsedImplicitly]
	[ExposeServices(typeof(IBilibiliDailyTask), typeof(IDailyTask))]
	public class SendDanmu : BilibiliDailyTask
	{
		public SendDanmu(ILogger<BilibiliDailyTask> logger, BilibiliUserManager manager) : base(logger, manager)
		{
		}

		public override async ValueTask RunAsync(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();

			if (User is null)
			{
				throw new ArgumentNullException(nameof(User));
			}

			var list = await Manager.GetLiveFansMedalListAsync(token);
			foreach (var fansMedal in list)
			{
				token.ThrowIfCancellationRequested();

				var header = $@"[{User.Username}] {fansMedal.uname}({fansMedal.roomid})";
				try
				{
					if (fansMedal.medal_level > 20)
					{
						Logger.LogInformation(@"{0} 徽章等级大于 20，跳过", header);
						continue;
					}

					var realId = await Manager.GetRealRoomIdAsync(fansMedal.roomid, token);
					if (realId != fansMedal.roomid)
					{
						Logger.LogInformation(@"{0} 真实房间号为 {3}", header, realId);
					}

					Logger.LogInformation(@"[{0}] {1}({2}) 发送弹幕", User.Username, fansMedal.uname, realId);

					await Manager.SendDanmuAsync(realId, token);
					await Task.Delay(TimeSpan.FromSeconds(1), token);
				}
				catch (Exception ex) when (ex is not TaskCanceledException)
				{
					Logger.LogError(ex, @"{0} 发送弹幕异常", header);
				}
			}
		}
	}
}
