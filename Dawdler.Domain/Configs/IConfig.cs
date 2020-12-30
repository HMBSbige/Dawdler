using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Dawdler.Configs
{
	public interface IConfig : ISingletonDependency
	{
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
