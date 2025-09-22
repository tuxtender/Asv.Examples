using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.MusicStore.RecordShelf.Album;

public class AlbumViewModel : DisposableViewModel
{
	private readonly Album _album;

	public AlbumViewModel(Album album, ILoggerFactory loggerFactory)
		: base(NavigationId.Empty, loggerFactory)
	{
		_album = album;

		Cover = new BindableReactiveProperty<Bitmap?>().DisposeItWith(Disposable);
	}

	public string Artist => _album.Artist;

	public string Title => _album.Title;

	public BindableReactiveProperty<Bitmap?> Cover { get; }

	/// <summary>
	/// Asynchronously loads and decodes the album cover image, then assigns it to <see cref="Cover"/>.
	/// </summary>
	public async Task LoadCover()
	{
		await using (var imageStream = await _album.LoadCoverBitmapAsync())
		{
			Cover.Value = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
		}
	}

	/// <summary>
	/// Saves the album and its cover to cache.
	/// </summary>        
	public async Task SaveToDiskAsync()
	{
		await _album.SaveAsync();

		if (Cover != null)
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
}