using Dawdler.Bilibili;

namespace Dawdler.BilibiliDailyTasks
{
	public interface IBilibiliDailyTask : IDailyTask
	{
		BilibiliUser? User { get; set; }
	}
}
