using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Dawdler
{
	public interface IDailyTask : ITransientDependency
	{
		ValueTask RunAsync(CancellationToken token);
	}
}
