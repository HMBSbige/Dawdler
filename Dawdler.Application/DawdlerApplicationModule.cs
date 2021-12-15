using Volo.Abp.Modularity;

namespace Dawdler;

[DependsOn(typeof(DawdlerDomainModule))]
public class DawdlerApplicationModule : AbpModule
{
}
