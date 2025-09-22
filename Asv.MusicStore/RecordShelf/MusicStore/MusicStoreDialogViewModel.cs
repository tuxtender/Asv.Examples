using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.MusicStore.RecordShelf.MusicStore;

public class MusicStoreDialogViewModel : DialogViewModelBase
{
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
	
	
		BuyMusicCommand = new ReactiveCommand(BuyMusic).DisposeItWith(Disposable);

	}

	public override IEnumerable<IRoutable> GetRoutableChildren()
	{
		throw new System.NotImplementedException();
	}

	// [ObservableProperty]
	// public partial string? SearchText { get; set; }
	
	public BindableReactiveProperty<string> SearchText { get; set; }

	// [ObservableProperty]
	// public partial bool IsBusy { get; private set; }
	
	public BindableReactiveProperty<bool> IsBusy { get; private set; }


	// [ObservableProperty]
	// public partial AlbumViewModel? SelectedAlbum { get; set; }
	//
	// public ObservableCollection<AlbumViewModel> SearchResults { get; } = new();

	/// <summary>
	/// This relay command sends a message indicating that the selected album has been purchased, which will notify music store view to close.
	/// </summary>
	private void BuyMusic(Unit unit)
	{
		// if (SelectedAlbum != null)
		// {
		// 	WeakReferenceMessenger.Default.Send(new MusicStoreClosedMessage(SelectedAlbum));
		// }
	}
	
	public ReactiveCommand BuyMusicCommand { get; }

	/// <summary>
	/// Performs an asynchronous search for albums based on the provided term and updates the results.
	/// </summary>
	private async Task DoSearch(string? term)
	{
		// _cancellationTokenSource?.Cancel();
		// _cancellationTokenSource = new CancellationTokenSource();
		// var cancellationToken = _cancellationTokenSource.Token;
		//
		// IsBusy = true;
		// SearchResults.Clear();
		//
		// var albums = await Album.SearchAsync(term);
		//
		// foreach (var album in albums)
		// {
		// 	var vm = new AlbumViewModel(album);
		// 	SearchResults.Add(vm);
		// }
		//
		// if (!cancellationToken.IsCancellationRequested)
		// {
		// 	LoadCovers(cancellationToken);
		// }
		//
		// IsBusy = false;
	}

	/// <summary>
	/// Asynchronously loads album cover images for each result, unless the operation is canceled.
	/// </summary>
	private async void LoadCovers(CancellationToken cancellationToken)
	{
		// foreach (var album in SearchResults.ToList())
		// {
		// 	await album.LoadCover();
		//
		// 	if (cancellationToken.IsCancellationRequested)
		// 	{
		// 		return;
		// 	}
		// }
	}

	/// <summary>
	/// Triggered when the search text in music store view changes and initiates a new search operation.
	/// </summary>
	// partial void OnSearchTextChanged(string? value)
	// {
	// 	_ = DoSearch(SearchText);
	// }

	public Album GetResult()
	{
		throw new System.NotImplementedException();
	}
}