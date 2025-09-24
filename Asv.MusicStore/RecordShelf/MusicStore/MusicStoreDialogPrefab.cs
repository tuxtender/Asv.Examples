using System.Composition;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.MusicStore.RecordShelf.Album;
using Microsoft.Extensions.Logging;

namespace Asv.MusicStore.RecordShelf.MusicStore;

[ExportDialogPrefab]
[Shared]
[method: ImportingConstructor]
public sealed class MusicStoreDialogPrefab (
	INavigationService nav,
	ILoggerFactory loggerFactory,
	IUnitService unitService
) : IDialogPrefab<EmptyDialogPayload, Album.Album?>
{
	public async Task<Album.Album?> ShowDialogAsync(EmptyDialogPayload dialogPayload)
	{
		using var vm = new MusicStoreDialogViewModel(loggerFactory, unitService);

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