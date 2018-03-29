using System.Collections.Generic;
using YaVipCore.Models;

namespace YaVipCore.Interface
{
    public interface IMusic
    {
        List<SongResult> SongSearch(string key, int page, int size);

        List<SongResult> AlbumSearch(string id);

        List<SongResult> ArtistSearch(string id, int page, int size);

        List<SongResult> CollectSearch(string id, int page, int size);

        List<SongResult> GetSingleSong(string id);

        string GetSongUrl(string id, string quality, string format);
    }
}