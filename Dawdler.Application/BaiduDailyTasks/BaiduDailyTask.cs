using Dawdler.Baidu;
using Microsoft.Extensions.Logging;

namespace Dawdler.BaiduDailyTasks;

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
