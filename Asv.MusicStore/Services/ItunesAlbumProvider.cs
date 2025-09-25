using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using iTunesSearch.Library;

namespace Asv.MusicStore.Services;

public class ItunesAlbumProvider : IAlbumProvider
{
	private readonly iTunesSearchManager _searchManager;
	private readonly HttpClient _httpClient;

	public ItunesAlbumProvider(iTunesSearchManager searchManager, HttpClient httpClient)
	{
		_searchManager = searchManager;
		_httpClient = httpClient;
	}

	public async Task<IEnumerable<MusicAlbum>> SearchAsync(string? searchTerm)
	{
		if (string.IsNullOrWhiteSpace(searchTerm))
		{
			return Enumerable.Empty<MusicAlbum>();
		}

		var query = await _searchManager.GetAlbumsAsync(searchTerm).ConfigureAwait(false);

		return query.Albums.Select(x =>
			new MusicAlbum(x.ArtistName, x.CollectionName,
				x.ArtworkUrl100.Replace("100x100bb", "600x600bb")));
	}

	public async Task<Stream> LoadCoverBitmapAsync(string coverUrl, CancellationToken cancellationToken)
	{
		if (File.Exists(coverUrl + ".bmp"))
		{
			return File.OpenRead(coverUrl + ".bmp");
		}
		else
		{
			var data = await _httpClient.GetByteArrayAsync(coverUrl, cancellationToken).ConfigureAwait(false);
			return new MemoryStream(data);
		}
	}
}