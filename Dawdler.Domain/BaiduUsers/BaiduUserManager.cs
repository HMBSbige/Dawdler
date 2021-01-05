using BilibiliLiveRecordDownLoader.Shared.Utils;
using Dawdler.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using UsedImplicitly = JetBrains.Annotations.UsedImplicitlyAttribute;

namespace Dawdler.BaiduUsers
{
	[UsedImplicitly]
	public class BaiduUserManager : ITransientDependency
	{
		private readonly ILogger _logger;
		private readonly IHttpClientFactory _clientFactory;
		private readonly AsyncSemaphore _lock = new(1);

		public BaiduUser? User { get; set; }

		private const string SignUrl = @"http://c.tieba.baidu.com/c/c/forum/sign";
		private const string TiebaListUrl = @"http://c.tieba.baidu.com/c/f/forum/favocommend";

		public BaiduUserManager(
			ILogger<BaiduUserManager> logger,
			IHttpClientFactory clientFactory)
		{
			_logger = logger;
			_clientFactory = clientFactory;
		}

		private HttpClient CreateClient()
		{
			return _clientFactory.CreateClient(HttpClientName.Baidu);
		}

		private async Task<string> PostAsync(string url, HttpContent content, CancellationToken token)
		{
			using var _ = _lock.EnterAsync(token);

			var client = CreateClient();

			var result = await client.PostAsync(url, content, token);

			var resultContent = await result.Content.ReadAsStringAsync(token);
			_logger.LogDebug(resultContent);
			return resultContent;
		}

		public async Task<string> SignAsync(string BDUSS, long fid, string kw, string tbs, CancellationToken token)
		{
			var sign = Md5.ComputeHash($@"BDUSS={BDUSS}fid={fid}kw={kw}tbs={tbs}tiebaclient!!!").ToUpper();
			var pair = new Dictionary<string, string>
			{
				{@"BDUSS", BDUSS},
				{@"fid", fid.ToString()},
				{@"kw", kw},
				{@"tbs", tbs},
				{@"sign", sign}
			};
			using var content = new FormUrlEncodedContent(pair.Cast());

			return await PostAsync(SignUrl, content, token);
		}

		public async Task<string> GetForumAsync(string BDUSS, CancellationToken token)
		{
			var sign = Md5.ComputeHash($@"BDUSS={BDUSS}tiebaclient!!!").ToUpper();

			var pair = new Dictionary<string, string>
			{
				{@"BDUSS", BDUSS},
				{@"sign", sign}
			};
			using var content = new FormUrlEncodedContent(pair.Cast());

			return await PostAsync(TiebaListUrl, content, token);
		}
	}
}
