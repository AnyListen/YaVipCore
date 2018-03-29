using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using YaVipCore.Helper;
using YaVipCore.Interface;
using YaVipCore.Models;

namespace YaVipCore.Api.Music
{
    public class TtMusic : IMusic
    {
        public static List<SongResult> Search(string key, int page, int size)
        {
            //var url = "http://search.dongting.com/song/search?page=" + page + "&user_id=0&tid=0&app=ttpod&size=" + size + "&q=" + key + "&active=0";
            //var url = "http://pcweb.ttpod.com/search/song?page=" + page + "&size=" + size + "50&q=" + key;
            //var html = CommonHelper.GetHtmlContent(url);
            //if (string.IsNullOrEmpty(html))
            //{
            //    return null;
            //}
            //try
            //{
            //    var json = JObject.Parse(html);
            //    if (json["total"].ToString() == "0")
            //    {
            //        return null;
            //    }
            //    var datas = json["data"];
            //    return GetListByJson(datas);
            //}
            //catch (Exception ex)
            //{
            //    CommonHelper.AddLog(ex.ToString());
            //    return null;
            //}

            return null;
        }

        private static List<SongResult> GetListByJson(JToken datas)
        {
            var list = new List<SongResult>();
            foreach (var j in datas)
            {
                try
                {
                    var song = new SongResult
                    {
                        SongId = j["songId"].ToString(),
                        SongName = j["songName"].ToString(),
                        SongSubName = j["alias"]?.ToString(),
                        SongLink = "http://h.dongting.com/yule/app/music_player_page.html?id=" + j["songId"],

                        ArtistId = j["singerId"].ToString(),
                        ArtistName = j["singerName"].ToString(),
                        ArtistSubName = "",

                        AlbumId = j["albumId"].ToString(),
                        AlbumName = j["albumName"].ToString(),
                        AlbumSubName = "",
                        AlbumArtist = j["singerName"].ToString(),

                        Length = 0,
                        BitRate = "128K",

                        FlacUrl = "",
                        ApeUrl = "",
                        WavUrl = "",
                        SqUrl = "",
                        HqUrl = "",
                        LqUrl = "",
                        CopyUrl = "",

                        SmallPic = CommonHelper.GetSongUrl("tt", "small", j["songId"].ToString(), "jpg"),
                        PicUrl = CommonHelper.GetSongUrl("tt", "big", j["songId"].ToString(), "jpg"),
                        LrcUrl = CommonHelper.GetSongUrl("tt", "320", j["songId"].ToString(), "lrc"),
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
                        Type = "tt"
                    };
                    if (j["mvList"]?.First != null && j["mvList"].First.HasValues)
                    {
                        var mvs = j["mvList"];
                        var max = 0;
                        foreach (JToken mv in mvs)
                        {
                            song.MvId = mv["videoId"].ToString();
                            if (max == 0)
                            {
                                song.MvHdUrl = mv["url"].ToString();
                                song.MvLdUrl = mv["url"].ToString();
                                max = mv["bitRate"].Value<int>();
                            }
                            else
                            {
                                if (mv["bitRate"].Value<int>() > max)
                                {
                                    song.MvHdUrl = mv["url"].ToString();
                                }
                                else
                                {
                                    song.MvLdUrl = mv["url"].ToString();
                                }
                            }
                        }
                    }
                    var links = j["urlList"];
                    if (links == null || links.ToString() == "[]" || j["urlList"].First == null)
                    {
                        continue;
                    }
                    foreach (JToken link in links)
                    {
                        switch (link["bitRate"].ToString())
                        {
                            case "128":
                                song.LqUrl = link["url"].ToString();
                                song.BitRate = "128K";
                                break;
                            case "192":
                                song.HqUrl = link["url"].ToString();
                                song.BitRate = "192K";
                                break;
                            case "320":
                                if (string.IsNullOrEmpty(song.HqUrl))
                                {
                                    song.HqUrl = link["url"].ToString();
                                }
                                song.SqUrl = link["url"].ToString();
                                song.BitRate = "320K";
                                break;
                        }
                        song.Length = link["duration"].ToString().Contains(":")
                            ? CommonHelper.TimeToNum(link["duration"].ToString())
                            : link["duration"].Value<int>()/1000;
                    }
                    if (j["llList"] != null && j["llList"].ToString() != "null" && j["llList"].ToString() != "[]")
                    {
                        foreach (JToken wsJToken in j["llList"])
                        {
                            song.BitRate = "无损";
                            switch (wsJToken["suffix"].ToString())
                            {
                                case "wav":
                                    song.WavUrl = wsJToken["url"].ToString();
                                    break;
                                case "ape":
                                    song.ApeUrl = wsJToken["url"].ToString();
                                    break;
                                case "flac":
                                    song.FlacUrl = wsJToken["url"].ToString();
                                    break;
                                default:
                                    song.FlacUrl = wsJToken["url"].ToString();
                                    break;
                            }
                        }
                    }
                    song.CopyUrl = CommonHelper.GetSongUrl("tt", "320", song.SongId, "mp3");
                    list.Add(song);
                }
                catch (Exception ex)
                {
                    CommonHelper.AddLog(ex.ToString());
                }
            }
            return list;
        }

        private static List<SongResult> SearchAlbum(string id)
        {
            var url = "http://api.dongting.com/song/album/" + id;
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                if (string.IsNullOrEmpty(json["data"]?.ToString()) || json["data"].ToString() == "null")
                {
                    return null;
                }
                var datas = json["data"]["songList"];
                var year = json["data"]["publishDate"].ToString();
                var cmp = json["data"]["companyName"].ToString();
                var lug = json["data"]["lang"].ToString();
                var ar = json["data"]["singerName"].ToString();
                var pic = json["data"]["picUrl"].ToString();
                var alias = json["data"]["alias"].ToString();
                var songs = json["data"]["songs"].Select(jToken => jToken.ToString()).ToList();
                var list = GetListByJson(datas);
                foreach (var r in list)
                {
                    r.TrackNum = songs.IndexOf(r.SongId) + 1;
                    r.Year = year;
                    r.Company = cmp;
                    r.Language = lug;
                    r.AlbumArtist = ar;
                    r.PicUrl = pic;
                    r.AlbumSubName = alias;
                }
                return list;
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static List<SongResult> SearchArtist(string id, int page, int size)
        {

            var url = "http://api.dongting.com/song/singer/" + id +
                     "/songs?app=ttpod&from=android&api_version=1.0&size=" + size + "&page=" + page + "&user_id=0&tid=0";
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                if (json["totalCount"].ToString() == "0")
                {
                    return null;
                }
                var datas = json["data"];
                return GetListByJson(datas);
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static List<SongResult> SearchCollect(string id)
        {

            var url = "http://api.songlist.ttpod.com/songlists/" + id;
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            var json = JObject.Parse(html);
            if (string.IsNullOrEmpty(json["songs"]?.ToString()) || json["songs"].ToString() == "null")
            {
                return null;
            }
            try
            {
                var datas = json["songs"];
                return GetListByJson(datas);
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static SongResult SearchSong(string id)
        {
            var html = CommonHelper.GetHtmlContent("http://api.dongting.com/song/song/" + id);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                var str = "[" + json["data"] + "]";
                var datas = JToken.Parse(str);
                var list = GetListByJson(datas);
                return list == null || list.Count <= 0 ? null : list[0];
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static string GetUrl(string id, string quality, string format)
        {
            var song = SearchSong(id);
            if (song == null && format == "jpg")
            {
                var url = "http://api.dongting.com/song/song/" + id;
                var html = CommonHelper.GetHtmlContent(url);
                if (string.IsNullOrEmpty(html))
                {
                    return "";
                }
                var link = Regex.Match(html, @"(?<=picUrl"":"")http://img.xiami.net[^""]+").Value;

                return quality == "small" ? link.Replace("_4.jpg", "_1.jpg") : link;
            }
            if (song == null)
            {
                return "";
            }
            switch (format)
            {
                case "mp4":
                case "flv":
                    return quality == "hd" ? song.MvHdUrl : song.MvLdUrl;
                case "lrc":
                    var url = "http://lp.music.ttpod.com/lrc/down?artist=" + WebUtility.UrlEncode(song.ArtistName) +
                              "&title=" + WebUtility.UrlEncode(song.SongName) + "&song_id=" + song.SongId;
                    var html = CommonHelper.GetHtmlContent(url);
                    if (string.IsNullOrEmpty(html))
                    {
                        return "";
                    }
                    var json = JObject.Parse(html);
                    return json["data"]?["lrc"]?.ToString();
                case "jpg":
                    return song.PicUrl;
                case "flac":
                    return song.FlacUrl;
                case "wav":
                    return song.WavUrl;
                case "ape":
                    return song.ApeUrl;
                case "mp3":
                    if (quality == "128")
                    {
                        return song.LqUrl;
                    }
                    return string.IsNullOrEmpty(song.SqUrl) ? song.LqUrl : song.SqUrl;
            }
            return song.LqUrl;
        }

        /// <summary>
        /// 根据艺术家和歌名得到歌曲信息
        /// 用于获取虾米付费歌曲
        /// </summary>
        /// <param name="ar">艺术家</param>
        /// <param name="name">歌名</param>
        /// <returns></returns>
        public static SongResult GetXmSqUrl(string ar, string name)
        {
            var key = ar + " - " + name;
            var list = Search(key, 1, 20);
            if (list == null)
            {
                return null;
            }
            if (list.Count <= 0)
            {
                return null;
            }
            var songs = list.Where(s => (s.SongName == name) && (s.ArtistName == ar)).ToList();
            var song = new SongResult();
            if (songs.Count <= 0)
            {
                decimal max = 0;
                var index = 0;
                var stringcompute1 = new StringCompute();
                foreach (var songResult in list)
                {
                    stringcompute1.SpeedyCompute(key, song.ArtistName + " - " + songResult.SongName);
                    var rate = stringcompute1.ComputeResult.Rate;
                    if (rate < (decimal)0.8)
                    {
                        continue;
                    }
                    if (rate > max)
                    {
                        max = rate;
                        song = list[index];
                    }
                    index++;
                }
            }
            else
            {
                song = songs[0];
            }
            return song;
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
            return SearchCollect(id);
        }

        public List<SongResult> GetSingleSong(string id)
        {
            return new List<SongResult> { SearchSong(id)};
        }

        public string GetSongUrl(string id, string quality, string format)
        {
            return GetUrl(id, quality, format);
        }
    }
}