using Dawdler.BilibiliUsers;

namespace Dawdler.BilibiliDailyTasks
{
	public interface IBilibiliDailyTask : IDailyTask
	{
		BilibiliUser? User { get; set; }
	}
}
