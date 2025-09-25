using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.MusicStore.RecordShelf.Album;

public class AlbumViewModel : RoutableViewModel
{
	private readonly Album _album;

	
	public AlbumViewModel(): base(NavigationId.Empty, NullLoggerFactory.Instance)
	{
		DesignTime.ThrowIfNotDesignMode();
		_album = new Album("Artist", "Title", string.Empty);
		Cover = new BindableReactiveProperty<Bitmap?>();
	}
	
	public AlbumViewModel(Album album, ILoggerFactory loggerFactory)
		: base(
			NavigationId.GenerateByHash(album.Artist, album.Title, album.CoverUrl),
			loggerFactory)
	{
		_album = album;
		Cover = new BindableReactiveProperty<Bitmap?>();
	}

	public string Artist => _album.Artist;

	public string Title => _album.Title;
	
	public Album Album => _album;

	public BindableReactiveProperty<Bitmap?> Cover { get; }

	/// <summary>
	/// Asynchronously loads and decodes the album cover image, then assigns it to <see cref="Cover"/>.
	/// </summary>
	public async Task LoadCover(CancellationToken cancelToken)
	{
		await using var imageStream = await _album.LoadCoverBitmapAsync(cancelToken);
		Cover.Value = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400), cancelToken);
	}

	/// <summary>
	/// Saves the album and its cover to cache.
	/// </summary>        
	public async Task SaveToDiskAsync()
	{
		await _album.SaveAsync();

		if (Cover.Value != null)
		{
			var bitmap = Cover.Value;

			await Task.Run(() =>
			{
				using (var fs = _album.SaveCoverBitmapStream())
				{
					bitmap.Save(fs);
				}
			});
		}
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