using JetBrains.Annotations;

namespace Dawdler.Tieba;

[UsedImplicitly]
public class ErrorMessage
{
	/// <summary>
	/// 错误代码
	/// </summary>
	public string? error_code { get; set; }

	/// <summary>
	/// 错误信息
	/// </summary>
	public string? error_msg { get; set; }

	/// <summary>
	/// 当前时间戳
	/// </summary>
	public long time { get; set; }
}
