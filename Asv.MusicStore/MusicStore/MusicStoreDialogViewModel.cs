using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Asv.MusicStore.Album;
using Asv.MusicStore.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.MusicStore.MusicStore;

public class MusicStoreDialogViewModel : DialogViewModelBase
{
	private readonly ObservableList<AlbumViewModel> _searchResults = [];
	private readonly IAlbumProvider _albumProvider;
	private readonly ILoggerFactory _loggerFactory;
	private CancellationTokenSource _cancellationTokenSource;

	public const string DialogId = $"{BaseId}.music_store";

	public MusicStoreDialogViewModel()
		: base(NavigationId.GenerateRandom(), NullLoggerFactory.Instance)
	{
		DesignTime.ThrowIfNotDesignMode();
	}

	[ImportingConstructor]
	public MusicStoreDialogViewModel(IAlbumProvider albumProvider, ILoggerFactory loggerFactory)
		: base(DialogId, loggerFactory)
	{
		_albumProvider = albumProvider;
		_loggerFactory = loggerFactory;
		_cancellationTokenSource = new CancellationTokenSource().DisposeItWith(Disposable);

		_searchResults.SetRoutableParent(this).DisposeItWith(Disposable);
		_searchResults.DisposeRemovedItems().DisposeItWith(Disposable);
		SearchResults = _searchResults
			.ToNotifyCollectionChangedSlim(SynchronizationContextCollectionEventDispatcher.Current)
			.DisposeItWith(Disposable);

		IsBusy = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
		SelectedAlbum = new BindableReactiveProperty<AlbumViewModel?>().DisposeItWith(Disposable);

		var searchText = new ReactiveProperty<string?>().DisposeItWith(Disposable);
		searchText.ObserveOnCurrentSynchronizationContext().Subscribe(x => _ = DoSearch(x));
		SearchText = new HistoricalStringProperty(
			nameof(SearchText),
			searchText,
			loggerFactory,
			this
		).DisposeItWith(Disposable);
	}

	public BindableReactiveProperty<bool> IsBusy { get; }
	public BindableReactiveProperty<AlbumViewModel?> SelectedAlbum { get; }
	public INotifyCollectionChangedSynchronizedViewList<AlbumViewModel> SearchResults { get; }
	public HistoricalStringProperty SearchText { get; }

	public MusicAlbum? GetResult()
	{
		return SelectedAlbum.Value?.Album;
	}

	public override IEnumerable<IRoutable> GetRoutableChildren()
	{
		yield return SearchText;

		foreach (var albumViewModel in _searchResults)
		{
			yield return albumViewModel;
		}
	}

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

		var albums = await _albumProvider.SearchAsync(term);

		foreach (var album in albums)
		{
			var vm = new AlbumViewModel(album, _albumProvider, _loggerFactory)
				.SetRoutableParent(this)
				.DisposeItWith(Disposable);

			_searchResults.Add(vm);
		}

		IsBusy.Value = false;

		await LoadCoversAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously loads album cover images for each result, unless the operation is canceled.
	/// </summary>
	private async Task LoadCoversAsync(CancellationToken cancellationToken)
	{
		try
		{
			foreach (var album in _searchResults)
			{
				await album.LoadCoverAsync(cancellationToken);
			}
		}
		catch (OperationCanceledException)
		{
			Logger.LogInformation("Loading covers aborted");
		}
	}
}