using Dawdler.Bilibili;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dawdler.Configs;

[UsedImplicitly]
public sealed class BilibiliUsersConfig : ConfigBase
{
	public List<BilibiliUser> Users { get; } = new();

	public BilibiliUsersConfig(ILogger<BilibiliUsersConfig> logger) : base(logger)
	{
	}

	public override string FilePath => ConfigPath.BilibiliUsers;

	public override string BackupFilePath => ConfigPath.BilibiliUsersBackup;

	protected override async ValueTask SaveToStreamAsync(Stream fs, JsonSerializerOptions options, CancellationToken token)
	{
		await JsonSerializer.SerializeAsync(fs, Users, options, token);
	}

	protected override async ValueTask LoadFromStreamAsync(Stream fs, CancellationToken token)
	{
		var users = await JsonSerializer.DeserializeAsync<List<BilibiliUser>>(fs, cancellationToken: token);
		if (users is not null)
		{
			Users.Clear();
			Users.AddRange(users);
		}
	}
}
