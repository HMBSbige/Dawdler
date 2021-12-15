using BilibiliLiveRecordDownLoader.Shared.Utils;
using Volo.Abp.DependencyInjection;

namespace Dawdler;

public interface IAppService : ITransientDependency
{
	ValueTask StartAsync(CancellationToken token);

	protected static async ValueTask<TimeSpan> GetNextDayCountdownAsync()
	{
		var now = await Ntp.GetCurrentTimeAsync();

		now = now.AddHours(8);
		var nextDay = now.Date.AddDays(1);

		return (nextDay - now).Add(TimeSpan.FromSeconds(1));
	}
}
