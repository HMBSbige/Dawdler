using JetBrains.Annotations;

namespace Dawdler.Tieba
{
	[UsedImplicitly]
	public class Forum
	{
		/// <summary>
		/// 贴吧名字
		/// </summary>
		public string? name { get; set; }

		/// <summary>
		/// fid
		/// </summary>
		public long id { get; set; }

		/// <summary>
		/// 等级
		/// </summary>
		public long level_id { get; set; }
	}
}
