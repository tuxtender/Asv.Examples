using System;
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
	private readonly ReactiveProperty<string?> _searchText;

	public const string DialogId = $"{BaseId}.musicstore";
	
	private CancellationTokenSource _cancellationTokenSource;


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

		_cancellationTokenSource = new CancellationTokenSource().DisposeItWith(Disposable);

		_searchText = new ReactiveProperty<string?>().DisposeItWith(Disposable);
		_searchText.ObserveOnCurrentSynchronizationContext().Subscribe(OnSearchTextChanged);

		SearchText = new HistoricalStringProperty(
			nameof(SearchText),
			_searchText,
			loggerFactory,
			this
		).DisposeItWith(Disposable);
	}

	public override IEnumerable<IRoutable> GetRoutableChildren()
	{
		yield return SearchText;
		
		foreach (var albumViewModel in _searchResults)
		{
			yield return albumViewModel;
		}
	}

	public BindableReactiveProperty<bool> IsBusy { get; }


	public BindableReactiveProperty<AlbumViewModel?> SelectedAlbum { get; }

	public INotifyCollectionChangedSynchronizedViewList<AlbumViewModel> SearchResults { get; }
	
	public HistoricalStringProperty SearchText { get; }

	

	public Album.Album? GetResult()
	{
		return SelectedAlbum.Value?.Album;
	}

	// protected override void Dispose(bool disposing)
	// {
	// 	if (disposing)
	// 	{
	// 		_cancellationTokenSource?.Cancel();
	// 		_cancellationTokenSource?.Dispose();
	// 	}
	//
	// 	base.Dispose(disposing);
	// }
	
	/// <summary>
	/// Performs an asynchronous search for albums based on the provided term and updates the results.
	/// </summary>
	private async Task DoSearch(string? term)
	{
		await _cancellationTokenSource.CancelAsync();
		_cancellationTokenSource.Dispose();
		_searchResults.ClearWithItemsDispose();
		_cancellationTokenSource = new CancellationTokenSource();
		var cancellationToken = _cancellationTokenSource.Token;
		
		IsBusy.Value = true;
		// _searchResults.Clear();
		
		var albums = await Album.Album.SearchAsync(term);
		
		foreach (var album in albums)
		{
			var vm = new AlbumViewModel(album, _loggerFactory)
				.SetRoutableParent(this)
				.DisposeItWith(Disposable);

			_searchResults.Add(vm);
		}

		IsBusy.Value = false;

		await LoadCovers(cancellationToken);
	}

	/// <summary>
	/// Asynchronously loads album cover images for each result, unless the operation is canceled.
	/// </summary>
	private async Task LoadCovers(CancellationToken cancellationToken)
	{
		try
		{
			foreach (var album in _searchResults)
			{
				await album.LoadCover(cancellationToken);

				// if (cancellationToken.IsCancellationRequested)
				// {
				// 	return;
				// }
			}
		}
		catch (OperationCanceledException ex)
		{
			Logger.LogInformation("Loading covers aborted");
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