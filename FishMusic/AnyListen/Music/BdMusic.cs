using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AnyListen.Helper;
using AnyListen.Models;
using Newtonsoft.Json.Linq;

namespace AnyListen.Music
{
    public class BdMusic:IMusic
    {
        public static List<SongResult> SearchSong(string key, int page, int size)
        {
            try
            {
                var url =
                "http://tingapi.ting.baidu.com/v1/restserver/ting?from=android&version=5.6.5.6&method=baidu.ting.search.merge&format=json&query=" +
                key + "&page_no=" + page + "&page_size=" + size + "&type=0&data_source=0&use_cluster=1";
                var html = CommonHelper.GetHtmlContent(url);
                if (string.IsNullOrEmpty(html))
                {
                    return null;
                }
                var json = JObject.Parse(html);
                if (json["error_code"].ToString() != "22000")
                {
                    return null;
                }
                if (json["result"]["song_info"] == null)
                {
                    return null;
                }
                var datas = json["result"]["song_info"]["song_list"];
                return GetListByJson(datas);
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static List<SongResult> GetListByJson(JToken songs)
        {
            if (songs == null)
            {
                return null;
            }
            var list = new List<SongResult>();
            foreach (JToken j in songs)
            {
                var song = new SongResult
                {
                    SongId = j["song_id"].ToString(),
                    SongName = j["title"].ToString(),
                    SongSubName = j["info"]?.ToString(),
                    SongLink = "http://music.baidu.com/song/" + j["song_id"],

                    ArtistId = j["ting_uid"].ToString(),
                    ArtistName = j["author"].ToString(),
                    ArtistSubName = "",

                    AlbumId = j["album_id"].ToString(),
                    AlbumName = j["album_title"].ToString(),
                    AlbumSubName = "",
                    AlbumArtist = j["author"].ToString(),

                    Length = j["file_duration"] == null ? 0 : Convert.ToInt32(j["file_duration"].ToString()),
                    BitRate = "",

                    FlacUrl = "",
                    ApeUrl = "",
                    WavUrl = "",
                    SqUrl = "",
                    HqUrl = "",
                    LqUrl = "",
                    CopyUrl = "",

                    SmallPic = "",
                    PicUrl = "",

                    LrcUrl = j["lrclink"]?.ToString() ?? CommonHelper.GetSongUrl("bd", "128", j["song_id"].ToString(), "lrc"),
                    TrcUrl = "",
                    KrcUrl = "",

                    MvId = "",
                    MvHdUrl = "",
                    MvLdUrl = "",

                    Language = j["language"]?.ToString(),
                    Company = "",
                    Year = j["publishtime"]?.ToString(),
                    Disc = 1,
                    TrackNum = j["album_no"]==null ? 0 : Convert.ToInt32(j["album_no"].ToString()),
                    Type = "bd"
                };

                if (song.ArtistId.Contains(","))
                {
                    song.ArtistId = song.ArtistId.Split(',')[0].Trim();
                }
                if (song.AlbumArtist.Contains(","))
                {
                    song.AlbumArtist = song.AlbumArtist.Split(',')[0].Trim();
                }
                var rate = j["all_rate"].ToString();
                song.BitRate = "128K";
                song.LqUrl = CommonHelper.GetSongUrl("bd", "128", song.SongId, "mp3");
                if (rate.Contains("192"))
                {
                    song.BitRate = "192K";
                    song.HqUrl = CommonHelper.GetSongUrl("bd", "192", song.SongId, "mp3");
                }
                if (rate.Contains("256"))
                {
                    song.BitRate = "256K";
                    song.HqUrl = CommonHelper.GetSongUrl("bd", "256", song.SongId, "mp3");
                }
                if (rate.Contains("320"))
                {
                    song.BitRate = "320K";
                    song.SqUrl = CommonHelper.GetSongUrl("bd", "320", song.SongId, "mp3");
                }
                if (rate.Contains("flac"))
                {
                    song.BitRate = "无损";
                    song.FlacUrl = CommonHelper.GetSongUrl("bd", "2000", song.SongId, "flac");
                }
                song.CopyUrl = CommonHelper.GetSongUrl("bd", "320", song.SongId, "mp3");
                if (j["has_mv"].ToString() == "1")
                {
                    song.MvHdUrl = CommonHelper.GetSongUrl("bd", "hd", song.SongId, "flv");
                    song.MvLdUrl = CommonHelper.GetSongUrl("bd", "ld", song.SongId, "flv");
                }
                song.SmallPic = string.IsNullOrEmpty(j["pic_small"]?.ToString()) ? CommonHelper.GetSongUrl("bd", "low", song.SongId, "jpg") : j["pic_small"]?.ToString();
                song.PicUrl = CommonHelper.GetSongUrl("bd", "high", song.SongId, "jpg");
                list.Add(song);
            }
            return list;
        }

        private static List<SongResult> SearchAlbum(string id)
        {
            try
            {
                var url =
                "http://tingapi.ting.baidu.com/v1/restserver/ting?from=android&version=5.6.5.6&method=baidu.ting.album.getAlbumInfo&format=json&album_id=" + id;
                var html = CommonHelper.GetHtmlContent(url);
                if (string.IsNullOrEmpty(html))
                {
                    return null;
                }
                if (html.Contains("error_code"))
                {
                    return null;
                }
                var json = JObject.Parse(html);
                var datas = json["songlist"];
                var year = json["albumInfo"]["publishtime"].ToString();
                var cmp = json["albumInfo"]["publishcompany"].ToString();
                var lug = json["albumInfo"]["language"].ToString();
                var ar = json["albumInfo"]["author"].ToString();
                var smallPic = json["albumInfo"]["pic_small"].ToString();
                string pic;
                if (!string.IsNullOrEmpty(json["albumInfo"]["pic_s1000"].ToString()))
                {
                    pic = json["albumInfo"]["pic_s1000"].ToString();
                }
                else if (!string.IsNullOrEmpty(json["albumInfo"]["pic_s500"].ToString()))
                {
                    pic = json["albumInfo"]["pic_s500"].ToString();
                }
                else
                {
                    pic = json["albumInfo"]["pic_radio"].ToString();
                }
                var list = GetListByJson(datas);
                var index = 0;
                foreach (var r in list)
                {
                    index++;
                    r.TrackNum = index;
                    r.Year = year;
                    r.Company = cmp;
                    r.Language = lug;
                    r.AlbumArtist = ar;
                    r.PicUrl = pic;
                    r.SmallPic = smallPic;
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
            try
            {
                var url =
                "http://tingapi.ting.baidu.com/v1/restserver/ting?from=android&version=5.6.5.6&method=baidu.ting.artist.getSongList&format=json&order=2&tinguid=" +
                id + "&offset=" + (page - 1) * size + "&limits=" + size;
                var html = CommonHelper.GetHtmlContent(url);
                if (string.IsNullOrEmpty(html))
                {
                    return null;
                }
                var json = JObject.Parse(html);
                if (json["error_code"].ToString() != "22000")
                {
                    return null;
                }
                var datas = json["songlist"];
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
            var url = "http://tingapi.ting.baidu.com/v1/restserver/ting?from=android&version=5.6.5.6&method=baidu.ting.diy.gedanInfo&format=json&listid=" + id;
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            var json = JObject.Parse(html);
            if (json["error_code"].ToString() != "22000")
            {
                return null;
            }
            var datas = json["content"];
            return GetListByJson(datas);
        }

        private static SongResult SearchSingle(string id)
        {
            var text = "songid=" + id + "&ts=1458575606";
            var url = "http://tingapi.ting.baidu.com/v1/restserver/ting?method=baidu.ting.song.getInfos&format=json&from=wp7&version=1.0.0&"
                      + text + "&e=" + EncryptForTingApi(text);
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            try
            {
                if (html.Contains("buy_url\":"))
                {
                    return GetPaySong(id);
                }
                var json = JObject.Parse(html);
                if (json["error_code"].ToString() != "22000")
                {
                    return null;
                }
                var match = "[" + json["songinfo"] + "]";
                var datas = JToken.Parse(match.Trim());
                var list = GetListByJson(datas);
                var song = list[0];
                var links = json["songurl"]["url"];
                foreach (JToken token in links)
                {
                    var fileBitrate = token["file_bitrate"].ToString();
                    var link = token["file_link"].ToString();
                    if (string.IsNullOrEmpty(link))
                    {
                        continue;
                    }
                    switch (fileBitrate)
                    {
                        case "128":
                            song.LqUrl = link;
                            break;
                        case "192":
                            song.HqUrl = link;
                            break;
                        case "256":
                            song.HqUrl = link;
                            break;
                        case "320":
                            song.SqUrl = link;
                            break;
                        case "flac":
                            break;
                    }
                }
                var j = json["songinfo"];
                song.SmallPic = j["pic_small"]?.ToString();
                if (!string.IsNullOrEmpty(j["pic_s1000"]?.ToString()))
                {
                    song.PicUrl = j["pic_s1000"].ToString();
                }
                else if (!string.IsNullOrEmpty(j["pic_s500"]?.ToString()))
                {
                    song.PicUrl = j["pic_s500"].ToString();
                }
                else if (!string.IsNullOrEmpty(j["pic_huge"]?.ToString()))
                {
                    song.PicUrl = j["pic_huge"].ToString();
                }
                else
                {
                    song.PicUrl = j["pic_radio"]?.ToString();
                }
                return song;
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static SongResult GetPaySong(string id)
        {
            var url = "http://tingapi.ting.baidu.com/v1/restserver/ting?from=webapp_music&method=baidu.ting.song.baseInfos&song_id=" + id;
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            var json = JObject.Parse(html);
            if (json["error_code"].ToString() != "22000")
            {
                return null;
            }
            var datas = json["result"]["items"];
            var list = GetListByJson(datas);
            var song = list?[0];
            if (song == null)
            {
                return null;
            }
            var j = json["result"]["items"].First;
            song.SmallPic = j["pic_small"]?.ToString();
            if (!string.IsNullOrEmpty(j["pic_s1000"]?.ToString()))
            {
                song.PicUrl = j["pic_s1000"].ToString();
            }
            else if (!string.IsNullOrEmpty(j["pic_s500"]?.ToString()))
            {
                song.PicUrl = j["pic_s500"].ToString();
            }
            else if (!string.IsNullOrEmpty(j["pic_huge"]?.ToString()))
            {
                song.PicUrl = j["pic_huge"].ToString();
            }
            else
            {
                song.PicUrl = j["pic_radio"]?.ToString();
            }
            return song;
        }

        private static string GetUrl(string id, string quality, string format)
        {
            
            switch (format)
            {
                case "flv":
                    return GetMvUrl(id, quality);
                case "flac":
                    var html = CommonHelper.GetHtmlContent("http://music.taihe.com/data/music/fmlink?rate=999&type=flac&songIds=" + id);
                    if (string.IsNullOrEmpty(html))
                    {
                        return null;
                    }
                    return
                        Regex.Match(html, @"(?<=songLink"":"")[^""]+(?="",""showLink"":""[\s\S]*?"",""format"":""flac"")")
                            .Value.Replace("\\", "");
                    //var text = "songid=" + id + "&ts=1458575606&dt=flac&mul=0";
                    //var ul = "http://tingapi.ting.baidu.com/v1/restserver/ting?method=baidu.ting.song.down&format=json&from=wp7&version=1.0.0&"
                    //          + text + "&e=" + EncryptForTingApi(text);
                    //var html = CommonHelper.GetHtmlContent(ul, 0 , new Dictionary<string, string>()
                    //{
                    //    {"Cookie","PSTM=1488983418; BIDUPSID=F46C68717B695B7473F76CBB579DC252; BAIDUCUID=++; __cfduid=d02b7e3d060fe7bbbf53c4bd7ebee064a1494310009; BAIDUID=3CCCE7960C331D0A4D00F071ABBD92AF:FG=1; MCITY=-315%3A; app_vip=show; BDUSS=mFkb0RMRFVzOERudFYwUGV0YUZiOTVDOWFpbEpCTFFyMmViSmU3NHZ5TUlOc0ZaSVFBQUFBJCQAAAAAAAAAAAEAAACS9~cTc2hlbGhlcgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAipmVkIqZlZT; userid=335017874; token_=131820370E0E080D0C0A050A091503242550264746afbc51d9e28f10a7cfed8d; H_PS_PSSID=1437_21115_18560_20241_20929" }
                    //});
                    //if (string.IsNullOrEmpty(html))
                    //{
                    //    return null;
                    //}
                    //return Regex.Match(html, @"(?<=file_link"":"")[^""]+").Value.Replace("\\", "");

            }
            var song = SearchSingle(id);
            if (song == null)
            {
                return "";
            }
            switch (format)
            {
                case "lrc":
                    return song.LrcUrl;
                case "jpg":
                    if (quality == "high")
                    {
                        return string.IsNullOrEmpty(song.PicUrl) ? "https://user-gold-cdn.xitu.io/2018/7/2/16459c1e94f61f29" : song.PicUrl;
                    }
                    return string.IsNullOrEmpty(song.SmallPic) ? (string.IsNullOrEmpty(song.PicUrl) ? "https://user-gold-cdn.xitu.io/2018/7/2/16459c1e94f61f29" : song.PicUrl) : song.SmallPic;
            }
            string url;
            switch (quality)
            {
                case "320":
                    if (string.IsNullOrEmpty(song.SqUrl))
                    {
                        url = string.IsNullOrEmpty(song.HqUrl) ? song.LqUrl : song.HqUrl;
                    }
                    else
                    {
                        url = song.SqUrl;
                    }
                    break;
                case "192":
                    url = string.IsNullOrEmpty(song.HqUrl) ? song.LqUrl : song.HqUrl;
                    break;
                default:
                    url = song.LqUrl;
                    break;
            }
            if (string.IsNullOrEmpty(url))
            {
                url = GetPayUrl(id);
            }
            try
            {
                return url.Replace("http://yinyueshiting.baidu.com", "https://ss0.bdstatic.com/y0s1hSulBw92lNKgpU_Z2jR7b2w6buu");
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetPayUrl(string id)
        {
            var html =
                    CommonHelper.GetHtmlContent("http://music.baidu.com/data/music/fmlink?songIds=" + id +
                                                "&type=mp3&rate=320");
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            var json = JObject.Parse(html);
            return json["errorCode"].ToString() != "22000" ? null : json["data"]["songList"].First["songLink"].ToString();
        }

        public List<SongResult> SongSearch(string key, int page, int size)
        {
            return SearchSong(key, page, size);
        }

        public List<SongResult> AlbumSearch(string id)
        {
            return SearchAlbum(id);
        }

        public List<SongResult> ArtistSearch(string id, int page, int size)
        {
            return SearchArtist(id,page,size);
        }

        public List<SongResult> CollectSearch(string id, int page, int size)
        {
            return SearchCollect(id);
        }

        public List<SongResult> GetSingleSong(string id)
        {
            return new List<SongResult> { SearchSingle(id) };
        }

        public string GetSongUrl(string id, string quality, string format)
        {
            return GetUrl(id, quality, format);
        }

        private static string GetMvUrl(string id, string quality)
        {
            var url =
                "http://tingapi.ting.baidu.com/v1/restserver/ting?from=android&version=5.6.5.6&provider=11%2C12&method=baidu.ting.mv.playMV&format=json&song_id=" +
                id + "&definition=0";
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }
            var json = JObject.Parse(html);
            if (json["error_code"].ToString() != "22000")
            {
                return "";
            }
            var videoId = json["result"]["video_info"]["sourcepath"].ToString();
            videoId = Regex.Match(videoId, @"(?<=video/)\d+").Value;
            if (string.IsNullOrEmpty(videoId))
            {
                if (json["result"]["video_info"]["sourcepath"].ToString().Contains("iqiyi.com"))
                {
                    return GetAqyUrl(json, quality);
                }
                return null;
            }
            url = "http://www.yinyuetai.com/api/info/get-video-urls?videoId=" + videoId;
            html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }
            json = JObject.Parse(html);
            if (json["error"].ToString().ToLower() != "false")
            {
                return "";
            }
            if (quality == "hd")
            {
                if (html.Contains("heVideoUrl"))
                {
                    return json["heVideoUrl"].ToString();
                }
                if (html.Contains("hdVideoUrl"))
                {
                    return json["hdVideoUrl"].ToString();
                }
                if (html.Contains("hcVideoUrl"))
                {
                    return json["hcVideoUrl"].ToString();
                }
            }
            else
            {
                if (html.Contains("hdVideoUrl"))
                {
                    return json["hdVideoUrl"].ToString();
                }
                if (html.Contains("hcVideoUrl"))
                {
                    return json["hcVideoUrl"].ToString();
                }
            }
            return "";
        }

        private static string GetAqyUrl(JObject json, string quality)
        {
            var link = json["result"]["files"].First["file_link"].ToString();
            var reg = Regex.Match(link, @"(?<=vid=)(\w+)(?:.tvId=)(\d+)");
            if (reg.Groups.Count != 3)
            {
                return null;
            }
            var videoId = reg.Groups[1].Value;
            var tvId = reg.Groups[2].Value;
            var html = CommonHelper.GetHtmlContent("http://cache.video.qiyi.com/vp/" + tvId + "/" + videoId + "/");
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            json = JObject.Parse(html);
            var jArray = json["tkl"].First["vs"];
            var dic = new Dictionary<int, JToken>();
            foreach (JToken jToken in jArray)
            {
                switch (jToken["bid"].ToString())
                {
                    case "10":
                        dic.Add(7, jToken);
                        break;
                    case "5":
                        dic.Add(6, jToken);
                        break;
                    case "4":
                        dic.Add(5, jToken);
                        break;
                    case "3":
                        dic.Add(4, jToken);
                        break;
                    case "2":
                        dic.Add(3, jToken);
                        break;
                    case "1":
                        dic.Add(2, jToken);
                        break;
                    default:
                        dic.Add(1, jToken);
                        break;
                }
            }
            JToken info;
            try
            {
                info = quality == "hd" ? dic[dic.Keys.Max()] : dic[dic.Keys.Max() - 1];
            }
            catch (Exception)
            {
                info = jArray.Last;
            }
            var linkToken = info["fs"];
            if (!info["fs"][0]["l"].ToString().StartsWith("/"))
            {
                linkToken = GetQiyLink(info["fs"][0]["l"].ToString()).EndsWith("mp4") ? info["flvs"] : info["fs"];
            }
            var tmpLink = linkToken[0]["l"].ToString().StartsWith("/")
                ? linkToken[0]["l"].ToString()
                : GetQiyLink(linkToken[0]["l"].ToString());
            tmpLink = "http://data.video.qiyi.com/videos" + tmpLink;
            html = CommonHelper.GetHtmlContent(tmpLink);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            return JObject.Parse(html)["l"].ToString();
        }

        #region 爱奇艺解码

        private static string GetQiyLink(string param1)
        {
            var loc2 = "";
            var loc3 = param1.Split('-');
            var loc4 = loc3.Length;
            var loc5 = loc4 - 1;
            while (loc5 >= 0)
            {
                var loc6 = GetVrsxorCode(Convert.ToUInt32(loc3[loc4 - loc5 - 1], 16), (uint)loc5);
                loc2 = Convert.ToChar(loc6) + loc2;
                loc5--;
            }
            return loc2;
        }

        private static uint GetVrsxorCode(uint param1, uint param2)
        {
            var loc3 = (int)(param2 % 3);
            if (loc3 == 1)
            {
                return param1 ^ 121;
            }
            if (loc3 == 2)
            {
                return param1 ^ 72;
            }
            return param1 ^ 103;
        }

        //private static string AiqiyiDecoder(byte[] param1)
        //{
        //    var loc3 = param1.Length;
        //    const int loc5 = 20110218;
        //    const int loc6 = loc5 % 100;
        //    var loc7 = loc3 % 4;
        //    var loc2 = new byte[loc3 + loc7];
        //    var loc4 = 0;
        //    while (loc4 + 4 <= loc3)
        //    {
        //        var temp = param1[loc4] << 24 | param1[loc4 + 1] << 16 | param1[loc4 + 2] << 8 |
        //                   param1[loc4 + 3];
        //        var loc8 = temp < 0 ? Convert.ToUInt32(UInt32.MaxValue + temp + 1) : Convert.ToUInt32(temp);
        //        loc8 = loc8 ^ Convert.ToUInt32(loc5);
        //        loc8 = rotate_right(loc8, loc6);
        //        loc2[loc4] = Convert.ToByte((loc8 & 4278190080) >> 24);
        //        loc2[loc4 + 1] = Convert.ToByte((loc8 & 16711680) >> 16);
        //        loc2[loc4 + 2] = Convert.ToByte((loc8 & 65280) >> 8);
        //        loc2[loc4 + 3] = Convert.ToByte(loc8 & 255);
        //        loc4 = loc4 + 4;
        //    }
        //    loc4 = 0;
        //    while (loc4 < loc7)
        //    {
        //        loc2[loc3 - loc7 - 1 + loc4] = param1[loc3 - loc7 - 1 + loc4];
        //        loc4++;
        //    }
        //    return Encoding.UTF8.GetString(loc2);
        //}

        //private static uint rotate_right(uint param1, int param2)
        //{
        //    var loc4 = 0;
        //    while (loc4 < param2)
        //    {
        //        var loc3 = param1 & 1;
        //        param1 = param1 >> 1;
        //        loc3 = loc3 << 31;
        //        param1 = param1 + loc3;
        //        loc4++;
        //    }
        //    return param1;
        //}

        #endregion

        private static string EncryptForTingApi(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            string result;
            using (var aesManaged = Aes.Create())
            {
                aesManaged.Key = Encoding.UTF8.GetBytes("a467f3d2f5fb0efe".ToUpper());
                aesManaged.IV = Encoding.UTF8.GetBytes("2012015318938857");
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, aesManaged.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(bytes, 0, bytes.Length);
                        cryptoStream.FlushFinalBlock();
                        var array = memoryStream.ToArray();
                        var text2 = Convert.ToBase64String(array);
                        text2 = Uri.EscapeDataString(text2);
                        result = text2;
                    }
                }
            }
            return result;
        }
    }
}