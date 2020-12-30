using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Dawdler
{
	[DependsOn(
		typeof(AbpAutofacModule)
	)]
	public class DawdlerModule : AbpModule
	{
		public override void ConfigureServices(ServiceConfigurationContext context)
		{
			context.Services.AddHostedService<DawdlerHostedService>();
		}
	}
}
