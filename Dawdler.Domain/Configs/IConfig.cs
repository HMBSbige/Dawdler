using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Dawdler.Configs
{
	public interface IConfig : ISingletonDependency
	{
		/// <summary>
		/// 配置文件路径
		/// </summary>
		string FilePath { get; }

		/// <summary>
		/// 备份配置文件路径
		/// </summary>
		string BackupFilePath { get; }

		/// <summary>
		/// 保存配置
		/// </summary>
		ValueTask SaveAsync(CancellationToken token);

		/// <summary>
		/// 加载配置
		/// </summary>
		ValueTask LoadAsync(CancellationToken token);
	}
}
