using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AnyListen.Helper;
using AnyListen.Models;
using Newtonsoft.Json.Linq;

namespace AnyListen.Music
{
    public class WyMusic:IMusic
    {
        #region 加密发送包
        public static string WyNewCookie = "MUSIC_U=8149f85dbe6e500d647b36e8504fce3bd243a48a699bc26e7531174de4cd486e26296e43872d0f6956a6c2c5be5fc4fd0d8f0d281b3ffd12; __remember_me=true; __csrf=26ae61aeed306bf69e4d2ea5edd00c9b;";

        private static string GetEncHtml(string url, string text)
        {
            const string secKey = "a44e542eaac91dce";
            var pad = 16 - text.Length % 16;
            for (var i = 0; i < pad; i++)
            {
                text = text + Convert.ToChar(pad);
            }
            var encText = AesEncrypt(AesEncrypt(text, "0CoJUm6Qyw8W8jud"), secKey);
            const string encSecKey = "411571dca16717d9af5ef1ac97a8d21cb740329890560688b1b624de43f49fdd7702493835141b06ae45f1326e264c98c24ce87199c1a776315e5f25c11056b02dd92791fcc012bff8dd4fc86e37888d5ccc060f7837b836607dbb28bddc703308a0ba67c24c6420dd08eec2b8111067486c907b6e53c027ae1e56c188bc568e";
            var data = new Dictionary<string, string>
            {
                {"params", encText},
                {"encSecKey", encSecKey},
            };
            var html = CommonHelper.PostData(url, data, 0, 0, new Dictionary<string, string>
            {
                {"Cookie", WyNewCookie}
            });
            return html;
        }

        private static string AesEncrypt(string toEncrypt, string key, string iv = "0102030405060708")
        {
            var keyArray = Encoding.UTF8.GetBytes(key);
            var ivArr = Encoding.UTF8.GetBytes(iv);
            var toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
            using (var aesDel = Aes.Create())
            {
                if (aesDel != null)
                {
                    aesDel.IV = ivArr;
                    aesDel.Key = keyArray;
                    aesDel.Mode = CipherMode.CBC;
                    aesDel.Padding = PaddingMode.PKCS7;
                    var cTransform = aesDel.CreateEncryptor();
                    var resultArr = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                    return Convert.ToBase64String(resultArr, 0, resultArr.Length);
                }
                return null;
            }
        }

        #endregion

        public static List<SongResult> Search(string key, int page, int size)
        {
            var text = "{\"s\":\"" + key + "\",\"type\":1,\"offset\":" + (page - 1) * size + ",\"limit\":" + size + ",\"total\":true}";
            var html = GetEncHtml("http://music.163.com/weapi/cloudsearch/get/web?csrf_token=", text);
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                var datas = json["result"]["songs"];
                return GetListByJson(datas);
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static List<SongResult> GetListByJson(JToken jsons)
        {
            if (jsons == null )
            {
                return null;
            }
            var list = new List<SongResult>();
            foreach (var j in jsons)
            {
                try
                {
                    //if (j["privilege"]["st"].ToString() == "-200")
                    //{
                    //    continue;
                    //}
                    var ar = j["ar"].Aggregate("", (current, jToken) => current + jToken["name"].ToString() + ";");
                    var song = new SongResult
                    {
                        SongId = j["id"].ToString(),
                        SongName = j["name"].ToString(),
                        SongSubName = j["alia"].First?.ToString(),
                        SongLink = "http://music.163.com/#/song?id=" + j["id"],

                        ArtistId = j["ar"].First["id"].ToString(),
                        ArtistName = ar.TrimEnd(';'),
                        ArtistSubName = "",

                        AlbumId = j["al"]["id"].ToString(),
                        AlbumName = j["al"]["name"].ToString(),
                        AlbumSubName = j["al"]["alia"]?.First?.ToString(),
                        AlbumArtist = j["ar"].First["name"].ToString(),

                        Length = string.IsNullOrEmpty(j["dt"]?.ToString()) ? 0 : Convert.ToInt32(j["dt"].ToString()) / 1000,
                        BitRate = "128K",

                        FlacUrl = "",
                        ApeUrl = "",
                        WavUrl = "",
                        SqUrl = "",
                        HqUrl = "",
                        LqUrl = "",
                        CopyUrl = "",

                        SmallPic = GetPicById(j["al"]["pic"].ToString(),100),
                        PicUrl = GetPicById(j["al"]["pic"].ToString()),

                        LrcUrl = CommonHelper.GetSongUrl("wy", "320", j["id"].ToString(), "lrc"),
                        TrcUrl = CommonHelper.GetSongUrl("wy", "320", j["id"].ToString(), "trc"),
                        KrcUrl = CommonHelper.GetSongUrl("wy", "320", j["id"].ToString(), "krc"),

                        MvId = j["mv"].ToString(),
                        MvHdUrl = "",
                        MvLdUrl = "",

                        Language = "",
                        Company = "",
                        Year = "",
                        Disc = string.IsNullOrEmpty(j["cd"]?.ToString()) ? 1 : Convert.ToInt32(j["cd"].ToString().Split('/')[0]),
                        TrackNum = string.IsNullOrEmpty(j["no"]?.ToString()) ? 1 : Convert.ToInt32(j["no"].ToString()),
                        Type = "wy"
                    };
                    if (!string.IsNullOrEmpty(song.MvId))
                    {
                        if (song.MvId != "0")
                        {
                            song.MvHdUrl = CommonHelper.GetSongUrl("wy", "hd", song.MvId, "mp4");
                            song.MvLdUrl = CommonHelper.GetSongUrl("wy", "ld", song.MvId, "mp4");
                        }
                    }
                    if (j["privilege"] != null)
                    {
                        var maxBr = j["privilege"]["maxbr"].ToString();
                        switch (maxBr)
                        {
                            case "999000":
                                song.BitRate = "无损";
                                song.FlacUrl = CommonHelper.GetSongUrl("wy", "999", song.SongId, "flac");
                                song.SqUrl = CommonHelper.GetSongUrl("wy", "320", song.SongId, "mp3");
                                song.HqUrl = CommonHelper.GetSongUrl("wy", "160", song.SongId, "mp3");
                                song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                                song.CopyUrl = song.SqUrl;
                                break;
                            case "320000":
                            case "192000":
                            case "190000":
                                song.BitRate = "320K";
                                song.SqUrl = CommonHelper.GetSongUrl("wy", "320", song.SongId, "mp3");
                                song.HqUrl = CommonHelper.GetSongUrl("wy", "160", song.SongId, "mp3");
                                song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                                song.CopyUrl = song.SqUrl;
                                break;
                            case "160000":
                                song.BitRate = "192K";
                                song.HqUrl = CommonHelper.GetSongUrl("wy", "160", song.SongId, "mp3");
                                song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                                song.CopyUrl = song.HqUrl;
                                break;
                            default:
                                song.BitRate = "128K";
                                song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                                song.CopyUrl = song.LqUrl;
                                break;
                        }
                        //if (j["fee"].ToString() == "4" || j["privilege"]["st"].ToString() != "0")
                        //{
                        //    if (song.BitRate == "无损")
                        //    {
                        //        song.BitRate = "320K";
                        //        song.FlacUrl = "";
                        //    }
                        //}
                    }
                    else
                    {
                        if (j["l"] != null & j["l"].ToString().Contains("fid"))
                        {
                            song.BitRate = "128K";
                            song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                            song.CopyUrl = song.LqUrl;
                        }
                        if (j["m"] != null & j["m"].ToString().Contains("fid"))
                        {
                            song.BitRate = j["m"]["br"].Value<int>() / 1000 + "K";
                            song.HqUrl = CommonHelper.GetSongUrl("wy", "160", song.SongId, "mp3");
                            song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                            song.CopyUrl = song.HqUrl;
                        }
                        if (j["h"]!= null & j["h"].ToString().Contains("fid"))
                        {
                            song.BitRate = j["h"]["br"].Value<int>() / 1000 + "K";
                            song.SqUrl = CommonHelper.GetSongUrl("wy", "320", song.SongId, "mp3");
                            song.HqUrl = CommonHelper.GetSongUrl("wy", "160", song.SongId, "mp3");
                            song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                            song.CopyUrl = song.SqUrl;
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

        private static string GetPicById(string id, int quality = 0)
        {
            var encryptPath = EncryptId(id);
            return $"http://p4.music.126.net/{encryptPath}/{id}.jpg" +
                   (quality == 0 ? "" : $"?param={quality}y{quality}");
        }

        private static SongResult SearchSingle(string id)
        {
            var text = new Dictionary<string, string> {{"c", "[{\"id\":\"" + id + "\"}]"}};
            var html = CommonHelper.PostData("http://music.163.com/api/v3/song/detail", text, 0, 0,
                new Dictionary<string, string>
                {
                    {"Cookie", WyNewCookie}
                });
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            var json = JObject.Parse(html);
            return GetListByJToken(json)?[0];
        }

        private static List<SongResult> GetListByJToken(JObject json, bool isPlayList = false)
        {
            var datas = isPlayList ? json["playlist"]["tracks"] : json["songs"];
            var list = new List<SongResult>();
            var index = -1;
            foreach (var j in datas)
            {
                try
                {
                    index++;
                    //if (json["privileges"][index]["st"].ToString() == "-200")
                    //{
                    //    continue;
                    //}
                    var ar = j["ar"].Aggregate("", (current, jToken) => current + (jToken["name"].ToString() + ";"));
                    var song = new SongResult
                    {
                        SongId = j["id"].ToString(),
                        SongName = j["name"].ToString(),
                        SongSubName = j["alia"].First?.ToString(),
                        SongLink = "http://music.163.com/#/song?id=" + j["id"],

                        ArtistId = j["ar"].First["id"].ToString(),
                        ArtistName = ar.TrimEnd(';'),
                        ArtistSubName = "",

                        AlbumId = j["al"]["id"].ToString(),
                        AlbumName = j["al"]["name"].ToString(),
                        AlbumSubName = j["al"]["alia"]?.First?.ToString(),
                        AlbumArtist = j["ar"].First["name"].ToString(),

                        Length = Convert.ToInt32(j["dt"].ToString()) / 1000,
                        BitRate = "128K",

                        FlacUrl = "",
                        ApeUrl = "",
                        WavUrl = "",
                        SqUrl = "",
                        HqUrl = "",
                        LqUrl = "",
                        CopyUrl = "",

                        SmallPic = GetPicById(j["al"]["pic"].ToString(), 100),
                        PicUrl = GetPicById(j["al"]["pic"].ToString()),

                        LrcUrl = CommonHelper.GetSongUrl("wy", "320", j["id"].ToString(), "lrc"),
                        TrcUrl = CommonHelper.GetSongUrl("wy", "320", j["id"].ToString(), "trc"),
                        KrcUrl = CommonHelper.GetSongUrl("wy", "320", j["id"].ToString(), "krc"),

                        MvId = j["mv"].ToString(),
                        MvHdUrl = "",
                        MvLdUrl = "",

                        Language = "",
                        Company = "",
                        Year = "",
                        Disc = string.IsNullOrEmpty(j["cd"]?.ToString()) ? 1 : Convert.ToInt32(j["cd"].ToString().Split('/')[0]),
                        TrackNum = string.IsNullOrEmpty(j["no"]?.ToString()) ? 1 : Convert.ToInt32(j["no"].ToString()),
                        Type = "wy"
                    };
                    if (!string.IsNullOrEmpty(song.MvId))
                    {
                        if (song.MvId != "0")
                        {
                            song.MvHdUrl = CommonHelper.GetSongUrl("wy", "hd", song.MvId, "mp4");
                            song.MvLdUrl = CommonHelper.GetSongUrl("wy", "ld", song.MvId, "mp4");
                        }
                    }
                    var maxBr = json["privileges"][index]["maxbr"].ToString();
                    if (maxBr == "999000")
                    {
                        song.BitRate = "无损";
                        song.FlacUrl = CommonHelper.GetSongUrl("wy", "999", song.SongId, "flac");
                        song.SqUrl = CommonHelper.GetSongUrl("wy", "320", song.SongId, "mp3");
                        song.HqUrl = CommonHelper.GetSongUrl("wy", "160", song.SongId, "mp3");
                        song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                        song.CopyUrl = song.SqUrl;
                    }
                    else if (maxBr == "320000")
                    {
                        song.BitRate = "320K";
                        song.SqUrl = CommonHelper.GetSongUrl("wy", "320", song.SongId, "mp3");
                        song.HqUrl = CommonHelper.GetSongUrl("wy", "160", song.SongId, "mp3");
                        song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                        song.CopyUrl = song.SqUrl;
                    }
                    else if (maxBr == "160000")
                    {
                        song.BitRate = "192K";
                        song.HqUrl = CommonHelper.GetSongUrl("wy", "160", song.SongId, "mp3");
                        song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                        song.CopyUrl = song.HqUrl;
                    }
                    else
                    {
                        song.BitRate = "128K";
                        song.LqUrl = CommonHelper.GetSongUrl("wy", "128", song.SongId, "mp3");
                        song.CopyUrl = song.LqUrl;
                    }
                    //if (j["fee"].ToString() == "4" || json["privileges"][index]["st"].ToString() != "0")
                    //{
                    //    if (song.BitRate == "无损")
                    //    {
                    //        song.BitRate = "320K";
                    //        song.FlacUrl = "";
                    //    }
                    //}
                    list.Add(song);
                }
                catch (Exception ex)
                {
                    CommonHelper.AddLog(j.ToString());
                    CommonHelper.AddLog(ex.ToString());
                }
            }
            return list;
        }

        private static List<SongResult> SearchAlbum(string id)
        {
            var text = "{\"id\":\"" + id + "\"}";
            var html = GetEncHtml("http://music.163.com/weapi/v1/album/" + id + "?csrf_token=", text);
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                var ar = json["album"]["artist"]["name"].ToString();
                var pic = json["album"]["picUrl"].ToString();
                var cmp = json["album"]["company"].ToString();
                var year =
                    CommonHelper.UnixTimestampToDateTime(Convert.ToInt64(json["album"]["publishTime"].ToString()) / 1000)
                        .ToString("yyyy-MM-dd");
                var datas = json["songs"];
                var list = GetListByJson(datas);
                foreach (var s in list)
                {
                    s.AlbumArtist = ar;
                    s.PicUrl = pic;
                    s.Company = cmp;
                    s.Year = year;
                }
                return list;
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static List<SongResult> SearchArtist(string id)
        {
            var text = "{\"id\":\"" + id + "\"}";
            var html = GetEncHtml("http://music.163.com/weapi/v1/artist/" + id + "?csrf_token=", text);
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                var datas = json["hotSongs"];
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
            var text = "{\"id\":\"" + id + "\",\"n\":" + size + ",\"offset\":" + (page - 1) * size + ",\"limit\":" + size +
                       ",\"total\":true}";
            var html = GetEncHtml("http://music.163.com/weapi/v3/playlist/detail?csrf_token=", text);
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                return GetListByJToken(json, true);
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static string GetUrl(string id, string quality, string format)
        {
            var text = "";
            switch (format.ToLower())
            {
                case "mp3":
                    if (quality == "320")
                    {
                        text = GetPlayUrl(id, "320000");
                    }
                    else if (quality == "160")
                    {
                        text = GetPlayUrl(id, "192000");
                    }
                    else
                    {
                        text = GetPlayUrl(id, "128000");
                    }
                    break;
                case "flac":
                    text = GetPlayUrl(id, "999000");
                    break;
                case "mp4":
                    text = GetMvUrl(id, quality.ToLower());
                    break;
                case "lrc":
                    text = GetLrc(id);
                    break;
                case "krc":
                    text = GetLrc(id, format.ToLower());
                    break;
                case "trc":
                    text = GetLrc(id, format.ToLower());
                    break;
                case "jpg":
                    text = GetPic(id);
                    break;
            }
            return text;
        }

        #region 新版API
        private static string GetPlayUrl(string id, string quality)
        {
            var text = "{\"ids\":[\"" + id + "\"],\"br\":" + quality + ",\"csrf_token\":\"\"}";
            var html = GetEncHtml("http://music.163.com/weapi/song/enhance/player/url?csrf_token=", text);
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            var json = JObject.Parse(html);
            
            var link = json["data"].First["url"].ToString();
            if (string.IsNullOrEmpty(link) || link == "null")
            {
                link = GetUrlFromThird(id, quality);
            }
            return link;
        }

        private static string GetUrlFromThird(string id, string quality)
        {
            var singleSong = SearchSingle(id);
            var songListQq = AnyListen.GetMusic("qq")
                .SongSearch(singleSong.ArtistName + "-" + singleSong.SongName, 1, 30);
            SongResult song = null;
            if (songListQq != null)
            {
                song = songListQq.FirstOrDefault(t => CommonHelper.CompareStr(t.SongName, singleSong.SongName) &&
                                                     (CommonHelper.CompareStr(t.ArtistName, singleSong.ArtistName) || CommonHelper.CompareStr(t.AlbumName, singleSong.AlbumName)));
            }
            if (song == null)
            {
                var songList = AnyListen.GetMusic("xm")
                    .SongSearch(singleSong.ArtistName + "-" + singleSong.SongName, 1, 30);
                if (songListQq != null)
                    song = songList.FirstOrDefault(t => CommonHelper.CompareStr(t.SongName, singleSong.SongName) &&
                                                        (CommonHelper.CompareStr(t.ArtistName, singleSong.ArtistName) ||
                                                         CommonHelper.CompareStr(t.AlbumName, singleSong.AlbumName))) ??
                           songListQq.FirstOrDefault(t => CommonHelper.CompareStr(t.SongName, singleSong.SongName));
                if (song == null)
                {
                    song = songList.FirstOrDefault(t => CommonHelper.CompareStr(t.SongName, singleSong.SongName));
                }
            }
            if (song == null)
            {
                return "";
            }
            if (quality == "999000")
            {
                if (!string.IsNullOrEmpty(song.FlacUrl))
                {
                    return song.FlacUrl;
                }
                if (!string.IsNullOrEmpty(song.ApeUrl))
                {
                    return song.ApeUrl;
                }
                if (!string.IsNullOrEmpty(song.WavUrl))
                {
                    return song.WavUrl;
                }
                if (!string.IsNullOrEmpty(song.SqUrl))
                {
                    return song.SqUrl;
                }
                if (!string.IsNullOrEmpty(song.HqUrl))
                {
                    return song.HqUrl;
                }
            }
            else if (quality == "320000" || quality == "192000")
            {
                if (!string.IsNullOrEmpty(song.SqUrl))
                {
                    return song.SqUrl;
                }
            }
            return song.LqUrl;
        }

        #endregion

        #region 旧版API

        private const string WyCookie =
            "__remember_me=true; MUSIC_U=c53e8fa763502bebd61488daaeb3d336fa0bd216452aa999b29453cbd46c07c3cbf122d59fa1ed6a2; __csrf=ae0c8a4af8e070d4c68d2c631501052c; os=WP; appver=1.2.2; deviceId=PByT0lnU4lzRlgzmqYWxZCbHHoE=; osver=Microsoft+Windows+NT+10.0.13067.0";

        //private static string GetLostUrl(string id, string quality)
        //{
        //    var singleSong = SearchSingle(id);
        //    var html = CommonHelper.GetHtmlContent("http://music.163.com/api/album/" + singleSong.AlbumId, 4,
        //        new Dictionary<string, string>
        //        {
        //            {"Cookie", WyCookie}
        //        });
        //    if (string.IsNullOrEmpty(html) || html == "null")
        //    {
        //        return null;
        //    }
        //    var json = JObject.Parse(html);
        //    var datas = json["album"]["songs"];
        //    var link = "";
        //    foreach (JToken song in datas.Where(song => song["id"].ToString() == id))
        //    {
        //        switch (quality)
        //        {
        //            case "320000":
        //                string dfsId;
        //                if (song["hMusic"].Type == JTokenType.Null)
        //                {
        //                    if (song["mMusic"].Type == JTokenType.Null)
        //                    {
        //                        return song["mp3Url"]?.ToString();
        //                    }
        //                    dfsId = song["mMusic"]["dfsId"]?.ToString();
        //                }
        //                else
        //                {
        //                    dfsId = song["hMusic"]["dfsId"]?.ToString();
        //                }
        //                link = GetUrlBySid(dfsId);
        //                break;
        //            case "192000":
        //                link = song["mMusic"].Type == JTokenType.Null ? song["mp3Url"]?.ToString() : GetUrlBySid(song["mMusic"]["dfsId"]?.ToString());
        //                break;
        //            default:
        //                link = song["mp3Url"]?.ToString();
        //                break;
        //        }
        //    }
        //    //return string.IsNullOrEmpty(link) ? GetLostUrlByPid(id, quality) : link;
        //    return link;
        //}

        //private static string GetLostUrlByPid(string id, string quality)
        //{
        //    var text = "{\"songid\":\"" + id + "\",\"offset\":0,\"limit\":10,\"total\":true}";
        //    var html = GetEncHtml("http://music.163.com/weapi/discovery/simiPlaylist", text);
        //    if (string.IsNullOrEmpty(html) || html == "null")
        //    {
        //        return null;
        //    }
        //    var json = JObject.Parse(html);
        //    foreach (JToken jToken in json["playlists"])
        //    {
        //        var pid = jToken["id"].ToString();
        //        var url = "http://music.163.com/api/playlist/detail?id=" + pid;
        //        html = CommonHelper.GetHtmlContent(url, WyNewCookie);
        //        if (string.IsNullOrEmpty(html))
        //        {
        //            return null;
        //        }
        //        var total = JObject.Parse(html);
        //        var datas = total["result"]["tracks"];
        //        var song = datas.SingleOrDefault(t => t["id"].ToString() == id);
        //        if (song == null)
        //        {
        //            continue;
        //        }
        //        switch (quality)
        //        {
        //            case "320000":
        //                string dfsId;
        //                if (song["hMusic"].Type == JTokenType.Null)
        //                {
        //                    if (song["mMusic"].Type == JTokenType.Null)
        //                    {
        //                        return song["mp3Url"]?.ToString();
        //                    }
        //                    dfsId = song["mMusic"]["dfsId"]?.ToString();
        //                }
        //                else
        //                {
        //                    dfsId = song["hMusic"]["dfsId"]?.ToString();
        //                }
        //                return GetUrlBySid(dfsId);
        //            case "192000":
        //                if (song["mMusic"].Type == JTokenType.Null)
        //                {
        //                    return song["mp3Url"]?.ToString();
        //                }
        //                return GetUrlBySid(song["mMusic"]["dfsId"]?.ToString());
        //            default:
        //                return song["mp3Url"]?.ToString();
        //        }
        //    }
        //    return null;
        //}

        //private static string GetUrlBySid(string dfsId)
        //{
        //    if (dfsId == "0")
        //    {
        //        return "";
        //    }
        //    var encryptPath = EncryptId(dfsId);
        //    var url = $"http://p2.music.126.net/{encryptPath}/{dfsId}.mp3";
        //    return url;
        //}

        private static string GetPic(string id)
        {
            var html = CommonHelper.GetHtmlContent("http://music.163.com/api/song/detail/?ids=%5B" + id + "%5D", 0,
                new Dictionary<string, string>
                {
                    {"Cookie", WyCookie}
                });
            return string.IsNullOrEmpty(html) ? null : JObject.Parse(html)["songs"].First["album"]["blurPicUrl"].ToString();
        }

        private static string GetLrc(string sid, string type = "lrc")
        {
            var text = "{\"id\":" + sid + ",\"lv\":-1,\"tv\":-1,\"csrf_token\":\"\"}";
            var html = GetEncHtml("http://music.163.com/weapi/song/lyric?csrf_token=", text);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            if (html.Contains("uncollected"))
            {
                return null;
            }
            var json = JObject.Parse(html);
            text = "";
            switch (type)
            {
                case "krc":
                    if (json["klyric"]?["lyric"] != null)
                    {
                        text = json["klyric"]["lyric"].Value<string>();
                    }
                    break;
                case "trc":
                    if (json["tlyric"]?["lyric"] != null)
                    {
                        text = json["tlyric"]["lyric"].Value<string>();
                    }
                    break;
                default:
                    if (json["lrc"]?["lyric"] != null)
                    {
                        text = json["lrc"]["lyric"].Value<string>();
                    }
                    break;
            }
            return text;
        }

        private static string GetMvUrl(string mid, string quality)
        {
            var url = "http://music.163.com/api/song/mv?id=" + mid + "&type=mp4";
            var html = CommonHelper.GetHtmlContent(url, 4, new Dictionary<string, string>
            {
                {"Cookie", WyCookie}
            });
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            var json = JObject.Parse(html);
            var dic = new Dictionary<int, string>();
            var max = 0;
            foreach (JToken jToken in json["mvs"])
            {
                if (jToken["br"].Value<int>() > max)
                {
                    max = jToken["br"].Value<int>();
                }
                dic.Add(jToken["br"].Value<int>(), jToken["mvurl"].ToString());
            }
            if (quality != "ld")
            {
                return dic[max];
            }
            switch (max)
            {
                case 1080:
                    return dic[720];
                case 720:
                    return dic[480];
                case 480:
                    return dic.ContainsKey(320) ? dic[320] : dic[240];
                default:
                    return dic[240];
            }
        }

        private static string EncryptId(string dfsId)
        {
            var encoding = new ASCIIEncoding();
            var bytes1 = encoding.GetBytes("3go8&$8*3*3h0k(2)2");
            var bytes2 = encoding.GetBytes(dfsId);
            for (var i = 0; i < bytes2.Length; i++)
                bytes2[i] = (byte)(bytes2[i] ^ bytes1[i % bytes1.Length]);
            using (var md5Hash = MD5.Create())
            {
                var res = Convert.ToBase64String(md5Hash.ComputeHash(bytes2));
                res = res.Replace('/', '_').Replace('+', '-');
                return res;
            }
        }
        #endregion

        public List<SongResult> SongSearch(string key, int page, int size)
        {
            return Search(key,page,size);
        }

        public List<SongResult> AlbumSearch(string id)
        {
            return SearchAlbum(id);
        }

        public List<SongResult> ArtistSearch(string id, int page, int size)
        {
            return SearchArtist(id);
        }

        public List<SongResult> CollectSearch(string id, int page, int size)
        {
            return SearchCollect(id, page, size);
        }

        public List<SongResult> GetSingleSong(string id)
        {
            return new List<SongResult> { SearchSingle(id) };
        }

        public string GetSongUrl(string id, string quality, string format)
        {
            return GetUrl(id,quality,format);
        }
    }
}