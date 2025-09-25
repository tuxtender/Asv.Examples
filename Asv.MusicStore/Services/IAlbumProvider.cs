using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Asv.MusicStore.Services;

public interface IAlbumProvider
{
	public Task<IEnumerable<MusicAlbum>> SearchAsync(string? searchTerm);
	public Task<Stream> LoadCoverBitmapAsync(string coverUrl, CancellationToken cancelToken);
}