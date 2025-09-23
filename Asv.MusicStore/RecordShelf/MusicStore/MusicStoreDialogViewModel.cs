using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Asv.MusicStore.RecordShelf.Album;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.MusicStore.RecordShelf.MusicStore;

public class MusicStoreDialogViewModel : DialogViewModelBase
{
	private readonly ObservableList<AlbumViewModel> _searchResults = [];
	private readonly ILoggerFactory _loggerFactory;
	public const string DialogId = $"{BaseId}.musicstore";
	
	private CancellationTokenSource? _cancellationTokenSource;


	public MusicStoreDialogViewModel()
		: this(NullLoggerFactory.Instance, NullUnitService.Instance)
	{
		DesignTime.ThrowIfNotDesignMode();
	}

	[ImportingConstructor]
	public MusicStoreDialogViewModel(ILoggerFactory loggerFactory, IUnitService unitService)
		: base(DialogId, loggerFactory) {
		_loggerFactory = loggerFactory;

		_searchResults.SetRoutableParent(this).DisposeItWith(Disposable);
		_searchResults.DisposeRemovedItems().DisposeItWith(Disposable);
		SearchResults = _searchResults
			.ToNotifyCollectionChangedSlim(SynchronizationContextCollectionEventDispatcher.Current)
			.DisposeItWith(Disposable);

		IsBusy = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
		
		SelectedAlbum = new BindableReactiveProperty<AlbumViewModel?>().DisposeItWith(Disposable);
		
		SearchText = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
		SearchText.Subscribe(OnSearchTextChanged);

	}

	public override IEnumerable<IRoutable> GetRoutableChildren()
	{
		foreach (var albumViewModel in _searchResults)
		{
			yield return albumViewModel;
		}
	}

	public BindableReactiveProperty<string?> SearchText { get; }


	
	public BindableReactiveProperty<bool> IsBusy { get; }


	public BindableReactiveProperty<AlbumViewModel?> SelectedAlbum { get; }

	public INotifyCollectionChangedSynchronizedViewList<AlbumViewModel> SearchResults { get; }

	public AlbumViewModel? GetResult()
	{
		return SelectedAlbum.Value;
	}

	/// <summary>
	/// Performs an asynchronous search for albums based on the provided term and updates the results.
	/// </summary>
	private async Task DoSearch(string? term)
	{
		_cancellationTokenSource?.Cancel();
		_cancellationTokenSource = new CancellationTokenSource();
		var cancellationToken = _cancellationTokenSource.Token;
		
		IsBusy.Value = true;
		_searchResults.Clear();
		
		var albums = await Album.Album.SearchAsync(term);
		
		foreach (var album in albums)
		{
			var vm = new AlbumViewModel(album, _loggerFactory);
			_searchResults.Add(vm);
		}
		
		if (!cancellationToken.IsCancellationRequested)
		{
			LoadCovers(cancellationToken);
		}
		
		IsBusy.Value = false;
		
		
	}

	/// <summary>
	/// Asynchronously loads album cover images for each result, unless the operation is canceled.
	/// </summary>
	private async void LoadCovers(CancellationToken cancellationToken)
	{
		foreach (var album in _searchResults)
		{
			await album.LoadCover();
		
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
		}
	}

	/// <summary>
	/// Triggered when the search text in music store view changes and initiates a new search operation.
	/// </summary>
	private void OnSearchTextChanged(string? value)
	{
		_ = DoSearch(value);
	}
}