using System.Composition;
using Asv.Avalonia;

namespace Asv.MusicStore.RecordShelf.Commands;

[ExportCommand]
[method: ImportingConstructor]
public class OpenRecordShelfPageCommand(INavigationService nav)
	: OpenPageCommandBase(RecordShelfPageViewModel.PageId, nav)
{
	public override ICommandInfo Info => StaticInfo;

	#region Static

	public const string Id = $"{BaseId}.open.{RecordShelfPageViewModel.PageId}";

	public static readonly ICommandInfo StaticInfo = new CommandInfo
	{
		Id = Id,
		Name = "RS.OpenControlsGalleryPageCommand_CommandInfo_Name",
		Description = "RS.OpenControlsGalleryPageCommand_CommandInfo_Description",
		Icon = RecordShelfPageViewModel.PageIcon,
		DefaultHotKey = null,
		Source = SystemModule.Instance,
	};

	#endregion
}