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

		/// <summary>
		/// 备份配置文件路径
		/// </summary>
		public abstract string BackupFilePath { get; }

		private readonly AsyncReaderWriterLock _lock = new(null);
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

		private async ValueTask EnsureConfigFileExistsAsync()
		{
			var dir = Path.GetDirectoryName(FilePath);
			if (!Directory.Exists(dir) && dir is not null)
			{
				Directory.CreateDirectory(dir);
			}

			if (!File.Exists(FilePath))
			{
				await File.Create(FilePath).DisposeAsync();
			}
		}

		public async ValueTask SaveAsync(CancellationToken token)
		{
			try
			{
				await using var _ = await _lock.WriteLockAsync(token);

				var tempFile = Path.ChangeExtension(@"TMP" + Path.GetRandomFileName(), Path.GetExtension(FilePath));

				await using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
				{
					await SaveToStreamAsync(fs, JsonOptions, token);
				}

				await EnsureConfigFileExistsAsync();

				File.Replace(tempFile, FilePath, BackupFilePath);
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

				if (await LoadAsync(FilePath, token))
				{
					return;
				}

				_logger.LogInformation($@"尝试加载备份文件 {BackupFilePath}");
				await LoadAsync(BackupFilePath, token);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $@"加载 {FilePath} 错误！");
			}
		}

		private async ValueTask<bool> LoadAsync(string filename, CancellationToken token)
		{
			try
			{
				await using var fs = File.OpenRead(filename);

				await LoadFromStreamAsync(fs, token);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $@"加载 {filename} 错误！");

				return false;
			}
		}
	}
}
