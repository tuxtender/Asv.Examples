using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.MusicStore.RecordShelf;

[ExportViewFor(typeof(RecordShelfPageViewModel))]
public partial class RecordShelfPageView : UserControl
{
	public RecordShelfPageView()
	{
		InitializeComponent();
	}
}