using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using AnyListen.Helper;
using AnyListen.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnyListen.Music
{
    public class XiamiMusic : IMusic
    {
        #region 初始化
        private static string _xmToken = "";
        private static string _xmCookie = "";
        private static DateTime _xmLastUpdate = DateTime.MinValue;
        private static void UpdateXmToken()
        {
            if (!string.IsNullOrEmpty(_xmCookie) && (DateTime.Now - _xmLastUpdate <= new TimeSpan(0, 1, 0))) return;
            var data = "{\"requestStr\":\"{\\\"header\\\":{\\\"platformId\\\":\\\"h5\\\",\\\"callId\\\":1493962280189,\\\"appVersion\\\":1000000,\\\"resolution\\\":\\\"600*1067\\\"}}\"}";
            var time = CommonHelper.GetTimeSpan(true);
            var signData = "" + "&" + time + "&12574478&" + data;
            var sign = CommonHelper.Md5(signData);
            var url = $"https://h5api.m.xiami.com/h5/mtop.alimusic.operation.dailyxiamiservice.getcontent/1.0/?appKey=12574478&t={time}&sign={sign}&api=mtop.alimusic.operation.dailyxiamiservice.getcontent&v=1.0&type=jsonp&timeout=20000&dataType=jsonp&callback=mtopjsonp1&data=%7B%22requestStr%22%3A%22%7B%5C%22header%5C%22%3A%7B%5C%22platformId%5C%22%3A%5C%22h5%5C%22%2C%5C%22callId%5C%22%3A1493962280189%2C%5C%22appVersion%5C%22%3A1000000%2C%5C%22resolution%5C%22%3A%5C%22600*1067%5C%22%7D%7D%22%7D";
            var cookie = CommonHelper.GetHtmlCookie(url, "");
            if (string.IsNullOrEmpty(cookie))
            {
                return;
            }
            _xmLastUpdate = DateTime.Now;
            var tempCookie = "";
            var arr = cookie.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in arr)
            {
                if (!s.Contains("_m_h5_tk")) continue;
                if (s.Contains("_m_h5_tk="))
                {
                    _xmToken = s.Replace("_m_h5_tk=", "").Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)[0];
                }
                tempCookie += (s + ";");
            }
            _xmCookie = tempCookie.Replace("Path=/", "");
        }

        private static string GetXmUrl(string method = "mtop.alimusic.recommend.songservice.gethotsongs", string model = "\\\"model\\\":{\\\"language\\\":0,\\\"pagingVO\\\":{\\\"page\\\":1,\\\"pageSize\\\":1}}", string token = "66f5b6a91a718cf9a696ac42599fb7ea", string uid = "274841932")
        {
            //{"requestStr":"{\"header\":{\"platformId\":\"win\",\"remoteIp\":\"172.17.16.175\",\"callId\":1510626410633,\"sign\":\"d0e1b20a985f20aa3f355864ec75111e\",\"appId\":200,\"deviceId\":\"e3f9fcd22f3d535818ddcaa98d454050badc3c5303bd98af8b6984e6088fa702\",\"accessToken\":\"66f5b6a91a718cf9a696ac42599111ea\",\"openId\":274841942,\"network\":1,\"appVersion\":3000900,\"resolution\":\"1178*704\",\"utdid\":\"e3f9fcd22f3d535818ddcaa98d454050badc3c5303bd98af8b6984e6088fa702\"},\"model\":{\"limit\":100,\"page\":1,\"userId\":274841932}}"}
            var time = CommonHelper.GetTimeSpan(true);
            var data = "{\"requestStr\":\"{\\\"header\\\":{\\\"platformId\\\":\\\"win\\\",\\\"remoteIp\\\":\\\"172.17.16.175\\\",\\\"callId\\\":" + time + ",\\\"appId\\\":200,\\\"accessToken\\\":\\\"" + token + "\\\",\\\"openId\\\":" + uid + ",\\\"network\\\":1,\\\"appVersion\\\":3000900,\\\"resolution\\\":\\\"1178 *704\\\",\\\"utdid\\\":\\\"e3f9fcd22f3d535818ddcaa98d454050badc3c5303bd98af8b6984e6088fa702\\\"}," + model + "}\"}";
            var signData = _xmToken + "&" + time + "&12574478&" + data;
            var sign = CommonHelper.Md5(signData);
            var url =
                "http://h5api.m.xiami.com/h5/" + method + "/1.0/?appKey=12574478&t=" + time + "&sign=" + sign + "&v=1.0&type=originaljson&dataType=json&api=" + method + "&data=" + HttpUtility.UrlEncode(data);
            return url;
        }

        private static string GetXmHtml(string method, string model, string token = "66f5b6a91a718cf9a696ac42599fb7ea", string uid = "274841932")
        {
            if (string.IsNullOrEmpty(token))
            {
                token = "66f5b6a91a718cf9a696ac42599f111a";
            }
            if (string.IsNullOrEmpty(uid))
            {
                uid = "274841742";
            }
            UpdateXmToken();
            var url = GetXmUrl(method, model, token, uid);
            return CommonHelper.GetHtmlContent(url, 8, new Dictionary<string, string> { { "Cookie", _xmCookie } });
        }

        //private static string GetXmHtmlWithUser(string method, string data)
        //{
        //    UpdateXmToken();
        //    var time = CommonHelper.GetTimeSpan(true);
        //    var signData = _xmToken + "&" + time + "&12574478&" + data;
        //    var sign = CommonHelper.Md5(signData);
        //    var url =
        //        "http://acs.m.xiami.com/h5/" + method + "/1.0/?appKey=12574478&t=" + time + "&sign=" + sign + "&v=1.0&type=originaljson&dataType=json&api=" + method + "&data=" + WebUtility.UrlEncode(data);
        //    return CommonHelper.GetHtmlContent(url, 8, new Dictionary<string, string> { { "Cookie", _xmCookie } }).Html;
        //}
        #endregion

        private static List<SongResult> Search(string key, int page, int size)
        {
            var html = GetXmHtml("mtop.alimusic.search.searchservice.searchsongs",
                "\\\"model\\\":{\\\"key\\\":\\\"" + key.Replace("\"", "") + "\\\",\\\"pagingVO\\\":{\\\"page\\\":" + page + ",\\\"pageSize\\\":" + size + "}}");
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                if (json["data"]["data"]["pagingVO"]["count"].Value<int>() <= 0)
                {
                    return null;
                }
                var data = json["data"]["data"]["songs"];
                return GetListByToken(data);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<SongResult> GetListByToken(JToken data)
        {
            var list = new List<SongResult>();
            foreach (var j in data)
            {
                try
                {
                    var song = new SongResult
                    {
                        SongId = j["songId"].ToString(),
                        SongName = j["songName"].ToString(),
                        SongSubName = j["subName"]?.ToString() ?? "",
                        SongLink = "http://www.xiami.com/song/" + j["songId"],

                        ArtistId = j["singerVOs"]?.First?["artistId"].ToString() ?? "",
                        ArtistName = j["singers"]?.ToString() ?? "",
                        ArtistSubName = j["singerVOs"]?.First?["alias"].ToString() ?? "",

                        AlbumId = j["albumId"]?.ToString() ?? "0",
                        AlbumName = j["albumName"]?.ToString() ?? "",
                        AlbumSubName = j["albumSubName"]?.ToString() ?? "",
                        AlbumArtist = j["artistName"]?.ToString() ?? "",

                        Length = j["length"]?.Value<int>() / 1000 ?? 0,
                        BitRate = "",

                        FlacUrl = "",
                        ApeUrl = "",
                        WavUrl = "",
                        SqUrl = "",
                        HqUrl = "",
                        LqUrl = "",
                        CopyUrl = CommonHelper.GetSongUrl("xm", "320", j["songId"].ToString(), "mp3"),

                        SmallPic = j["albumLogo"].ToString().Replace(".jpg", "_1.jpg"),
                        PicUrl = j["albumLogo"].ToString(),

                        LrcUrl = CommonHelper.GetSongUrl("xm", "128", j["songId"].ToString(), "lrc"),
                        TrcUrl = "",
                        KrcUrl = "",

                        MvId = j["mvId"]?.ToString() ?? "",
                        MvHdUrl = "",
                        MvLdUrl = "",

                        Language = "",
                        Company = "",
                        Year = j["gmtCreate"] == null ? "" : CommonHelper.UnixTimestampToDateTime(j["gmtCreate"].Value<long>() / 1000).ToString("yyyy-MM-dd"),
                        Disc = j["cdSerial"].Value<int>(),
                        TrackNum = j["track"].Value<int>(),
                        Type = "xm"
                    };

                    if (j["lyricInfo"] != null)
                    {
                        song.LrcUrl = j["lyricInfo"]["lyricFile"].ToString();
                        if (song.LrcUrl.Contains(".trc"))
                        {
                            song.TrcUrl = song.LrcUrl;
                        }
                    }
                    if (j["lyric"] != null)
                    {
                        song.LrcUrl = j["lyric"].ToString();
                        if (song.LrcUrl.Contains(".trc"))
                        {
                            song.TrcUrl = song.LrcUrl;
                        }
                    }
                    if (j["listenFiles"] != null)
                    {
                        foreach (var jToken in j["listenFiles"])
                        {
                            var songLink = jToken["listenFile"]?.ToString() ?? jToken["url"].ToString();
                            switch (jToken["quality"].ToString())
                            {
                                case "l":
                                    if (string.IsNullOrEmpty(song.BitRate))
                                    {
                                        song.BitRate = "128K";
                                    }
                                    song.LqUrl = songLink;
                                    break;
                                case "h":
                                    if (song.BitRate != "无损")
                                    {
                                        song.BitRate = "320K";
                                    }
                                    song.HqUrl = song.SqUrl = songLink;
                                    break;
                                case "s":
                                    song.BitRate = "无损";
                                    switch (jToken["format"].ToString())
                                    {
                                        case "flac":
                                            song.FlacUrl = songLink;
                                            break;
                                        case "ape":
                                            song.ApeUrl = songLink;
                                            break;
                                        default:
                                            song.WavUrl = songLink;
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in j["purviewRoleVOs"])
                        {
                            if (!item["isExist"].Value<bool>()) continue;
                            switch (item["quality"].ToString())
                            {
                                case "l":
                                    if (string.IsNullOrEmpty(song.BitRate))
                                    {
                                        song.BitRate = "128K";
                                    }
                                    song.LqUrl = CommonHelper.GetSongUrl("xm", "128", song.SongId, "mp3");
                                    break;
                                case "h":
                                    if (song.BitRate != "无损")
                                    {
                                        song.BitRate = "320K";
                                    }
                                    song.HqUrl = song.SqUrl = CommonHelper.GetSongUrl("xm", "320", song.SongId, "mp3");
                                    break;
                                case "s":
                                    song.BitRate = "无损";
                                    song.FlacUrl = CommonHelper.GetSongUrl("xm", "999", song.SongId, "flac");
                                    break;
                            }
                        }
                    }
                    list.Add(song);
                }
                catch (Exception ex)
                {
                    CommonHelper.AddLog("JsonData：" + JsonConvert.SerializeObject(j));
                    CommonHelper.AddLog(ex.ToString());
                }
            }
            return list;
        }

        private static List<SongResult> SearchAlbum(string id)
        {
            var html = GetXmHtml("mtop.alimusic.music.albumservice.getalbumdetail", "\\\"model\\\":{\\\"albumId\\\":" + id + "}");
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                if (json["data"]["data"] == null)
                {
                    return null;
                }
                var data = json["data"]["data"]["albumDetail"]["songs"];
                var year = CommonHelper.UnixTimestampToDateTime(json["data"]["data"]["albumDetail"]["gmtPublish"].Value<long>() / 1000).ToString("yyyy-MM-dd");
                var ar = json["data"]["data"]["albumDetail"]["artistName"].ToString();
                var cmp = json["data"]["data"]["albumDetail"]["company"].ToString();
                var lng = json["data"]["data"]["albumDetail"]["language"].ToString();
                var list = GetListByToken(data);
                if (list == null)
                {
                    return null;
                }
                foreach (var songResult in list)
                {
                    songResult.Year = year;
                    songResult.AlbumArtist = ar;
                    songResult.Company = cmp;
                    songResult.Language = lng;
                }
                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<SongResult> SearchArtist(string id, int page, int size)
        {
            var html = GetXmHtml("mtop.alimusic.music.songservice.getartistsongs",
                "\\\"model\\\":{\\\"artistId\\\":\\\"" + id + "\\\",\\\"pagingVO\\\":{\\\"page\\\":" + page +
                ",\\\"pageSize\\\":" + size + "}}");
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                if (json["data"]["data"]["pagingVO"]["count"].Value<int>() <= 0)
                {
                    return null;
                }
                var data = json["data"]["data"]["songs"];
                return GetListByToken(data);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<SongResult> SearchCollect(string id, int page, int size)
        {
            var html = GetXmHtml("mtop.alimusic.music.list.collectservice.getcollectdetail",
                "\\\"model\\\":{\\\"listId\\\":" + id + ",\\\"isFullTags\\\":false,\\\"pagingVO\\\":{\\\"pageSize\\\":" +
                size + ",\\\"page\\\":" + page + "}}");
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                if (json["data"]["data"]["pagingVO"]["count"].Value<int>() <= 0)
                {
                    return null;
                }
                var data = json["data"]["data"]["collectDetail"]["songs"];
                return GetListByToken(data);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<SongResult> SearchSong(string ids)
        {
            var html = GetXmHtml("mtop.alimusic.music.songservice.getsongs",
                "\\\"model\\\":{\\\"songIds\\\":[" + ids + "]}");
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                var data = json["data"]["data"]["songs"];
                return GetListByToken(data);
            }
            catch (Exception)
            {
                return null;
            }
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
            var list = SearchSong(id);
            if (list == null)
            {
                return null;
            }
            var song = list[0];
            switch (format)
            {
                case "flac":
                    if (!string.IsNullOrEmpty(song.FlacUrl))
                    {
                        return song.FlacUrl;
                    }
                    if (!string.IsNullOrEmpty(song.ApeUrl))
                    {
                        return song.ApeUrl;
                    }
                    return !string.IsNullOrEmpty(song.WavUrl) ? song.WavUrl : null;
                case "lrc":
                    return song.LrcUrl;
                case "jpg":
                    return song.PicUrl;
                default:
                    return quality == "128" ? song.LqUrl : song.SqUrl;
            }
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
            return SearchSong(id);
        }

        public string GetSongUrl(string id, string quality, string format)
        {
            return GetUrl(id,quality,format);
        }

        public List<SongResult> SongSearch(string key, int page, int size)
        {
            return Search(key, page, size);
        }
    }
}
