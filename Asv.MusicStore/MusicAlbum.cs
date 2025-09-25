namespace Asv.MusicStore;

public class MusicAlbum(string artist, string title, string coverUrl)
{
	public string Artist { get; } = artist;
	public string Title { get; } = title;
	public string CoverUrl { get; } = coverUrl;
}