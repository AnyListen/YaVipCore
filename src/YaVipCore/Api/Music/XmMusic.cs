using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using YaVipCore.Helper;
using YaVipCore.Interface;
using YaVipCore.Models;

namespace YaVipCore.Api.Music
{
    public class XmMusic:IMusic
    {
        public static List<SongResult> Search(string key, int page)
        {
            var url = "http://www.xiami.com/app/xiating/search-song2?key=" + key + "&uid=0&callback=xiami&page=" + page;
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html) || html == "null" || html.Contains("无法在虾米资料库中得到结果"))
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                var data = json["data"];
                var list = new List<SongResult>();
                foreach (var j in data)
                {
                    try
                    {
                        var song = new SongResult
                        {
                            SongId = j["songId"].ToString(),
                            SongName = j["songName"].ToString(),
                            SongSubName = j["subName"]?.ToString(),
                            SongLink = "http://www.xiami.com/song/" + j["songId"],

                            ArtistId = j["artist"]["artistId"].ToString(),
                            ArtistName = j["artist"]["artistName"].ToString(),
                            ArtistSubName = j["artist"]["alias"]?.ToString(),

                            AlbumId = j["album"]["albumId"]?.ToString() ?? "0",
                            AlbumName = j["album"]["albumName"]?.ToString() ?? "",
                            AlbumSubName = j["album"]["subTitle"]?.ToString() ?? "",
                            AlbumArtist = j["album"]["artist"]?["artistName"].ToString() ?? j["artist"]["artistName"].ToString(),

                            Length = Convert.ToInt32(Convert.ToDouble(j["length"].ToString())),
                            BitRate = "320K",

                            FlacUrl = "",
                            ApeUrl = "",
                            WavUrl = "",
                            SqUrl = CommonHelper.GetSongUrl("xm", "320", j["songId"].ToString(), "mp3"),
                            HqUrl = CommonHelper.GetSongUrl("xm", "192", j["songId"].ToString(), "mp3"),
                            LqUrl = CommonHelper.GetSongUrl("xm", "128", j["songId"].ToString(), "mp3"),
                            CopyUrl = CommonHelper.GetSongUrl("xm", "320", j["songId"].ToString(), "mp3"),

                            SmallPic = ("http://img.xiami.net/" + j["album_logo"]),
                            PicUrl = ("http://img.xiami.net/" + j["album_logo"]).Replace("_1.", "_4."),

                            LrcUrl = CommonHelper.GetSongUrl("xm", "128", j["songId"].ToString(), "lrc"),
                            TrcUrl = "",
                            KrcUrl = "",

                            MvId = j["mvId"]?.ToString(),
                            MvHdUrl = "",
                            MvLdUrl = "",

                            Language = j["album"]["language"]?.ToString() ?? "",
                            Company = j["album"]["company"]?.ToString() ?? "",
                            Year =string.IsNullOrEmpty(j["demoCreateTime"].ToString())?"" : CommonHelper.UnixTimestampToDateTime(Convert.ToInt64(string.IsNullOrEmpty(j["demoCreateTime"].ToString()) ? "0" : j["demoCreateTime"].ToString())/1000).ToString("yyyy-MM-dd"),
                            Disc = 1,
                            TrackNum = 0,
                            Type = "xm"
                        };
                        int disc;
                        if (Int32.TryParse(j["cdSerial"].ToString(), out disc))
                        {
                            song.Disc = disc;
                        }
                        if (Int32.TryParse(j["track"].ToString(), out disc))
                        {
                            song.TrackNum = disc;
                        }
                        if (!string.IsNullOrEmpty(song.MvId))
                        {
                            if (song.MvId != "0")
                            {
                                song.MvHdUrl = CommonHelper.GetSongUrl("xm", "hd", song.SongId, "mp4");
                                song.MvLdUrl = CommonHelper.GetSongUrl("xm", "ld", song.SongId, "mp4");
                            }
                        }
                        foreach (JToken jToken in j["audios"])
                        {
                            switch (jToken["audioQualityEnum"].ToString())
                            {
                                case "LOW":
                                    song.LqUrl = jToken["filePath"].ToString();
                                    break;
                                case "LOSSLESS":
                                    if (jToken["format"].ToString() == "flac")
                                    {
                                        song.FlacUrl = jToken["filePath"].ToString();
                                    }
                                    else if (jToken["format"].ToString() == "wav")
                                    {
                                        song.WavUrl = jToken["filePath"].ToString();
                                    }
                                    else
                                    {
                                        song.ApeUrl = jToken["filePath"].ToString();
                                    }
                                    song.BitRate = "无损";
                                    break;
                                case "HIGH":
                                    song.HqUrl = song.SqUrl = jToken["filePath"].ToString();
                                    break;
                            }
                        }
                        list.Add(song);
                    }
                    catch (Exception ex)
                    {
                        CommonHelper.AddLog(ex.ToString());
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static List<SongResult> SearchAlbum(string id)
        {
            var list = GetResultsByIds(id, 1);
            if (list == null || list.Count <= 0)
            {
                list = GetLostAlbum(id);
            }
            var url = "http://www.xiami.com/app/xiating/album?spm=0.0.0.0.L6k2wP&id=" + id + "&uid=0";
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return list;
            }
            try
            {
                var match = Regex.Match(html,
                    @"(?<=h1 title="")([^""]+)(?:""[\s\S]+?p class="")([^""]*)(?:""[\s\S]+detail_songer"">)([^<]+)(?:</a>[\s\S]+?<em>)([^<]+)(?:</em>[\s\S]+?<em>)([^<]+)(?:</em>[\s\S]+?<em>)([^<]+)(?:</em>[\s\S]+?<em>)([^<]+)(?:</em>)");
                if (match.Length <= 0)
                {
                    return list;
                }
                foreach (var s in list)
                {
                    s.AlbumSubName = match.Groups[2].Value;
                    s.AlbumArtist = match.Groups[3].Value;
                    s.Language = match.Groups[4].Value;
                    s.Company = match.Groups[5].Value;
                    s.Year = match.Groups[6].Value.Replace("年", "-").Replace("月", "-").Replace("日", "");
                }
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
            }
            return list;
        }

        private static List<SongResult> GetResultsByIds(string ids, int type)
        {
            var albumUrl = "http://www.xiami.com/song/playlist/id/" + ids + "/type/" + type + "/cat/json";
            var html = CommonHelper.GetHtmlContent(albumUrl);
            if (string.IsNullOrEmpty(html) || html.Contains("应版权方要求，没有歌曲可以播放"))
            {
                return null;
            }
            var list = new List<SongResult>();
            try
            {
                var json = JObject.Parse(html);
                var data = json["data"]["trackList"];
                if (string.IsNullOrEmpty(data.ToString()))
                {
                    return null;
                }
                foreach (var j in data)
                {
                    try
                    {
                        var song = new SongResult
                        {
                            SongId = j["songId"].ToString(),
                            SongName = j["songName"].ToString().HtmlDecode(),
                            SongSubName = j["subName"]?.ToString().HtmlDecode(),
                            SongLink = "http://www.xiami.com/song/" + j["song_id"],

                            ArtistId = j["artistId"].ToString(),
                            ArtistName = j["singers"].ToString().HtmlDecode(),
                            ArtistSubName = j["artist_sub_title"]?.ToString().HtmlDecode(),

                            AlbumId = j["albumId"].ToString(),
                            AlbumName = j["album_name"].ToString().HtmlDecode(),
                            AlbumSubName = j["album_sub_title"]?.ToString().HtmlDecode(),
                            AlbumArtist = j["artist"].ToString().HtmlDecode(),

                            Length = Convert.ToInt32(j["length"].ToString()),
                            BitRate = "128K",

                            FlacUrl = "",
                            ApeUrl = "",
                            WavUrl = "",
                            SqUrl = "",
                            HqUrl = "",
                            LqUrl = Jurl(j["location"].ToString()),
                            CopyUrl = CommonHelper.GetSongUrl("xm", "320", j["song_id"].ToString(), "mp3"),

                            SmallPic = ("http://img.xiami.net/" + j["album_logo"]),
                            PicUrl = ("http://img.xiami.net/" + j["album_logo"]).Replace("_1.", "_4."),

                            LrcUrl = j["lyric"].ToString(),
                            TrcUrl = "",
                            KrcUrl = j["lyric_karaok"]?.ToString(),

                            MvId = j["mvUrl"]?.ToString(),
                            MvHdUrl = "",
                            MvLdUrl = "",

                            Language = "",
                            Company = "",
                            Year = "",
                            Disc = Convert.ToInt32(j["cd_serial"].ToString()),
                            TrackNum = Convert.ToInt32(j["track"].ToString()),
                            Type = "xm"
                        };
                        if (j["purview"] != null)
                        {
                            song.BitRate = "320K";
                            song.SqUrl = song.HqUrl = j["purview"]["filePath"]?.ToString();
                        }
                        if (!string.IsNullOrEmpty(song.MvId))
                        {
                            if (song.MvId != "0")
                            {
                                song.MvHdUrl = CommonHelper.GetSongUrl("xm", "hd", song.SongId, "mp4");
                                song.MvLdUrl = CommonHelper.GetSongUrl("xm", "ld", song.SongId, "mp4");
                            }
                        }
                        if (!string.IsNullOrEmpty(j["ttpodId"]?.ToString()))
                        {
                            song.BitRate = "320K";
                            song.HqUrl = CommonHelper.GetSongUrl("tt", "192", j["ttpodId"].ToString(), "mp3");
                            song.SqUrl = CommonHelper.GetSongUrl("tt", "320", j["ttpodId"].ToString(), "mp3");
                        }
                        list.Add(song);
                    }
                    catch (Exception ex)
                    {
                        CommonHelper.AddLog(ex.ToString());
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static List<SongResult> GetLostAlbum(string id)
        {
            var url = "http://api.xiami.com/web?id=" + id + "&r=album%2Fdetail&app_key=09bef203bfa02bfbe3f1cfd7073cb0f3";
            var html = CommonHelper.GetHtmlContent(url, 1, new Dictionary<string, string>
            {
                {"Referer", "http://m.xiami.com/"}
            });
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            var json = JObject.Parse(html);
            var datas = json["data"]["songs"];
            var list = new List<SongResult>();
            var index = 0;
            foreach (JToken j in datas)
            {
                index++;
                var song = new SongResult
                {
                    SongId = j["song_id"].ToString(),
                    SongName = j["song_name"].ToString(),
                    SongSubName = "",

                    ArtistId = j["artist_id"].ToString(),
                    ArtistName = j["singers"].ToString(),
                    ArtistSubName = "",

                    AlbumId = j["album_id"].ToString(),
                    AlbumName = j["album_name"].ToString(),
                    AlbumSubName = "",
                    AlbumArtist = json["data"]["artist_name"].ToString(),

                    Length = string.IsNullOrEmpty(j["songtime"]?.ToString()) ? 0 : Convert.ToInt32(j["songtime"].ToString()),
                    BitRate = "320K",

                    FlacUrl = "",
                    ApeUrl = "",
                    WavUrl = "",
                    SqUrl = CommonHelper.GetSongUrl("xm", "320", j["song_id"].ToString(), "mp3"),
                    HqUrl = CommonHelper.GetSongUrl("xm", "192", j["song_id"].ToString(), "mp3"),
                    LqUrl = CommonHelper.GetSongUrl("xm", "128", j["song_id"].ToString(), "mp3"),
                    CopyUrl = CommonHelper.GetSongUrl("xm", "320", j["song_id"].ToString(), "mp3"),

                    SmallPic = j["album_logo"].ToString(),
                    PicUrl = j["album_logo"].ToString().Replace("_1.", "_4."),

                    LrcUrl = CommonHelper.GetSongUrl("xm", "128", j["song_id"].ToString(), "lrc"),
                    TrcUrl = "",
                    KrcUrl = "",

                    MvId = j["mv_id"]?.ToString(),
                    MvHdUrl = "",
                    MvLdUrl = "",

                    Language = "",
                    Company = "",
                    Year = CommonHelper.UnixTimestampToDateTime(Convert.ToInt64(json["data"]["gmt_publish"].ToString())).ToString("yyyy-MM-dd"),
                    Disc = 1,
                    TrackNum = index,
                    Type = "xm"
                };
                if (j["purview_roles"] != null)
                {
                    foreach (JToken jToken in j["purview_roles"])
                    {
                        switch (jToken["quality"].ToString())
                        {
                            case "l":
                                song.LqUrl = CommonHelper.GetSongUrl("xm", "128", song.SongId, "mp3");
                                break;
                            case "h":
                                song.HqUrl = song.SqUrl = CommonHelper.GetSongUrl("xm", "320", song.SongId, "mp3");
                                break;
                            case "s":
                                song.FlacUrl = CommonHelper.GetSongUrl("xm", "999", song.SongId, "flac");
                                song.BitRate = "无损";
                                break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(song.MvId))
                {
                    if (song.MvId != "0")
                    {
                        song.MvHdUrl = CommonHelper.GetSongUrl("xm", "hd", song.SongId, "mp4");
                        song.MvLdUrl = CommonHelper.GetSongUrl("xm", "ld", song.SongId, "mp4");
                    }
                }
                list.Add(song);
            }
            return list;
        }

        private static SongResult SearchSong(string songId)
        {
            var list = GetResultsByIds(songId, 0);
            var song = list?[0] ?? GetLostSong(songId);
            if (song != null)
            {
                GetSongDetials(song);
            }
            return song;
        }

        private static SongResult GetLostSong(string songId)
        {
            var url = "http://api.xiami.com/web?id=" + songId + "&r=song%2Fdetail&app_key=09bef203bfa02bfbe3f1cfd7073cb0f3";
            var html = CommonHelper.GetHtmlContent(url, 1, new Dictionary<string, string>
            {
                {"Referer", "http://m.xiami.com/"}
            });
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            var json = JObject.Parse(html);
            var songName = json["data"]["song"]["song_name"] + "-" + json["data"]["song"]["artist_name"];
            var list = Search(songName, 1);
            var single = list?.FirstOrDefault(x => x.SongId == songId);
            return single;
        }

        private static void GetSongDetials(SongResult song)
        {
            var url = "http://www.xiami.com/app/xiating/album?spm=0.0.0.0.L6k2wP&id=" + song.AlbumId + "&uid=0";
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return;
            }
            var match = Regex.Match(html,
                @"(?<=h1 title="")([^""]+)(?:""[\s\S]+?p class="")([^""]*)(?:""[\s\S]+detail_songer"">)([^<]+)(?:</a>[\s\S]+?<em>)([^<]+)(?:</em>[\s\S]+?<em>)([^<]+)(?:</em>[\s\S]+?<em>)([^<]+)(?:</em>[\s\S]+?<em>)([^<]+)(?:</em>)");
            if (match.Length <= 0)
            {
                return;
            }
            song.AlbumName = match.Groups[1].Value;
            song.AlbumSubName = match.Groups[2].Value;
            song.AlbumArtist = match.Groups[3].Value;
            song.Language = match.Groups[4].Value;
            song.Company = match.Groups[5].Value;
            song.Year = match.Groups[6].Value.Replace("年", "-").Replace("月", "-").Replace("日", "");
            var discs = Regex.Matches(html, @"(?<=<ul class=""playlist)[\s\S]+?(?=</ul>)");
            for (int i = 0; i < discs.Count; i++)
            {
                match = Regex.Match(discs[i].Value, @"(?<=rel=""" + song.SongId + @"""[\s\S]+?list_index"">)\d+(?=[\s\S]+?id=" + song.SongId + ")");
                if (!string.IsNullOrEmpty(match.Value))
                {
                    song.TrackNum = Convert.ToInt32(match.Value);
                    song.Disc = i + 1;
                }
            }
        }

        private List<SongResult> SearchArtist(string id, int page)
        {
            var text = "id=" + id + "&uid=0&callback=xiami&page=" + page;
            var html = CommonHelper.GetHtmlContent("http://www.xiami.com/app/xiating/artist-song?" + text);
            if (string.IsNullOrEmpty(html))
            {
                html = CommonHelper.GetHtmlContent("http://www.xiami.com/app/xiating/artist-song?" + text);
                if (string.IsNullOrEmpty(html))
                {
                    html = CommonHelper.GetHtmlContent("http://www.xiami.com/app/xiating/artist-song?" + text);
                    if (string.IsNullOrEmpty(html))
                    {
                        return null;
                    }
                }
            }
            var json = JObject.Parse(html);
            var data = json["data"];
            var list = new List<SongResult>();
            foreach (var j in data)
            {
                try
                {
                    var song = new SongResult
                    {
                        SongId = j["song_id"].ToString(),
                        SongName = j["name"].ToString(),
                        SongSubName = j["singers"]?.ToString(),
                        SongLink = "http://www.xiami.com/song/" + j["song_id"],

                        ArtistId = j["artist_id"].ToString(),
                        ArtistName = j["artist_name"].ToString(),
                        ArtistSubName = j["artist_sub_title"]?.ToString(),

                        AlbumId = j["album_id"]?.ToString() ?? "0",
                        AlbumName = j["album_name"]?.ToString() ?? "",
                        AlbumSubName = j["album_sub_title"]?.ToString(),
                        AlbumArtist = j["artist_name"].ToString(),

                        Length = CommonHelper.TimeToNum(j["songtime"].ToString()),
                        BitRate = "320K",

                        FlacUrl = "",
                        ApeUrl = "",
                        WavUrl = "",
                        SqUrl = CommonHelper.GetSongUrl("xm", "320", j["song_id"].ToString(), "mp3"),
                        HqUrl = CommonHelper.GetSongUrl("xm", "192", j["song_id"].ToString(), "mp3"),
                        LqUrl = CommonHelper.GetSongUrl("xm", "128", j["song_id"].ToString(), "mp3"),
                        CopyUrl = CommonHelper.GetSongUrl("xm", "320", j["song_id"].ToString(), "mp3"),

                        SmallPic = "http://img.xiami.net/" + j["album_logo"],
                        PicUrl = ("http://img.xiami.net/" + j["album_logo"]).Replace("_1.", "_4."),

                        LrcUrl = CommonHelper.GetSongUrl("xm", "320", j["song_id"].ToString(), "lrc"),
                        TrcUrl = "",
                        KrcUrl = "",

                        MvId = j["mv_id"]?.ToString(),
                        MvHdUrl = "",
                        MvLdUrl = "",

                        Language = "",
                        Company = "",
                        Year = "",
                        Disc = string.IsNullOrEmpty(j["cd_serial"]?.ToString()) ? 1 : Convert.ToInt32(j["cd_serial"].ToString()),
                        TrackNum = string.IsNullOrEmpty(j["track"]?.ToString()) ? 0 : Convert.ToInt32(j["track"].ToString()),
                        Type = "xm"
                    };
                    if (!string.IsNullOrEmpty(song.MvId))
                    {
                        if (song.MvId != "0")
                        {
                            song.MvHdUrl = CommonHelper.GetSongUrl("xm", "hd", song.SongId, "mp4");
                            song.MvLdUrl = CommonHelper.GetSongUrl("xm", "ld", song.SongId, "mp4");
                        }
                    }
                    list.Add(song);
                }
                catch (Exception ex)
                {
                    CommonHelper.AddLog(ex.ToString());
                }
            }
            return list;
        }

        private List<SongResult> SearchCollect(string id)
        {
            var url = "http://api.xiami.com/web?v=2.0&app_key=1&id=" + id + "&type=collectId&r=collect/detail";
            var html = CommonHelper.GetHtmlContent(url, 1, new Dictionary<string, string>
            {
                {"Referer", "http://m.xiami.com/"}
            });
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                var datas = json["data"]["songs"];
                var list = new List<SongResult>();
                foreach (JToken j in datas)
                {
                    if (j["song_id"].ToString() == "0")
                    {
                        return null;
                    }
                    var song = new SongResult
                    {
                        SongId = j["song_id"].ToString(),
                        SongName = j["song_name"].ToString(),
                        SongSubName = "",

                        ArtistId = j["artist_id"].ToString(),
                        ArtistName = j["singers"].ToString(),
                        ArtistSubName = "",

                        AlbumId = j["album_id"].ToString(),
                        AlbumName = j["album_name"].ToString(),
                        AlbumSubName = "",
                        AlbumArtist = j["artist_name"].ToString(),

                        Length = Convert.ToInt32(j["length"].ToString()),
                        BitRate = "320K",

                        FlacUrl = "",
                        ApeUrl = "",
                        WavUrl = "",
                        SqUrl = CommonHelper.GetSongUrl("xm", "320", j["song_id"].ToString(), "mp3"),
                        HqUrl = CommonHelper.GetSongUrl("xm", "192", j["song_id"].ToString(), "mp3"),
                        LqUrl = j["listen_file"].ToString(),
                        CopyUrl = CommonHelper.GetSongUrl("xm", "320", j["song_id"].ToString(), "mp3"),

                        SmallPic = j["album_logo"].ToString(),
                        PicUrl = j["album_logo"].ToString().Replace("_1.", "_4."),
                        LrcUrl = CommonHelper.GetSongUrl("xm", "128", j["song_id"].ToString(), "lrc"),
                        TrcUrl = "",
                        KrcUrl = "",

                        MvId = j["mv_id"]?.ToString(),
                        MvHdUrl = "",
                        MvLdUrl = "",

                        Language = "",
                        Company = "",
                        Year = "",
                        Disc = 1,
                        TrackNum = 0,
                        Type = "xm"
                    };
                    if (string.IsNullOrEmpty(song.LqUrl))
                    {
                        song.LqUrl = CommonHelper.GetSongUrl("xm", "128", j["song_id"].ToString(), "mp3");
                    }
                    if (!string.IsNullOrEmpty(song.MvId))
                    {
                        if (song.MvId != "0")
                        {
                            song.MvHdUrl = CommonHelper.GetSongUrl("xm", "hd", song.SongId, "mp4");
                            song.MvLdUrl = CommonHelper.GetSongUrl("xm", "ld", song.SongId, "mp4");
                        }
                    }
                    list.Add(song);
                }
                return list;
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static string Jurl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return "";
            }
            var num = Convert.ToInt32(url.Substring(0, 1));
            var newurl = url.Substring(1);
            var yushu = newurl.Length % num;
            var colunms = (int)Math.Ceiling((double)newurl.Length / num);
            var arrList = new string[num];
            var a = 0;
            for (var i = 0; i < num; i++)
            {
                if (i < yushu)
                {
                    arrList[i] = newurl.Substring(a, colunms);
                    a += colunms;
                }
                else
                {
                    if (yushu == 0)
                    {
                        arrList[i] = newurl.Substring(a, colunms);
                        a += colunms;
                    }
                    else
                    {
                        arrList[i] = newurl.Substring(a, colunms - 1);
                        a += (colunms - 1);
                    }
                }
            }
            var sb = new StringBuilder();
            if (yushu == 0)
            {
                for (var i = 0; i < colunms; i++)
                {
                    for (var j = 0; j < num; j++)
                    {
                        sb.Append(arrList[j].Substring(i, 1));
                    }
                }
            }
            else
            {
                for (var i = 0; i < colunms; i++)
                {
                    if (i == colunms - 1)
                    {
                        for (var j = 0; j < yushu; j++)
                        {
                            sb.Append(arrList[j].Substring(i, 1));
                        }
                    }
                    else
                    {
                        for (var j = 0; j < num; j++)
                        {
                            sb.Append(arrList[j].Substring(i, 1));
                        }
                    }
                }
            }
            var str = WebUtility.UrlDecode(sb.ToString());
            return str?.Replace("^", "0").Replace("+", " ").Replace(".mp$", "mp3");
        }

        private static string GetUrl(string id, string quality, string format)
        {
            if (format == "mp4" || format == "flv")
            {
                string mvId;
                string html;
                if (Regex.IsMatch(id, @"^\d+$"))
                {
                    var url = "http://www.xiami.com/song/" + id;
                    html = CommonHelper.GetHtmlContent(url);
                    if (string.IsNullOrEmpty(html))
                    {
                        return "";
                    }
                    mvId = Regex.Match(html, @"(?<=href=""/mv/)\w+(?="")").Value;
                }
                else
                {
                    mvId = id;
                }
                if (string.IsNullOrEmpty(mvId))
                {
                    return null;
                }
                html = CommonHelper.GetHtmlContent("http://m.xiami.com/mv/" + mvId, 2);
                return string.IsNullOrEmpty(html) ? "" : Regex.Match(html, @"(?<=<video src="")[^""]+(?=""\s*poster=)").Value;
            }
            if (format == "flac")
            {
                var single = GetLostSong(id);
                if (!string.IsNullOrEmpty(single.FlacUrl))
                {
                    return single.FlacUrl;
                }
                if (!string.IsNullOrEmpty(single.ApeUrl))
                {
                    return single.ApeUrl;
                }
                if (!string.IsNullOrEmpty(single.WavUrl))
                {
                    return single.WavUrl;
                }
                return null;
            }
            var song = SearchSong(id);
            if (song == null)
            {
                return null;
            }
            if (format == "lrc")
            {
                return song.LrcUrl;
            }
            if (format == "jpg")
            {
                return song.PicUrl;
            }
            
            if (string.IsNullOrEmpty(song.LqUrl) && string.IsNullOrEmpty(song.SqUrl))
            {
                var list = Search(song.ArtistName + " - " + song.SongName, 1);
                if (list != null)
                {
                    song = list.FirstOrDefault(x => x.SongId == id) ??
                           TtMusic.GetXmSqUrl(song.ArtistName, song.SongName);
                }
            }
            return quality != "128" ? song.SqUrl : song.LqUrl;
        }

        public List<SongResult> SongSearch(string key, int page, int size)
        {
            return Search(key, page);
        }

        public List<SongResult> AlbumSearch(string id)
        {
            return SearchAlbum(id);
        }

        public List<SongResult> ArtistSearch(string id, int page, int size)
        {
            return SearchArtist(id, page);
        }

        public List<SongResult> CollectSearch(string id, int page, int size)
        {
            return SearchCollect(id);
        }

        public List<SongResult> GetSingleSong(string id)
        {
            return new List<SongResult> {SearchSong(id)};
        }

        public string GetSongUrl(string id, string quality, string format)
        {
            return GetUrl(id, quality, format);
        }
    }
}