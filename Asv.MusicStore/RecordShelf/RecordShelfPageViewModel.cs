using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Cfg;
using Asv.Common;
using Asv.MusicStore.RecordShelf.Album;
using Asv.MusicStore.RecordShelf.MusicStore;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.MusicStore.RecordShelf;

public interface IRecordShelfPageViewModel : IPage;

public sealed class RecordShelfPageViewModelConfig : PageConfig;

[ExportPage(PageId)]
public class RecordShelfPageViewModel
	: PageViewModel<IRecordShelfPageViewModel, RecordShelfPageViewModelConfig>,
		IRecordShelfPageViewModel
{
	private readonly ILoggerFactory _loggerFactory;
	private readonly MusicStoreDialogPrefab _musicStorePrefab;
	public const string PageId = "record_shelf";
	public const MaterialIconKind PageIcon = MaterialIconKind.ViewGallery;

	public RecordShelfPageViewModel()
		: this(
			DesignTime.CommandService,
			DesignTime.ContainerHost,
			DesignTime.Configuration,
			NullDialogService.Instance,
			NullLoggerFactory.Instance
		)
	{
		DesignTime.ThrowIfNotDesignMode();
	}

	[ImportingConstructor]
	public RecordShelfPageViewModel(ICommandService cmd,
		IContainerHost containerHost,
		IConfiguration cfg,
		IDialogService dialogService,
		ILoggerFactory loggerFactory)
		: base(PageId, cmd, cfg, loggerFactory)
	{
		_loggerFactory = loggerFactory;
		_musicStorePrefab = dialogService.GetDialogPrefab<MusicStoreDialogPrefab>();

		
		Title = "RS.ControlsGalleryPageViewModel_Title";
		Icon = PageIcon;

		OpenMusicStoreDialogCommand = new ReactiveCommand(ShowMusicStoreDialogAsync).DisposeItWith(Disposable);
	}

	public ReactiveCommand OpenMusicStoreDialogCommand { get; }
	
	public ObservableCollection<AlbumViewModel> Albums { get; } = new();
	
	public override IExportInfo Source => SystemModule.Instance;

	
	private async ValueTask ShowMusicStoreDialogAsync(Unit unit, CancellationToken cancellationToken)
	{
	
		var album = await _musicStorePrefab.ShowDialogAsync(EmptyDialogPayload.EmptyDialog);
		// var result = rawResult?.ToString() ?? $"({RS.DialogControlsPageViewModel_CancelResult})";
		var msg = "RS.DialogControlsPageViewModel_GeoPoint_Result";
		
		Logger.LogInformation("{msg}", msg);

		if (album is not null)
		{
			Albums.Add(album);
			// await album.SaveToDiskAsync();
		}
	}
	public override IEnumerable<IRoutable> GetRoutableChildren()
	{
		throw new System.NotImplementedException();
	}

	/// <summary>
	/// Loads albums and their covers from cache.
	/// </summary>
	private async void LoadAlbums()
	{
		var albums = (await Album.Album.LoadCachedAsync()).Select(x => new AlbumViewModel(x, _loggerFactory)).ToList();
		foreach (var album in albums)
		{
			Albums.Add(album);
		}
		var coverTasks = albums.Select(album => album.LoadCover());
		await Task.WhenAll(coverTasks);
	}
	
	protected override void AfterLoadExtensions()
	{
	}
}