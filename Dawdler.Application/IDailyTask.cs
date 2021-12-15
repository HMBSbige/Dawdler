using Volo.Abp.DependencyInjection;

namespace Dawdler;

public interface IDailyTask : ITransientDependency
{
	ValueTask RunAsync(CancellationToken token);
}
