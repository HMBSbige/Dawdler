using Dawdler.BilibiliUsers;
using JetBrains.Annotations;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Dawdler.BilibiliDailyTasks
{
	[UsedImplicitly]
	[ExposeServices(typeof(IBilibiliDailyTask))]
	public class MangaClockIn : IBilibiliDailyTask
	{
		public BilibiliUser? User { get; set; }

		private readonly BilibiliUserManager _manager;

		public MangaClockIn(BilibiliUserManager manager)
		{
			_manager = manager;
		}

		public ValueTask RunAsync(CancellationToken token)
		{
			throw new System.NotImplementedException();
		}
	}
}
