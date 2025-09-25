using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Cfg;
using Asv.Common;
using Asv.MusicStore.Album;
using Asv.MusicStore.MusicStore;
using Asv.MusicStore.Services;
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
	public List<MusicAlbumDto> Albums { get; } = [];
}

[ExportPage(PageId)]
public class RecordShelfPageViewModel
	: PageViewModel<IRecordShelfPageViewModel, RecordShelfPageViewModelConfig>,
		IRecordShelfPageViewModel
{
	private readonly IAlbumProvider _albumProvider;
	private readonly IConfiguration _cfg;
	private readonly ILoggerFactory _loggerFactory;
	private readonly MusicStoreDialogPrefab _musicStorePrefab;
	private readonly ObservableList<AlbumViewModel> _albums = [];

	public const string PageId = "record_shelf";
	public const MaterialIconKind PageIcon = MaterialIconKind.ViewGallery;

	public RecordShelfPageViewModel()
		: this(
			null,
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
	public RecordShelfPageViewModel(IAlbumProvider albumProvider,
		ICommandService cmd,
		IContainerHost containerHost,
		IConfiguration cfg,
		IDialogService dialogService,
		ILoggerFactory loggerFactory)
		: base(PageId, cmd, cfg, loggerFactory)
	{
		_albumProvider = albumProvider;
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
		LoadAlbums(config.Albums);
	}

	private void LoadAlbums(IEnumerable<MusicAlbumDto> albums)
	{
		foreach (var albumDto in albums)
		{
			var album = new MusicAlbum(albumDto.Artist, albumDto.Title, albumDto.CoverUrl);

			var vm = new AlbumViewModel(album, _albumProvider, _loggerFactory)
				.SetRoutableParent(this)
				.DisposeItWith(Disposable);

			var imageBytes = Convert.FromBase64String(albumDto.Image);
			using var ms = new MemoryStream(imageBytes);
			vm.Cover.Value = new Bitmap(ms);

			_albums.Add(vm);
		}
	}

	private async ValueTask ShowMusicStoreDialogAsync(Unit unit, CancellationToken cancellationToken)
	{
		var album = await _musicStorePrefab.ShowDialogAsync(EmptyDialogPayload.EmptyDialog);

		if (album == null)
			return;

		var albumViewModel = new AlbumViewModel(album, _albumProvider, _loggerFactory)
			.SetRoutableParent(this)
			.DisposeItWith(Disposable);

		await albumViewModel.LoadCoverAsync(CancellationToken.None);
		_albums.Add(albumViewModel);

		SaveAlbum(album, albumViewModel.Cover.Value);
	}

	private void SaveAlbum(MusicAlbum album, Bitmap? albumImage)
	{
		using var stream = new MemoryStream();
		albumImage?.Save(stream);
		var imageBytes = stream.ToArray();
		var base64String = Convert.ToBase64String(imageBytes);

		var musicAlbumDto = new MusicAlbumDto(album.Title, album.Artist, album.CoverUrl, base64String);
		Config.Albums.Add(musicAlbumDto);

		_cfg.Set(Config);
	}
}