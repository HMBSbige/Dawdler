using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Dawdler.Configs
{
	public abstract class ConfigBase : IConfig
	{
		private readonly ILogger _logger;

		/// <summary>
		/// 配置文件路径
		/// </summary>
		public abstract string FilePath { get; }

		private readonly AsyncReaderWriterLock _lock = new();
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.Default,
			IgnoreReadOnlyProperties = true
		};

		protected ConfigBase(ILogger<ConfigBase> logger)
		{
			_logger = logger;
		}

		protected abstract ValueTask SaveToStreamAsync(Stream fs, JsonSerializerOptions options, CancellationToken token);
		protected abstract ValueTask LoadFromStreamAsync(Stream fs, CancellationToken token);

		private void EnsureDir()
		{
			var dir = Path.GetDirectoryName(FilePath);
			if (!Directory.Exists(dir) && dir is not null)
			{
				Directory.CreateDirectory(dir);
			}
		}

		public async ValueTask SaveAsync(CancellationToken token)
		{
			try
			{
				await using var _ = await _lock.WriteLockAsync(token);

				EnsureDir();
				await using var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

				await SaveToStreamAsync(fs, JsonOptions, token);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $@"保存 {FilePath} 错误！");
			}
		}

		public async ValueTask LoadAsync(CancellationToken token)
		{
			try
			{
				await using var _ = await _lock.ReadLockAsync(token);

				await using var fs = File.OpenRead(FilePath);

				await LoadFromStreamAsync(fs, token);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $@"加载 {FilePath} 错误！");
			}
		}
	}
}
