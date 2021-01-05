using BilibiliApi.Clients;
using BilibiliApi.Model.FansMedal;
using BilibiliApi.Model.Login.Password.OAuth2;
using BilibiliApi.Model.Manga.GetClockInInfo;
using BilibiliLiveRecordDownLoader.Shared.Utils;
using Dawdler.Configs;
using Dawdler.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using UsedImplicitly = JetBrains.Annotations.UsedImplicitlyAttribute;

namespace Dawdler.BilibiliUsers
{
	[UsedImplicitly]
	public class BilibiliUserManager : ITransientDependency
	{
		private readonly ILogger _logger;
		private readonly IHttpClientFactory _clientFactory;
		private readonly BilibiliUsersConfig _usersConfig;

		public BilibiliUser? User { get; set; }

		public BilibiliUserManager(
			ILogger<BilibiliUserManager> logger,
			IHttpClientFactory clientFactory,
			BilibiliUsersConfig usersConfig)
		{
			_logger = logger;
			_clientFactory = clientFactory;
			_usersConfig = usersConfig;
		}

		private BilibiliApiClient CreateClient([NotNull] BilibiliUser? user)
		{
			if (user is null)
			{
				throw new ArgumentNullException(nameof(user));
			}

			var client = _clientFactory.CreateClient(HttpClientName.Bilibili);
			if (!string.IsNullOrWhiteSpace(user.Cookie))
			{
				_logger.LogDebug($@"Cookie: {user.Cookie}");
				client.DefaultRequestHeaders.Add(@"Cookie", user.Cookie);
			}

			return new BilibiliApiClient(client);
		}

		private static string ToCookie(IEnumerable<BilibiliCookie> cookies)
		{
			var hashSet = new HashSet<string>();
			foreach (var cookie in cookies)
			{
				hashSet.Add($@"{cookie.name}={cookie.value}");
			}

			return string.Join(';', hashSet);
		}

		private async ValueTask HandleAppLoginMessageAsync(AppLoginMessage message, CancellationToken token)
		{
			var tokenInfo = message.data!.token_info!;
			User!.AccessToken = tokenInfo.access_token!;
			User.RefreshToken = tokenInfo.refresh_token!;

			var cookies = message.data.cookie_info!.cookies!;
			User.Csrf = cookies.First(x => x.name == @"bili_jct").value!;
			User.Cookie = ToCookie(cookies);
			User.IsLogin = true;

			await _usersConfig.SaveAsync(token);
		}

		private static bool IsToken([NotNullWhen(true)] string? token)
		{
			return !string.IsNullOrWhiteSpace(token) && token.Length == 32;
		}

		public async Task CheckLoginStatusAsync(CancellationToken token)
		{
			var client = CreateClient(User);
			try
			{
				User.IsLogin = await client.CheckLoginStatusAsync(token);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, @"检查登录状态失败");
				User.IsLogin = null;
			}
		}

		public async Task LoginAsync(CancellationToken token)
		{
			if (string.IsNullOrWhiteSpace(User?.Username) || string.IsNullOrWhiteSpace(User.Password))
			{
				throw new BilibiliNoLoginException(@"无用户名或密码");
			}

			var client = CreateClient(User);
			var message = await client.LoginAsync(User.Username, User.Password, token);

			await HandleAppLoginMessageAsync(message, token);
		}

		public async Task RefreshTokenAsync(CancellationToken token)
		{
			if (!IsToken(User?.AccessToken) || !IsToken(User.RefreshToken))
			{
				throw new Exception(@"Token 格式错误");
			}

			var client = CreateClient(User);
			var message = await client.RefreshTokenAsync(User.AccessToken, User.RefreshToken, token);

			await HandleAppLoginMessageAsync(message, token);
		}

		public async Task<bool?> MangaClockInAsync(CancellationToken token)
		{
			var client = CreateClient(User);
			return await client.MangaClockInAsync(User.AccessToken, token);
		}

		public async Task<bool?> ShareComicAsync(CancellationToken token)
		{
			var client = CreateClient(User);
			return await client.ShareComicAsync(User.AccessToken, token);
		}

		public async Task<GetClockInInfoMessage> GetMangaClockInInfoAsync(CancellationToken token)
		{
			var client = CreateClient(User);
			return await client.GetMangaClockInInfoAsync(User.AccessToken, token);
		}

		public async Task<List<FansMedalList>> GetLiveFansMedalListAsync(CancellationToken token)
		{
			var client = CreateClient(User);
			return await client.GetLiveFansMedalListAsync(token);
		}

		public async Task SendDanmuAsync(long roomId, CancellationToken token)
		{
			var client = CreateClient(User);
			await client.SendDanmuAsync(roomId, User.Csrf, rnd: Timestamp.GetTimestamp(DateTime.UtcNow).ToString(), token: token);
		}

		public async Task<long> GetRealRoomIdAsync(long roomId, CancellationToken token)
		{
			var client = CreateClient(User);
			var message = await client.GetRoomInitAsync(roomId, token);
			if (message?.data is null)
			{
				throw new HttpRequestException(@"获取真实房间号出错");
			}
			return message.data.room_id;
		}

		public async Task<TimeSpan> GetTokenExpireTimeAsync(CancellationToken token)
		{
			var client = CreateClient(User);
			var message = await client.GetTokenInfoAsync(User.AccessToken, token);
			if (message.data is null)
			{
				throw new HttpRequestException(@"获取 Token 信息失败");
			}
			return TimeSpan.FromSeconds(message.data.expires_in);
		}
	}
}
