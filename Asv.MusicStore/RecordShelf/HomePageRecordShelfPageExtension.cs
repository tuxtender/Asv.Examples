using System.Composition;
using Asv.Avalonia;
using Asv.Common;
using Asv.MusicStore.RecordShelf.Commands;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.MusicStore.RecordShelf;

[ExportExtensionFor<IHomePage>]
[method: ImportingConstructor]
public class HomePageRecordShelfPageExtension(ILoggerFactory loggerFactory)
    : AsyncDisposableOnce,
        IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenRecordShelfPageCommand
                .StaticInfo.CreateAction(
                    loggerFactory,
                   "RS.OpenControlsGalleryPageCommand_Action_Title",
                    "RS.OpenControlsGalleryPageCommand_Action_Description"
                )
                .DisposeItWith(contextDispose)
        );
    }
}
