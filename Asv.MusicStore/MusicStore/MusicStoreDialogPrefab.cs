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
			Title = "RS.GeoPointDialogPrefab_Content_Title",
			PrimaryButtonText = "RS.DialogButton_Save", // "Buy Album"
			SecondaryButtonText = "RS.DialogButton_Cancel",
			DefaultButton = ContentDialogButton.Primary,
		};

		vm.ApplyDialog(dialogContent);

		var result = await dialogContent.ShowAsync();

		return result is ContentDialogResult.Primary ? vm.GetResult() : null;
	}
}