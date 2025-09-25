using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.MusicStore.Services;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.MusicStore.Album;

public class AlbumViewModel : RoutableViewModel
{
	private readonly IAlbumProvider _albumProvider;

	public AlbumViewModel()
		: base(NavigationId.Empty, NullLoggerFactory.Instance)
	{
		DesignTime.ThrowIfNotDesignMode();
		Album = new MusicAlbum("Artist", "Title", string.Empty);
		Cover = new BindableReactiveProperty<Bitmap?>();
	}

	public AlbumViewModel(MusicAlbum album, IAlbumProvider albumProvider, ILoggerFactory loggerFactory)
		: base(NavigationId.GenerateByHash(album.Artist, album.Title, album.CoverUrl),
			loggerFactory)
	{
		_albumProvider = albumProvider;
		Album = album;
		Cover = new BindableReactiveProperty<Bitmap?>();
	}

	public string Artist => Album.Artist;

	public string Title => Album.Title;

	public MusicAlbum Album { get; }

	public BindableReactiveProperty<Bitmap?> Cover { get; }

	/// <summary>
	/// Asynchronously loads and decodes the album cover image, then assigns it to <see cref="Cover"/>.
	/// </summary>
	public async Task LoadCoverAsync(CancellationToken cancellationToken)
	{
		await using var imageStream = await _albumProvider.LoadCoverBitmapAsync(Album.CoverUrl, cancellationToken);
		Cover.Value = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400), cancellationToken);
	}

	public override IEnumerable<IRoutable> GetRoutableChildren() => [];

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Cover.Value?.Dispose();
		}

		base.Dispose(disposing);
	}
}