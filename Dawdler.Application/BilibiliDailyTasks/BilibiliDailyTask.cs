using Dawdler.Bilibili;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Dawdler.BilibiliDailyTasks
{
	public abstract class BilibiliDailyTask : IBilibiliDailyTask
	{
		protected readonly ILogger Logger;
		protected readonly BilibiliUserManager Manager;

		private BilibiliUser? _user;
		public BilibiliUser? User
		{
			get => _user;
			set
			{
				_user = value;
				Manager.User = User;
			}
		}

		protected BilibiliDailyTask(
			ILogger<BilibiliDailyTask> logger,
			BilibiliUserManager manager)
		{
			Logger = logger;
			Manager = manager;
		}

		public abstract ValueTask RunAsync(CancellationToken token);
	}
}
