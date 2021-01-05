using BilibiliLiveRecordDownLoader.Shared.Utils;
using Dawdler.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;

namespace Dawdler
{
	public static class ServiceExtensions
	{
		public static IServiceCollection ConfigureHttpClient(this IServiceCollection services)
		{
			services.AddHttpClient(HttpClientName.Bilibili, client =>
			{
				client.DefaultRequestVersion = HttpVersion.Version20;
				client.DefaultRequestHeaders.Accept.ParseAdd(@"application/json, text/javascript, */*; q=0.01");
				client.DefaultRequestHeaders.Referrer = new Uri(@"https://live.bilibili.com/");
				client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgents.Chrome);
			}).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { UseCookies = false });
			return services;
		}
	}
}
