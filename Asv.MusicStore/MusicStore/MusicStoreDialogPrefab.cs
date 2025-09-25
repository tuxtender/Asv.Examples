using System.Composition;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.MusicStore.Services;
using Microsoft.Extensions.Logging;

namespace Asv.MusicStore.MusicStore;

[ExportDialogPrefab]
[Shared]
[method: ImportingConstructor]
public sealed class MusicStoreDialogPrefab(
	IAlbumProvider albumProvider,
	INavigationService nav,
	ILoggerFactory loggerFactory) : IDialogPrefab<EmptyDialogPayload, MusicAlbum?>
{
	public async Task<MusicAlbum?> ShowDialogAsync(EmptyDialogPayload dialogPayload)
	{
		using var vm = new MusicStoreDialogViewModel(albumProvider, loggerFactory);

		var dialogContent = new ContentDialog(vm, nav)
		{
			PrimaryButtonText = RS.MusicStoreDialogPrefab_DialogButton_BuyAlbum,
			SecondaryButtonText = RS.MusicStoreDialogPrefab_DialogButton_Exit,
			DefaultButton = ContentDialogButton.Primary,
		};

		vm.ApplyDialog(dialogContent);

		var result = await dialogContent.ShowAsync();

		return result is ContentDialogResult.Primary ? vm.GetResult() : null;
	}
}