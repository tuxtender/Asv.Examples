using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.MusicStore.RecordShelf.MusicStore;

[ExportViewFor(typeof(MusicStoreDialogViewModel))]

public partial class MusicStoreDialogView : UserControl
{
	public MusicStoreDialogView()
	{
		InitializeComponent();
	}
}