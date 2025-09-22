using System.Collections.Generic;
using System.Composition;
using Asv.Avalonia;
using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.MusicStore.RecordShelf;

public interface IRecordShelfPageViewModel : IPage;

public sealed class RecordShelfPageViewModelConfig : PageConfig;

[ExportPage(PageId)]
public class RecordShelfPageViewModel
	: PageViewModel<IRecordShelfPageViewModel, RecordShelfPageViewModelConfig>,
		IRecordShelfPageViewModel
{
	public const string PageId = "record_shelf";
	public const MaterialIconKind PageIcon = MaterialIconKind.ViewGallery;

	public RecordShelfPageViewModel()
		: this(
			DesignTime.CommandService,
			DesignTime.ContainerHost,
			DesignTime.Configuration,
			NullLoggerFactory.Instance
		)
	{
		DesignTime.ThrowIfNotDesignMode();
	}

	[ImportingConstructor]
	public RecordShelfPageViewModel(ICommandService cmd,
		IContainerHost containerHost,
		IConfiguration cfg,
		ILoggerFactory loggerFactory)
		: base(PageId, cmd, cfg, loggerFactory)
	{
		Title = "RS.ControlsGalleryPageViewModel_Title";
		Icon = PageIcon;
	}

	public override IExportInfo Source => SystemModule.Instance;

	public override IEnumerable<IRoutable> GetRoutableChildren()
	{
		throw new System.NotImplementedException();
	}

	protected override void AfterLoadExtensions()
	{
		throw new System.NotImplementedException();
	}
}