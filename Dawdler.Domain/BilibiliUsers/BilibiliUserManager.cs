using BilibiliApi.Clients;
using BilibiliApi.Model.Login.Password.OAuth2;
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

namespace Dawdler.BilibiliUsers
{
	[JetBrains.Annotations.UsedImplicitly]
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
				throw new BilibiliNoLoginException();
			}

			var client = CreateClient(User);
			var message = await client.LoginAsync(User.Username, User.Password, token);

			var tokenInfo = message.data!.token_info!;
			User.AccessToken = tokenInfo.access_token!;
			User.RefreshToken = tokenInfo.refresh_token!;

			var cookies = message.data.cookie_info!.cookies!;
			User.Csrf = cookies.First(x => x.name == @"bili_jct").value!;
			User.Cookie = ToCookie(cookies);
			User.IsLogin = true;

			await _usersConfig.SaveAsync(token);
		}

		public async Task<bool?> MangaClockInAsync(CancellationToken token)
		{
			var client = CreateClient(User);
			return await client.MangaClockInAsync(User.AccessToken, token);
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
	}
}
