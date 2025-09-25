using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.MusicStore.MusicStore;

[ExportViewFor(typeof(MusicStoreDialogViewModel))]

public partial class MusicStoreDialogView : UserControl
{
	public MusicStoreDialogView()
	{
		InitializeComponent();
	}
}