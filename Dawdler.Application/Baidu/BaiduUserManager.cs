using BilibiliLiveRecordDownLoader.Shared.Utils;
using Dawdler.Tieba;
using Dawdler.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Volo.Abp.DependencyInjection;
using UsedImplicitly = JetBrains.Annotations.UsedImplicitlyAttribute;

namespace Dawdler.Baidu;

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

	private static void CheckUser([NotNull] BaiduUser? user)
	{
		if (user is null)
		{
			throw new ArgumentNullException(nameof(user));
		}
	}

	private async Task<string> PostAsync(string url, HttpContent content, CancellationToken token)
	{
		using var _ = await _lock.EnterAsync(token);

		var client = CreateClient();

		var result = await client.PostAsync(url, content, token);

		var resultContent = await result.Content.ReadAsStringAsync(token);
		_logger.LogDebug(resultContent);
		return resultContent;
	}

	private async Task<string> GetSignMessageAsync(long fid, string kw, string tbs, CancellationToken token)
	{
		CheckUser(User);

		var sign = Md5.ToHexString($@"BDUSS={User.BDUSS}fid={fid}kw={kw}tbs={tbs}tiebaclient!!!").ToUpper();
		var pair = new Dictionary<string, string>
		{
			{@"BDUSS", User.BDUSS},
			{@"fid", fid.ToString()},
			{@"kw", kw},
			{@"tbs", tbs},
			{@"sign", sign}
		};
		using var content = new FormUrlEncodedContent(pair.Cast());

		return await PostAsync(SignUrl, content, token);
	}

	private async Task<string> GetForumsMessageAsync(CancellationToken token)
	{
		CheckUser(User);

		var sign = Md5.ToHexString($@"BDUSS={User.BDUSS}tiebaclient!!!").ToUpper();

		var pair = new Dictionary<string, string>
		{
			{@"BDUSS", User.BDUSS},
			{@"sign", sign}
		};
		using var content = new FormUrlEncodedContent(pair.Cast());

		return await PostAsync(TiebaListUrl, content, token);
	}

	private static Exception HandleError(string json)
	{
		var error = JsonSerializer.Deserialize<ErrorMessage>(json);
		if (error is null || error.error_code == @"0")
		{
			return new JsonException();
		}

		return new TiebaErrorException(error);
	}

	public async Task<ForumMessage> GetForumsAsync(CancellationToken token)
	{
		var json = await GetForumsMessageAsync(token);
		try
		{
			throw HandleError(json);
		}
		catch (JsonException)
		{
			var message = JsonSerializer.Deserialize<ForumMessage>(json);
			if (message?.forum_list is null)
			{
				throw new HttpRequestException(@"获取贴吧列表失败！");
			}
			return message;
		}
	}

	public async Task<SignInfo> SignAsync(Forum forum, ForumMessage forumMessage, CancellationToken token)
	{
		var json = await GetSignMessageAsync(forum.id, forum.name!, forumMessage.anti!.tbs!, token);
		try
		{
			throw HandleError(json);
		}
		catch (JsonException)
		{
			var message = JsonSerializer.Deserialize<SignMessage>(json);
			if (message?.user_info is null)
			{
				throw new HttpRequestException(@"获取签到信息失败！");
			}
			return message.user_info;
		}
	}
}
