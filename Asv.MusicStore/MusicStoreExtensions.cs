using System.Net.Http;
using Asv.MusicStore.Services;
using iTunesSearch.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.MusicStore;

public static class MusicStoreExtensions
{
	public static IHostApplicationBuilder UseMusicStoreServices(this IHostApplicationBuilder builder)
	{
		var itunesAlbumProvider = new ItunesAlbumProvider(new iTunesSearchManager(), new HttpClient());
		builder.Services.AddSingleton<IAlbumProvider>(itunesAlbumProvider);
		return builder;
	}
}