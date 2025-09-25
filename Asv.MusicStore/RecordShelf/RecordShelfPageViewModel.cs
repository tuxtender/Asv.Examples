using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Cfg;
using Asv.Common;
using Asv.MusicStore.RecordShelf.Album;
using Asv.MusicStore.RecordShelf.MusicStore;
using Avalonia.Media.Imaging;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.MusicStore.RecordShelf;

public interface IRecordShelfPageViewModel : IPage;

public sealed class RecordShelfPageViewModelConfig : PageConfig 
{
	public List<AlbumDto> Albums { get; } = [];
}

[ExportPage(PageId)]
public class RecordShelfPageViewModel
	: PageViewModel<IRecordShelfPageViewModel, RecordShelfPageViewModelConfig>,
		IRecordShelfPageViewModel
{
	private readonly IConfiguration _cfg;
	private readonly ILoggerFactory _loggerFactory;
	private readonly MusicStoreDialogPrefab _musicStorePrefab;
	private readonly ObservableList<AlbumViewModel> _albums = [];
	
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
		_cfg = cfg;
		_loggerFactory = loggerFactory;
		_musicStorePrefab = dialogService.GetDialogPrefab<MusicStoreDialogPrefab>();

		
		Title = "RS.ControlsGalleryPageViewModel_Title";
		Icon = PageIcon;

		_albums.SetRoutableParent(this).DisposeItWith(Disposable);
		_albums.DisposeRemovedItems().DisposeItWith(Disposable);
		Albums = _albums
			.ToNotifyCollectionChangedSlim()
			.DisposeItWith(Disposable);

		OpenMusicStoreDialogCommand = new ReactiveCommand(ShowMusicStoreDialogAsync).DisposeItWith(Disposable);
	}

	public ReactiveCommand OpenMusicStoreDialogCommand { get; }
	
	public NotifyCollectionChangedSynchronizedViewList<AlbumViewModel> Albums { get; }
	
	public override IExportInfo Source => SystemModule.Instance;

	
	private async ValueTask ShowMusicStoreDialogAsync(Unit unit, CancellationToken cancellationToken)
	{
	
		var album = await _musicStorePrefab.ShowDialogAsync(EmptyDialogPayload.EmptyDialog);
		// var result = rawResult?.ToString() ?? $"({RS.DialogControlsPageViewModel_CancelResult})";
		var msg = "RS.DialogControlsPageViewModel_GeoPoint_Result";
		
		Logger.LogInformation("{msg}", msg);

		if (album is not null)
		{
			var albumViewModel = new AlbumViewModel(album, _loggerFactory)
				.SetRoutableParent(this)
				.DisposeItWith(Disposable);
			
			await albumViewModel.LoadCover(CancellationToken.None);

			_albums.Add(albumViewModel);
			using var stream = new MemoryStream();
			albumViewModel.Cover.Value?.Save(stream);
			var imageBytes = stream.ToArray();
			var base64String = Convert.ToBase64String(imageBytes);

			Config.Albums.Add(new AlbumDto(album.Title, album.Artist, album.CoverUrl, base64String));

			_cfg.Set(Config);
		}
	}
	public override IEnumerable<IRoutable> GetRoutableChildren()
	{
		foreach (var album in _albums)
		{
			yield return album;
		}
	}

	protected override void AfterLoadExtensions()
	{
		var config = _cfg.Get<RecordShelfPageViewModelConfig>();

		foreach (var albumDto in config.Albums)
		{
			var album = new Album.Album(albumDto.Artist, albumDto.Title, albumDto.CoverUrl);

			var vm = new AlbumViewModel(album, _loggerFactory)
				.SetRoutableParent(this)
				.DisposeItWith(Disposable);
			
			var imageBytes = Convert.FromBase64String(albumDto.Image);
			using var ms = new MemoryStream(imageBytes);
			vm.Cover.Value = new Bitmap(ms);

			_albums.Add(vm);
		}
	}
}