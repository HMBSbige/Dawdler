using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Dawdler
{
	[DependsOn(
		typeof(AbpAutofacModule),
		typeof(DawdlerApplicationModule)
	)]
	[UsedImplicitly]
	public class DawdlerModule : AbpModule
	{
		public override void ConfigureServices(ServiceConfigurationContext context)
		{
			context.Services.ConfigureHttpClient();
			context.Services.AddHostedService<DawdlerHostedService>();
		}
	}
}
