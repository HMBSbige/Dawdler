using Dawdler.BilibiliUsers;
using Dawdler.Utils;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Dawdler.Configs
{
	[UsedImplicitly]
	public sealed class BilibiliUsersConfig : ConfigBase
	{
		public List<BilibiliUser> Users { get; } = new();

		public BilibiliUsersConfig(ILogger<BilibiliUsersConfig> logger) : base(logger)
		{
		}

		protected override string FilePath { get; } = ConfigPath.BilibiliUsers;

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
}
