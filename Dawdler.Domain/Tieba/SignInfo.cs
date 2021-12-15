using JetBrains.Annotations;

namespace Dawdler.Tieba;

[UsedImplicitly]
public class SignInfo
{
	/// <summary>
	/// 本日签到排名
	/// </summary>
	public long user_sign_rank { get; set; }

	/// <summary>
	/// 签到时间戳
	/// </summary>
	public long sign_time { get; set; }

	/// <summary>
	/// 连续签到天数
	/// </summary>
	public long cont_sign_num { get; set; }

	/// <summary>
	/// 签到经验
	/// </summary>
	public long sign_bonus_point { get; set; }

	/// <summary>
	/// 漏签天数
	/// </summary>
	public long miss_sign_num { get; set; }

	/// <summary>
	/// 等级名称
	/// </summary>
	public string? level_name { get; set; }
}
