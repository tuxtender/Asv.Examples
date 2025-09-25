using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.MusicStore.Album;

[ExportViewFor(typeof(AlbumViewModel))]
public partial class AlbumView : UserControl
{
	public AlbumView()
	{
		InitializeComponent();
	}
}