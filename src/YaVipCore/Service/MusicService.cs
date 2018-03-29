using YaVipCore.Api.Music;
using YaVipCore.Interface;

namespace YaVipCore.Service
{
    public class MusicService
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
    }
}