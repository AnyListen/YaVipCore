using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using AnyListen.Helper;
using AnyListen.Models;
using Newtonsoft.Json.Linq;

namespace AnyListen.Music
{
    public class KwMusic : IMusic
    {
        public static List<SongResult> Search(string key, int page, int size)
        {
            var url = "http://search.kuwo.cn/r.s?client=kt&all=" + key + "&pn=" + (page - 1) + "&rn="
                + size + "&ft=music&plat=pc&cluster=1&result=json&rformat=json&ver=mbox&show_copyright_off=1&vipver=MUSIC_8.1.2.0_W4&encoding=utf8";
            //var url = $"http://search2013.kuwo.cn/r.s?all={key}&ft=music&client=kt&cluster=0&pn={page-1}&rn={size}&rformat=json&encoding=utf8";
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                if (json["TOTAL"].ToString() == "0")
                {
                    return null;
                }
                var datas = json["abslist"];
                return GetListByJson(datas);
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static List<SongResult> GetListByJson(JToken datas)
        {
            var list = new List<SongResult>();
            foreach (JToken j in datas)
            {
                var song = new SongResult
                {
                    SongId = j["MUSICRID"].ToString().Replace("MUSIC_", ""),
                    SongName = j["SONGNAME"].ToString().HtmlDecode(),
                    SongSubName = j["ALIAS"]?.ToString().HtmlDecode() ?? "",
                    SongLink = "",

                    ArtistId = j["ARTISTID"]?.ToString() ?? "",
                    ArtistName = (j["ARTIST"]?.ToString() ?? "").HtmlDecode().Replace("&", ";").Replace("+", ";"),
                    ArtistSubName = "",

                    AlbumId = j["ALBUMID"]?.ToString() ?? "0",
                    AlbumName = j["ALBUM"]?.ToString().HtmlDecode() ?? "",
                    AlbumSubName = "",
                    AlbumArtist = (j["ARTIST"]?.ToString() ?? "").HtmlDecode().Replace("&", ";").Replace("+", ";"),

                    Length = Convert.ToInt32(j["DURATION"]?.ToString() ?? "0"),
                    BitRate = "128K",

                    FlacUrl = "",
                    ApeUrl = "",
                    WavUrl = "",
                    SqUrl = "",
                    HqUrl = "",
                    LqUrl = "",
                    CopyUrl = "",

                    PicUrl = "",
                    LrcUrl = "",
                    TrcUrl = "",
                    KrcUrl = "",

                    MvId = "",
                    MvHdUrl = "",
                    MvLdUrl = "",

                    Language = j["LANGUAGE"]?.ToString(),
                    Company = j["COMPANY"]?.ToString(),
                    Year = j["RELEASEDATE"]?.ToString(),
                    Disc = 1,
                    TrackNum = 0,
                    Type = "kw"
                };
                if (string.IsNullOrEmpty(song.Year))
                {
                    try
                    {
                        song.Year = j["TIMESTAMP"].ToString().Substring(0, 10);
                    }
                    catch (Exception)
                    {
                        //
                    }
                }
                song.SmallPic = CommonHelper.GetSongUrl("kw", "low", song.SongId, "jpg");
                song.PicUrl = CommonHelper.GetSongUrl("kw", "high", song.SongId, "jpg");
                song.LrcUrl = CommonHelper.GetSongUrl("kw", "320", song.SongId, "lrc");
                var format = j["FORMATS"]?.ToString() ?? j["formats"].ToString();
                if (format.Contains("MP3128"))
                {
                    song.BitRate = "128K";
                    song.CopyUrl = song.LqUrl = CommonHelper.GetSongUrl("kw", "128", song.SongId, "mp3");
                }
                if (format.Contains("MP3192"))
                {
                    song.BitRate = "192K";
                    song.CopyUrl = song.HqUrl = CommonHelper.GetSongUrl("kw", "192", song.SongId, "mp3");
                }
                if (format.Contains("MP3H"))
                {
                    song.BitRate = "320K";
                    song.CopyUrl = song.SqUrl = CommonHelper.GetSongUrl("kw", "320", song.SongId, "mp3");
                }
                if (format.Contains("AL"))
                {
                    song.BitRate = "无损";
                    song.ApeUrl = CommonHelper.GetSongUrl("kw", "1000", song.SongId, "ape");
                }
                if (format.Contains("MP4"))
                {
                    song.MvHdUrl = song.MvLdUrl = CommonHelper.GetSongUrl("kw", "1000", song.SongId, "mp4");
                }
                if (format.Contains("MV"))
                {
                    song.MvLdUrl = CommonHelper.GetSongUrl("kw", "1000", song.SongId, "mkv");
                }

                list.Add(song);
            }
            return list;
        }

        private static List<SongResult> SearchAlbum(string id)
        {
            var url = "http://search.kuwo.cn/r.s?stype=albuminfo&albumid=" + id + "&client=kt&plat=pc&cluster=1&ver=mbox&show_copyright_off=1&vipver=MUSIC_8.1.2.0_W4&encoding=utf8";
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html) || html == "{}")
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                var al = json["name"].ToString();
                var ar = json["artist"].ToString().HtmlDecode();
                var lu = json["lang"].ToString();
                var smallPic = "http://img3.kuwo.cn/star/albumcover/" + json["pic"];
                var pic = "http://img3.kuwo.cn/star/albumcover/" + json["pic"].ToString().Replace("120/", "500/");
                var year = json["pub"].ToString();
                var cmp = json["company"].ToString();
                var list = GetSongsByToken(json["musiclist"]);

                for (var i = 0; i < list.Count; i++)
                {
                    list[i].AlbumId = id;
                    list[i].AlbumName = al;
                    list[i].AlbumArtist = ar;
                    list[i].Language = lu;
                    list[i].PicUrl = pic;
                    list[i].SmallPic = smallPic;
                    list[i].TrackNum = i + 1;
                    list[i].Year = year;
                    list[i].Company = cmp;

                }
                return list;
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static List<SongResult> GetSongsByToken(JToken datas)
        {
            var list = new List<SongResult>();
            foreach (JToken j in datas)
            {
                var song = new SongResult
                {
                    SongId = j["id"].ToString(),
                    SongName = j["name"].ToString().HtmlDecode(),
                    SongSubName = "",
                    SongLink = "",

                    ArtistId = j["artistid"]?.ToString() ?? "",
                    ArtistName = (j["artist"]?.ToString() ?? "").HtmlDecode().Replace("&", ";").Replace("+", ";"),
                    ArtistSubName = "",

                    AlbumId = j["albumid"]?.ToString() ?? "",
                    AlbumName = j["album"]?.ToString().HtmlDecode() ?? "",
                    AlbumSubName = "",
                    AlbumArtist = (j["artist"]?.ToString() ?? "").HtmlDecode().Replace("&", ";").Replace("+", ";"),

                    Length = Convert.ToInt32(j["duration"]?.ToString() ?? "0"),
                    BitRate = "128K",

                    FlacUrl = "",
                    ApeUrl = "",
                    WavUrl = "",
                    SqUrl = "",
                    HqUrl = "",
                    LqUrl = "",
                    CopyUrl = "",

                    PicUrl = "",
                    LrcUrl = "",
                    TrcUrl = "",
                    KrcUrl = "",

                    MvId = "",
                    MvHdUrl = "",
                    MvLdUrl = "",

                    Language = "",
                    Company = "",
                    Year = "",
                    Disc = 1,
                    TrackNum = 0,
                    Type = "kw"
                };
                song.SmallPic = CommonHelper.GetSongUrl("kw", "low", song.SongId, "jpg");
                song.PicUrl = CommonHelper.GetSongUrl("kw", "high", song.SongId, "jpg");
                song.LrcUrl = CommonHelper.GetSongUrl("kw", "320", song.SongId, "lrc");
                var format = j["FORMATS"]?.ToString() ?? j["formats"].ToString();
                if (format.Contains("MP3128"))
                {
                    song.BitRate = "128K";
                    song.CopyUrl = song.LqUrl = CommonHelper.GetSongUrl("kw", "128", song.SongId, "mp3");
                }
                if (format.Contains("MP3192"))
                {
                    song.BitRate = "192K";
                    song.CopyUrl = song.HqUrl = CommonHelper.GetSongUrl("kw", "192", song.SongId, "mp3");
                }
                if (format.Contains("MP3H"))
                {
                    song.BitRate = "320K";
                    song.CopyUrl = song.SqUrl = CommonHelper.GetSongUrl("kw", "320", song.SongId, "mp3");
                }
                if (format.Contains("AL"))
                {
                    song.BitRate = "无损";
                    song.ApeUrl = CommonHelper.GetSongUrl("kw", "1000", song.SongId, "ape");
                }
                if (format.Contains("MP4"))
                {
                    song.MvHdUrl = song.MvLdUrl = CommonHelper.GetSongUrl("kw", "1000", song.SongId, "mp4");
                }
                if (format.Contains("MV"))
                {
                    song.MvLdUrl = CommonHelper.GetSongUrl("kw", "1000", song.SongId, "mkv");
                }
                list.Add(song);
            }
            return list;
        }

        private static List<SongResult> SearchArtist(string id, int page, int size)
        {
            //http://search2013.kuwo.cn/r.s?stype=artist2music&artistid=947&pn=0&rn=30
            var url = "http://search.kuwo.cn/r.s?ft=music&itemset=newkw&newsearch=1&cluster=0&rn=" + size + "&pn=" +
                      (page - 1) + "&primitive=0&rformat=json&encoding=UTF8&artist=" + id;
            if (Regex.IsMatch(id, @"^\d+$"))
            {
                url = "http://search.kuwo.cn/r.s?ft=music&itemset=newkw&newsearch=1&cluster=0&rn=" + size + "&pn=" +
                      (page - 1) + "&primitive=0&rformat=json&encoding=UTF8&artistid=" + id;
            }
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                if (json["TOTAL"].ToString() == "0")
                {
                    return null;
                }
                var datas = json["abslist"];
                return GetListByJson(datas);
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static List<SongResult> SearchCollect(string id, int page, int size)
        {
            var url = "http://nplserver.kuwo.cn/pl.svc?op=getlistinfo&pid=" + id + "&pn=" + (page - 1) + "&rn=" + size +
                      "&encode=utf-8&keyset=pl2012&identity=kuwo";
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html) || html == "{}")
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                return GetSongsByToken(json["musiclist"]);
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static SongResult SearchSong(string id)
        {
            var html = CommonHelper.GetHtmlContent("http://search.kuwo.cn/r.s?rformat=json&RID=MUSIC_" + id);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            var json = JObject.Parse(html);
            var list = GetSongsByToken(json["abslist"]);
            var song = list?[0];
            if (song == null) return null;
            html = CommonHelper.GetHtmlContent("http://www.kuwo.cn/webmusic/st/getMuiseByRid?rid=MUSIC_" + id + "&flag=1");
            if (string.IsNullOrEmpty(html)) return song;
            json = JObject.Parse(html);
            song.SongName = json["songName"].ToString().HtmlDecode();
            song.ArtistName = json["artist"].ToString().HtmlDecode();
            song.AlbumName = json["album"].ToString().HtmlDecode();
            song.AlbumArtist = json["songName"].ToString().HtmlDecode();
            song.Length = Convert.ToInt32(json["duration"]?.ToString() ?? "0");
            return song;
        }

        private static string GetUrl(string id, string quality, string format)
        {
            if (format == "lrc")
            {
                var html =
                    CommonHelper.GetHtmlContent("http://mobile.kuwo.cn/mpage/html5/songinfoandlrc?mid=" + id + "&flag=0");
                if (string.IsNullOrEmpty(html))
                {
                    return null;
                }
                var j = JObject.Parse(html);
                if (string.IsNullOrEmpty(j["lrclist"]?.ToString()))
                {
                    return null;
                }
                var name = j["songinfo"]["name"]?.ToString().HtmlDecode();
                var ar = j["songinfo"]["artist"]?.ToString().HtmlDecode();
                var sb = new StringBuilder();
                foreach (JToken jToken in j["lrclist"])
                {
                    sb.AppendLine("[" + CommonHelper.NumToTime(jToken["timeId"].ToString()) + ".00]" + jToken["text"]);
                }
                if (string.IsNullOrEmpty(sb.ToString()))
                {
                    return null;
                }
                return "[ti:" + name + "]\n[ar: " + ar + "]\n[by: 雅音FM]\n" + sb;
            }
            if (format == "jpg")
            {
                var html =
                    CommonHelper.GetHtmlContent("http://player.kuwo.cn/webmusic/sj/dtflagdate?flag=6&rid=MUSIC_" + id);
                if (string.IsNullOrEmpty(html) || !html.Contains(".jpg"))
                {
                    return quality == "low" ? "http://yyfm.oss-cn-qingdao.aliyuncs.com/anylisten/2311.jpg" : "http://yyfm.oss-cn-qingdao.aliyuncs.com/anylisten/23.jpg";
                }
                var strs = html.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (html.Contains("star/albumcover"))
                {
                    return strs[strs.Length - 1].Replace("albumcover/120", quality == "high" ? "albumcover/500" : "albumcover/120");
                }
                return strs[1].Replace("starheads/120", quality == "high" ? "starheads/500" : "starheads/120");
            }
            if (format == "mkv")
            {
                return "http://antiserver.kuwo.cn/anti.s?rid=MUSIC_" + id + "&response=res&format=mkv&type=convert_url";
            }
            if (format == "mp4")
            {
                return "http://antiserver.kuwo.cn/anti.s?rid=MUSIC_" + id + "&response=res&format=mp4&type=convert_url";
            }
            var text = "type=convert_url2&br=" + quality + "&format="+(format == "ape" ? "ape" : "mp3") +"&sig=0&rid="+id+"&network=wifi";
            var link = "http://nmobi.kuwo.cn/mobi.s?f=kuwo&q=" + Convert.ToBase64String(KuwoDES.EncryptToBytes(text, "ylzsxkwm"));
            var result = CommonHelper.GetHtmlContent(link);
            return string.IsNullOrEmpty(result) ? "" : Regex.Match(result, @"(?<=url=)http:\S+").Value;
        }

        public List<SongResult> SongSearch(string key, int page, int size)
        {
            return Search(key, page, size);
        }

        public List<SongResult> AlbumSearch(string id)
        {
            return SearchAlbum(id);
        }

        public List<SongResult> ArtistSearch(string id, int page, int size)
        {
            return SearchArtist(id, page, size);
        }

        public List<SongResult> CollectSearch(string id, int page, int size)
        {
            return SearchCollect(id, page, size);
        }

        public List<SongResult> GetSingleSong(string id)
        {
            return new List<SongResult> { SearchSong(id) };
        }

        public string GetSongUrl(string id, string quality, string format)
        {
            return GetUrl(id, quality, format);
        }
    }
}