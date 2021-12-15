using Dawdler.Baidu;

namespace Dawdler.BaiduDailyTasks;

public interface IBaiduDailyTask : IDailyTask
{
	BaiduUser? User { get; set; }
}
