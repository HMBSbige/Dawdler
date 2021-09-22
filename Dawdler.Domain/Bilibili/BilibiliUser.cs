using JetBrains.Annotations;
using System.Text.Json.Serialization;

namespace Dawdler.Bilibili
{
	[UsedImplicitly]
	public record BilibiliUser
	{
		public string Username { get; set; } = string.Empty;

		public string Password { get; set; } = string.Empty;

		public string AccessToken { get; set; } = string.Empty;

		public string RefreshToken { get; set; } = string.Empty;

		/// <summary>
		/// 等价于 Cookie 中的 bili_jct
		/// </summary>
		public string Csrf { get; set; } = string.Empty;

		public string Cookie { get; set; } = string.Empty;

		[JsonIgnore]
		public bool? IsLogin { get; set; }
	}
}
