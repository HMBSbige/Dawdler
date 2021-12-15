using Dawdler.Baidu;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dawdler.Configs;

[UsedImplicitly]
public class BaiduUsersConfig : ConfigBase
{
	public List<BaiduUser> Users { get; } = new();

	public BaiduUsersConfig(ILogger<ConfigBase> logger) : base(logger)
	{
	}

	public override string FilePath => ConfigPath.BaiduUsers;

	public override string BackupFilePath => ConfigPath.BaiduUsersBackup;

	protected override async ValueTask SaveToStreamAsync(Stream fs, JsonSerializerOptions options, CancellationToken token)
	{
		await JsonSerializer.SerializeAsync(fs, Users, options, token);
	}

	protected override async ValueTask LoadFromStreamAsync(Stream fs, CancellationToken token)
	{
		var users = await JsonSerializer.DeserializeAsync<List<BaiduUser>>(fs, cancellationToken: token);
		if (users is not null)
		{
			Users.Clear();
			Users.AddRange(users);
		}
	}
}
