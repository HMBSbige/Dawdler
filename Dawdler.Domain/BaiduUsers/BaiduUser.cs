using JetBrains.Annotations;

namespace Dawdler.BaiduUsers
{
	[UsedImplicitly]
	public record BaiduUser
	{
		public string BDUSS { get; set; } = string.Empty;
	}
}
