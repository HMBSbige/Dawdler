using JetBrains.Annotations;

namespace Dawdler.Baidu;

[UsedImplicitly]
public record BaiduUser
{
	public string BDUSS { get; set; } = string.Empty;
}
