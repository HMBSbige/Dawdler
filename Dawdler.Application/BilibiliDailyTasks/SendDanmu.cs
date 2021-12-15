using BilibiliApi.Model.FansMedal;
using Dawdler.Bilibili;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Volo.Abp.DependencyInjection;

namespace Dawdler.BilibiliDailyTasks;

[UsedImplicitly]
[ExposeServices(typeof(IBilibiliDailyTask), typeof(IDailyTask))]
public class SendDanmu : BilibiliDailyTask
{
	private const int RetryTimes = 3;

	public SendDanmu(ILogger<BilibiliDailyTask> logger, BilibiliUserManager manager) : base(logger, manager)
	{
	}

	public override async ValueTask RunAsync(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		ICollection<FansMedalList> list;
		try
		{
			list = await Manager.GetLiveFansMedalListAsync(token);
		}
		catch (JsonException ex) when (ex.BytePositionInLine is 75 && ex.Path is @"$.data")
		{
			throw new BilibiliNoLoginException(@"获取粉丝勋章列表失败，可能未登录");
		}
		for (var i = 0; i < RetryTimes; ++i)
		{
			token.ThrowIfCancellationRequested();

			list = await SendDanmuAsync(list, token);
			if (list.Count == 0)
			{
				break;
			}

			await Task.Delay(TimeSpan.FromSeconds(1), token);
		}
	}

	private async ValueTask<ICollection<FansMedalList>> SendDanmuAsync(IEnumerable<FansMedalList> list, CancellationToken token)
	{
		if (User is null)
		{
			throw new ArgumentNullException(nameof(User));
		}

		var fail = new List<FansMedalList>();
		foreach (var fansMedal in list)
		{
			token.ThrowIfCancellationRequested();

			var header = $@"[{User.Username}] {fansMedal.uname}({fansMedal.roomid})";
			try
			{
				if (fansMedal.medal_level >= 20)
				{
					Logger.LogInformation(@"{0} 徽章等级 ≥ 20，跳过", header);
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
			catch (HttpRequestException ex) when (ex.Message.Contains(@"账号未登录"))
			{
				throw new BilibiliNoLoginException(ex.Message);
			}
			catch (Exception ex) when (ex is not TaskCanceledException)
			{
				Logger.LogError(ex, @"{0} 发送弹幕异常", header);
				fail.Add(fansMedal);
			}
		}
		return fail;
	}
}
