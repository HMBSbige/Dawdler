using BilibiliLiveRecordDownLoader.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Dawdler
{
	public interface IAppService : ITransientDependency
	{
		ValueTask StartAsync(CancellationToken token);

		protected static async ValueTask<TimeSpan> GetNextDayCountdownAsync()
		{
			var now = DateTime.UtcNow;
			try
			{
				now = await Ntp.GetCurrentTime();
			}
			catch
			{
				// ignored
			}

			now = now.AddHours(8);
			var nextDay = now.Date.AddDays(1);

			return (nextDay - now).Add(TimeSpan.FromSeconds(1));
		}
	}
}
