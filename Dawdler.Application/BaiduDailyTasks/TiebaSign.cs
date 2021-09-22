using BilibiliLiveRecordDownLoader.Shared.Utils;
using Dawdler.Baidu;
using Dawdler.Tieba;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Dawdler.BaiduDailyTasks
{
	[UsedImplicitly]
	[ExposeServices(typeof(IBaiduDailyTask), typeof(IDailyTask))]
	public class TiebaSign : BaiduDailyTask
	{
		private const int RetryTimes = 3;

		public TiebaSign(ILogger<BaiduDailyTask> logger, BaiduUserManager manager) : base(logger, manager)
		{
		}

		public override async ValueTask RunAsync(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();

			var message = await Manager.GetForumsAsync(token);
			Logger.LogInformation(@"获取贴吧列表成功！");
			Logger.LogInformation(@"{0} 总共有 {1} 个贴吧", Timestamp.GetTime(message.time).ToLocalTime(), message.forum_list!.Length);
			Logger.LogDebug(@"TBS: {0}", message.anti!.tbs);
			foreach (var forum in message.forum_list)
			{
				Logger.LogDebug(@"{0}({1}):{2}级", forum.name, forum.id, forum.level_id);
			}

			var success = 0;
			var list = message.forum_list.ToList();
			for (var i = 0; i < RetryTimes; ++i)
			{
				token.ThrowIfCancellationRequested();

				var listCount = list.Count;
				list = await SignAsync(list, message, token);
				success += listCount - list.Count;
				if (list.Count == 0)
				{
					break;
				}

				await Task.Delay(TimeSpan.FromSeconds(1), token);
			}

			Logger.LogInformation(@"签到完成: {0}/{1}", success, message.forum_list.Length);
		}

		private async Task<List<Forum>> SignAsync(IEnumerable<Forum> list, ForumMessage message, CancellationToken token)
		{
			var failList = new List<Forum>();
			foreach (var forum in list)
			{
				token.ThrowIfCancellationRequested();
				try
				{
					var res = await Manager.SignAsync(forum, message, token);
					Logger.LogInformation($@"{Timestamp.GetTime(res.sign_time).ToLocalTime()} {forum.name}:{res.level_name}:今日本吧第 {res.user_sign_rank} 个签到，经验 +{res.sign_bonus_point}，漏签 {res.miss_sign_num} 天，连续签到 {res.cont_sign_num} 天");
				}
				catch (TiebaErrorException ex) when (ex.Error.error_code == @"160002")
				{
					Logger.LogInformation(@"{0} 已签到", forum.name);
				}
				catch (TiebaErrorException ex) when (ex.Error.error_code == @"340008")
				{
					Logger.LogInformation(@"{0} 在黑名单中，不能操作", forum.name);
				}
				catch (Exception ex) when (ex is not TaskCanceledException)
				{
					failList.Add(forum);
					Logger.LogError(ex, @"{0} 签到失败", forum.name);
				}
			}

			return failList;
		}
	}
}
