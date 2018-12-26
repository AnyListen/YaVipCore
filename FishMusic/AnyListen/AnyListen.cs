using System;
using AnyListen.Music;

namespace AnyListen
{
    public class AnyListen
    {
        public static IMusic GetMusic(string type)
        {
            switch (type)
            {
                case "wy":
                    return new WyMusic();
                case "xm":
                    return new XiamiMusic();
                case "tt":
                    return new TtMusic();
                case "qq":
                    return new TxMusic();
                case "bd":
                    return new BdMusic();
                case "kw":
                    return new KwMusic();
                case "kg":
                    return new KgMusic();
                case "sn":
                    return new SonyMusic();
                default:
                    return null;
            }
        }

        public static string GetRealUrl(string url)
        {
            if (url.StartsWith("http"))
            {
                return url;
            }
            var arr = url.Split(new[] {'_', '.'}, StringSplitOptions.RemoveEmptyEntries);
            return GetMusic(arr[0]).GetSongUrl(arr[2], arr[1], arr[3]);
        }
    }
}