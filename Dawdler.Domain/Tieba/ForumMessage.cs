using JetBrains.Annotations;

namespace Dawdler.Tieba
{
	[UsedImplicitly]
	public class ForumMessage
	{
		/// <summary>
		/// 贴吧列表
		/// </summary>
		public Forum[]? forum_list { get; set; }

		public Anti? anti { get; set; }

		/// <summary>
		/// 当前时间戳
		/// </summary>
		public long time { get; set; }
	}
}
