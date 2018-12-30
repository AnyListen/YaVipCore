using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AnyListen.Models;
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

        public static List<SongResult> Search(string type, string key, int page, int size)
        {
            var item = "search";
            var id = "";
            if (key.Contains("music.163.com"))
            {
                type = "wy";
                var match = Regex.Match(key, "/(\\w+)\\?id=(\\d+)");
                if (match.Success)
                {
                    item = match.Groups[1].Value;
                    id = match.Groups[2].Value;
                }
            }
            else if (key.Contains("xiami.com"))
            {
                type = "xm";
                var match = Regex.Match(key, "xiami.com/(\\w+)/(\\w+)");
                if (match.Success)
                {
                    item = match.Groups[1].Value;
                    id = match.Groups[2].Value;
                }
            }
            else if (key.Contains("y.qq.com"))
            {
                type = "qq";
                var match = Regex.Match(key, "yqq/(\\w+)/(\\w+)\\.htm");
                if (match.Success)
                {
                    item = match.Groups[1].Value;
                    id = match.Groups[2].Value;
                }
            }
            else if (key.Contains("taihe.com") || key.Contains("baidu.com"))
            {
                type = "bd";
                var match = Regex.Match(key, "com/(\\w+)/(\\w+)");
                if (match.Success)
                {
                    item = match.Groups[1].Value;
                    id = match.Groups[2].Value;
                }
            }
            else if (key.Contains("kugou.com"))
            {
                type = "kg";
                var match = Regex.Match(key, "/(\\w+)/(\\w+)\\.htm");
                if (match.Success)
                {
                    item = match.Groups[1].Value;
                    id = match.Groups[2].Value;
                }
            }
            else if (key.Contains("kuwo.cn"))
            {
                type = "kw";
                var match = Regex.Match(key, "cn/(\\w+)/(\\d+)");
                if (match.Success)
                {
                    item = match.Groups[1].Value;
                    id = match.Groups[2].Value;
                }
                if (key.Contains("cinfo"))
                {
                    item = "playlist";
                    id = Regex.Match(key, @"(?<=cinfo_)\d+").Value;
                }
                if (key.Contains("m.kuwo.cn"))
                {
                    match = Regex.Match(key, "newh5/(\\w+)/[\\s\\S]+?id=(\\d+)");
                    if (match.Success)
                    {
                        item = match.Groups[1].Value;
                        id = match.Groups[2].Value;
                    }
                }
            }
            else if (key.Contains("hi-resmusic"))
            {
                type = "sn";
                var match = Regex.Match(key, "album.html[\\s\\S]*?\\?id=(\\d+)");
                if (match.Success)
                {
                    item = "album";
                    id = match.Groups[1].Value;
                }
            }
            List<SongResult> resultList = new List<SongResult>();
            var music = GetMusic(type);
            switch (item)
            {
                case "album":
                    resultList = music.AlbumSearch(id);
                    break;
                case "song":
                    resultList = music.GetSingleSong(id);
                    break;
                case "artist":
                case "singer":
                    resultList = music.ArtistSearch(id, page, size);
                    break;
                case "playlist":
                case "songlist":
                case "playsquare":
                case "single":
                case "collect":
                    resultList = music.CollectSearch(id, page, size);
                    break;
                default:
                    resultList = music.SongSearch(key, page, size);
                    break;
            }
            return resultList;
        }
    }
}