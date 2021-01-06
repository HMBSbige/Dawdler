using Dawdler.BaiduUsers;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Dawdler.BaiduDailyTasks
{
	public abstract class BaiduDailyTask : IBaiduDailyTask
	{
		protected readonly ILogger Logger;
		protected readonly BaiduUserManager Manager;

		private BaiduUser? _user;

		public BaiduUser? User
		{
			get => _user;
			set
			{
				_user = value;
				Manager.User = User;
			}
		}

		protected BaiduDailyTask(
			ILogger<BaiduDailyTask> logger,
			BaiduUserManager manager)
		{
			Logger = logger;
			Manager = manager;
		}

		public abstract ValueTask RunAsync(CancellationToken token);
	}
}
