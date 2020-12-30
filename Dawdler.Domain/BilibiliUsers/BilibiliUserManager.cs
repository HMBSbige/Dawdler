using BilibiliApi.Clients;
using BilibiliApi.Model.Login.Password.OAuth2;
using Dawdler.Configs;
using Dawdler.Utils;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

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

		private BilibiliApiClient CreateClient()
		{
			if (User is null)
			{
				throw new ArgumentNullException(nameof(User));
			}

			var client = _clientFactory.CreateClient(HttpClientName.Bilibili);
			if (!string.IsNullOrWhiteSpace(User.Cookie))
			{
				_logger.LogDebug($@"Cookie: {User.Cookie}");
				client.DefaultRequestHeaders.Add(@"Cookie", User.Cookie);
			}

			return new BilibiliApiClient(client);
		}

		public async Task<bool> CheckLoginStatusAsync(CancellationToken token)
		{
			var client = CreateClient();
			return await client.CheckLoginStatusAsync(token);
		}

		public async Task LoginAsync(CancellationToken token)
		{
			if (string.IsNullOrWhiteSpace(User?.Username) || string.IsNullOrWhiteSpace(User.Password))
			{
				throw new BilibiliNoLoginException();
			}

			var client = CreateClient();
			var message = await client.LoginAsync(User.Username, User.Password, token);

			var tokenInfo = message.data!.token_info!;
			User.AccessToken = tokenInfo.access_token!;
			User.RefreshToken = tokenInfo.refresh_token!;

			var cookies = message.data.cookie_info!.cookies!;
			User.Csrf = cookies.Single(x => x.name == @"bili_jct").value!;
			User.Cookie = ToCookie(cookies);

			await _usersConfig.SaveAsync(token);
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
