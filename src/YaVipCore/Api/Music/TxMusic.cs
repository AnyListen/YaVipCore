using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using YaVipCore.Helper;
using YaVipCore.Interface;
using YaVipCore.Models;
using System.Net.Http;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace YaVipCore.Api.Music
{
    public class TxMusic : IMusic
    {
        private const string MusicIp = "http://mobileoc.music.tc.qq.com/";

        private static string _txToken;
        private static DateTime _lastUpdateTime = DateTime.MinValue;
        private const string Vkdata = "<root><uid>3597552139</uid><sid>201701022335383597552139</sid><v>90</v><cv>70003</cv><ct>1</ct><OpenUDID>YYFM</OpenUDID><mcc>460</mcc><mnc>01</mnc><chid>001</chid><webp>0</webp><gray>0</gray><patch>105</patch><jailbreak>0</jailbreak><nettype>2</nettype><qq>123456</qq><authst>000158684fba0058e4775ffac60646f56c8e8a80f239509d0c8823cb5d9b4028425a67a293e5050cae54479f0724faccdae931a1b9b54e1766bd815a882625e2298cee1cce9a80249075a16f0a54d86bd16122bb368b8e304ede177f94622cd1</authst><localvip>2</localvip><cid>352</cid><platform>ios</platform><musicname>M8000047jzQv0sV4pz.mp3</musicname><downloadfrom>0</downloadfrom></root>";

        private static void UpdateToken()
        {
            if (string.IsNullOrEmpty(_txToken) || DateTime.Now - _lastUpdateTime > new TimeSpan(12,0,0))
            {
                try
                {
                    var response = new HttpClient().PostAsync("http://acc.music.qq.com/base/fcgi-bin/fcg_music_express_mobile2.fcg", new StringContent(Vkdata)).Result;
                    var buffer = response.Content.ReadAsByteArrayAsync().Result;
                    var resultStr = Decompress(buffer);
                    _txToken = Regex.Match(resultStr, @"(?<=mp3"">)[^<]+").Value;
                    _lastUpdateTime = DateTime.Now;
                }
                catch (Exception)
                {
                    //
                }
            }
        }

        private static string Decompress(byte[] baseBytes)
        {
            var newBytes = new byte[baseBytes.Length - 5];
            Array.Copy(baseBytes, 5, newBytes, 0, newBytes.Length);
            string resultStr;
            using (var memoryStream = new MemoryStream(newBytes))
            {
                using (var inf = new InflaterInputStream(memoryStream))
                {
                    using (var buffer = new MemoryStream())
                    {
                        var result = new byte[1024];

                        int resLen;
                        while ((resLen = inf.Read(result, 0, result.Length)) > 0)
                        {
                            buffer.Write(result, 0, resLen);
                        }
                        resultStr = Encoding.UTF8.GetString(result);
                    }
                }
            }
            return resultStr;
        }

        public static List<SongResult> Search(string key, int page, int size)
        {
            //https://c.y.qq.com/soso/fcgi-bin/client_search_cp?ct=24&qqmusic_ver=1298&remoteplace=sizer.yqq.song_next&t=0&aggr=1&cr=1&catZhida=1&lossless=0&flag_qc=0&p=1&n=20&w=%E5%8D%93%E4%BE%9D%E5%A9%B7-%E5%A6%82%E6%A2%A6%E5%88%9D%E9%86%92&g_tk=1220316356&loginUin=584586119&hostUin=0&format=jsonp&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0
            //var url = "http://soso.music.qq.com/fcgi-bin/search_cp?aggr=0&catZhida=0&lossless=1&sem=1&w=" + key + "&n=" + size + "&t=0&p=" + page + "&remoteplace=sizer.yqqlist.song&g_tk=5381&loginUin=0&hostUin=0&format=jsonp&inCharset=GB2312&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0";
            var url =
                $"https://c.y.qq.com/soso/fcgi-bin/client_search_cp?ct=24&qqmusic_ver=1298&remoteplace=sizer.yqq.song_next&t=0&aggr=1&cr=1&catZhida=1&lossless=0&flag_qc=0&p={page}&n={size}&w={key}&g_tk=1220316356&loginUin=584586119&hostUin=0&format=jsonp&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0";
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html.Replace("callback(", "").TrimEnd(')'));
                if (json["data"]["song"]["totalnum"].ToString() == "0")
                {
                    return null;
                }
                var datas = json["data"]["song"]["list"];
                return GetListByJson(datas);
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static List<SongResult> GetListByJson(JToken songs, bool isArtist = false)
        {
            if (songs == null)
            {
                return null;
            }
            var list = new List<SongResult>();
            foreach (JToken token in songs)
            {
                var j = isArtist ? token["musicData"] : token;
                var ar =
                    j["singer"].Aggregate("", (current, fJToken) => current + fJToken["name"].ToString() + ";")
                        .TrimEnd(';');
                var song = new SongResult
                {
                    SongId = j["songid"].ToString(),
                    SongName = j["songname"]?.ToString().HtmlDecode(),
                    SongSubName = "",
                    SongLink = "http://y.qq.com/portal/song/"+ (j["songmid"]?.ToString() ?? j["songid"]) + ".html",

                    ArtistId = j["singer"].First?["id"]?.ToString(),
                    ArtistName = ar.HtmlDecode(),
                    ArtistSubName = "",

                    AlbumId = j["albummid"]?.ToString() ?? "",
                    AlbumName = j["albumname"]?.ToString().HtmlDecode(),
                    AlbumSubName = "",
                    AlbumArtist = j["singer"].First?["name"].ToString().HtmlDecode(),

                    Length = j["interval"].Value<int>(),
                    BitRate = "128K",

                    FlacUrl = "",
                    ApeUrl = "",
                    WavUrl = "",
                    SqUrl = "",
                    HqUrl = "",
                    LqUrl = "",
                    CopyUrl = "",

                    SmallPic = "",
                    PicUrl = "",

                    LrcUrl = CommonHelper.GetSongUrl("qq", "128", j["songid"].ToString(), "lrc"),
                    TrcUrl = "",
                    KrcUrl = "",

                    MvId = j["vid"]?.ToString(),
                    MvHdUrl = "",
                    MvLdUrl = "",

                    Language = "",
                    Company = "",
                    Year = j["pubtime"] == null ? "" : CommonHelper.UnixTimestampToDateTime(Convert.ToInt64(j["pubtime"].ToString())).ToString("yyyy-MM-dd"),
                    Disc = 1,
                    TrackNum = string.IsNullOrEmpty(j["belongCD"]?.ToString()) ? 0 : Convert.ToInt32(j["belongCD"].ToString()),
                    Type = "qq"
                };
                var mid = j["strMediaMid"]?.ToString() ?? (j["media_mid"]?.ToString() ?? j["songmid"]?.ToString());
                if (string.IsNullOrEmpty(mid))
                {
                    song.BitRate = "320K";
                    song.LqUrl = song.HqUrl = song.SqUrl = j["songurl"].ToString();
                    song.CopyUrl = CommonHelper.GetSongUrl("qq", "320", song.SongId, "mp3");
                }
                else
                {
                    if (j["size128"].ToString() != "0")
                    {
                        song.BitRate = "128K";
                        song.LqUrl = CommonHelper.GetSongUrl("qq", "128", mid, "mp3");
                    }
                    if (j["sizeogg"].ToString() != "0")
                    {
                        song.BitRate = "192K";
                        song.HqUrl = CommonHelper.GetSongUrl("qq", "192", mid, "ogg");
                    }
                    if (j["size320"].ToString() != "0")
                    {
                        song.BitRate = "320K";
                        song.SqUrl = CommonHelper.GetSongUrl("qq", "320", mid, "mp3");
                    }
                    if (j["sizeape"].ToString() != "0")
                    {
                        song.BitRate = "无损";
                        song.ApeUrl = CommonHelper.GetSongUrl("qq", "999", mid, "ape");
                    }
                    if (j["sizeflac"].ToString() != "0")
                    {
                        song.BitRate = "无损";
                        song.FlacUrl = CommonHelper.GetSongUrl("qq", "999", mid, "flac");
                    }
                    song.CopyUrl = CommonHelper.GetSongUrl("qq", "320", mid, "mp3");
                }
                if (!string.IsNullOrEmpty(song.MvId))
                {
                    song.MvHdUrl = CommonHelper.GetSongUrl("qq", "hd", song.MvId, "mp4");
                    song.MvLdUrl = CommonHelper.GetSongUrl("qq", "ld", song.MvId, "mp4");
                }
                if (string.IsNullOrEmpty(song.AlbumId))
                {
                    song.SmallPic = "http://yyfm.oss-cn-qingdao.aliyuncs.com/anylisten/2311.jpg";
                    song.PicUrl = "http://yyfm.oss-cn-qingdao.aliyuncs.com/anylisten/23.jpg";
                }
                else
                {
                    song.SmallPic = "http://i.gtimg.cn/music/photo/mid_album_150/" + song.AlbumId[song.AlbumId.Length - 2] +
                              "/" + song.AlbumId[song.AlbumId.Length - 1] + "/" + song.AlbumId + ".jpg";
                    song.PicUrl = "http://i.gtimg.cn/music/photo/mid_album_500/" + song.AlbumId[song.AlbumId.Length - 2] +
                                  "/" + song.AlbumId[song.AlbumId.Length - 1] + "/" + song.AlbumId + ".jpg";
                }
                list.Add(song);
            }
            return list;
        }

        private static List<SongResult> SearchAlbum(string id)
        {
            var str = "albummid=" + id;
            if (Regex.IsMatch(id, @"^\d+$"))
            {
                str = "albumid=" + id;
            }
            var url =
                "http://i.y.qq.com/v8/fcg-bin/fcg_v8_album_info_cp.fcg?" + str +
                "&g_tk=5381&uin=0&format=jsonp&inCharset=utf-8&outCharset=utf-8&notice=0&platform=h5&needNewCode=1";
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html.Replace("callback(", "").TrimEnd(')'));
                if (json["message"].ToString() != "succ")
                {
                    return null;
                }
                var datas = json["data"]["list"];
                var year = json["data"]["aDate"].ToString();
                var cmp = json["data"]["company"].ToString();
                var lug = json["data"]["lan"].ToString().HtmlDecode();
                var ar = json["data"]["singername"].ToString().HtmlDecode();
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
            var str = "singermid=" + id;
            if (Regex.IsMatch(id, @"^\d+$"))
            {
                str = "singerid=" + id;
            }
            var url =
                "http://i.y.qq.com/v8/fcg-bin/fcg_v8_singer_track_cp.fcg?order=listen&begin=" + (page - 1) * size +
                "&num=" + size + "&" + str + "&g_tk=5381&uin=0&format=jsonp&inCharset=utf-8&outCharset=utf-8&notice=0&platform=h5page&needNewCode=1&from=h5";
            var html = CommonHelper.GetHtmlContent(url).Replace("callback(", "").TrimEnd(')');
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                if (json["message"].ToString() != "succ")
                {
                    return null;
                }
                var datas = json["data"]["list"];
                return GetListByJson(datas, true);
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static List<SongResult> SearchCollect(string id)
        {

            var url = "http://i.y.qq.com/qzone-music/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&json=1&utf8=1&onlysong=0&nosign=1&disstid=" + id + "&g_tk=5381&loginUin=0&hostUin=0&format=jsonp&inCharset=GB2312&outCharset=utf-8&notice=0&platform=yqq&jsonpCallback=jsonCallback&needNewCode=0";
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            var json = JObject.Parse(html.Replace("jsonCallback(", "").TrimEnd(')'));
            if (json["cdlist"] == null)
            {
                return null;
            }
            try
            {
                var datas = json["cdlist"].First["songlist"];
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
            try
            {
                #region 旧版
                
                //var str = id;
                //if (Regex.IsMatch(id, @"^\d+$"))
                //{
                //    str = id + "_num";
                //}
                ////http://y.qq.com/portal/song/004Xy7Rb2kGXEu.html
                //var url = "http://y.qq.com/portal/song/"+str+".html";
                //var html = CommonHelper.GetHtmlContent(url);
                //if (string.IsNullOrEmpty(html))
                //{
                //    return null;
                //}
                //var j = JToken.Parse("["+ Regex.Match(html, @"(?<=var\s*g_SongData\s*=\s*)({[\s\S]+?})(?=;)").Value + "]");
                //var list = GetListByJson(j);
                //return list?[0];

                #endregion

                var url = "http://i.y.qq.com/s.plcloud/fcgi-bin/fcg_list_songinfo_cp.fcg?midlist="+id;
                if (Regex.IsMatch(id, @"^\d+$"))
                {
                    url = "http://i.y.qq.com/s.plcloud/fcgi-bin/fcg_list_songinfo_cp.fcg?idlist=" + id;
                }
                var html = CommonHelper.GetHtmlContent(url);
                if (string.IsNullOrEmpty(html))
                {
                    return null;
                }
                var json = JObject.Parse(html);
                var datas = json["data"];
                var list = GetListByJson(datas);
                return list?[0];
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static string GetUrl(string id, string quality, string format)
        {
            switch (format)
            {
                case "lrc":
                    if (!Regex.IsMatch(id, @"^\d+$"))
                    {
                        id = SearchSong(id).SongId;
                    }
                    var url =
                        "http://lyric.music.qq.com/fcgi-bin/fcg_query_lyric.fcg?nobase64=1&musicid=" + id + "&callback=jsonp1";
                    var html = CommonHelper.GetHtmlContent(url, 1, new Dictionary<string, string>
                    {
                        {"Host", "lyric.music.qq.com"},
                        {"Referer", "http://lyric.music.qq.com"}
                    });
                    if (string.IsNullOrEmpty(html))
                    {
                        return "";
                    }
                    var json = JObject.Parse(html.Replace("jsonp1(", "").TrimEnd(')'));
                    return json["retcode"].ToString() != "0" ? null : json["lyric"].ToString().HtmlDecode();
                case "mp4":
                case "flv":
                    return GetMvUrl(id, quality);
                case "jpg":
                    return SearchSong(id).PicUrl;
            }

            if (Regex.IsMatch(id, @"^\d+$"))
            {
                switch (format)
                {
                    case "ape":
                        return "http://stream.qqmusic.tc.qq.com/" + (Convert.ToInt32(id) + 80000000) + ".ape";
                    case "flac":
                        return "http://stream.qqmusic.tc.qq.com/" + (Convert.ToInt32(id) + 70000000) + ".flac";
                    case "ogg":
                        return "http://stream.qqmusic.tc.qq.com/" + (Convert.ToInt32(id) + 40000000) + ".ogg";
                    case "mp3":
                        switch (quality)
                        {
                            case "128":
                                return "http://stream.qqmusic.tc.qq.com/" + (Convert.ToInt32(id) + 30000000) + ".mp3";
                            case "192":
                                return "http://stream.qqmusic.tc.qq.com/" + (Convert.ToInt32(id) + 40000000) + ".ogg";
                        }
                        return "http://stream.qqmusic.tc.qq.com/" + id + ".mp3";
                    default:
                        return "http://stream.qqmusic.tc.qq.com/" + id + ".mp3";
                }
            }
            UpdateToken();
            string fileName;
            switch (format)
            {
                case "ape":
                    fileName = "A000" + id + ".ape";
                    break;
                case "flac":
                    fileName = "F000" + id + ".flac";
                    break;
                case "ogg":
                    fileName = "O600" + id + ".ogg";
                    break;
                case "m4a":
                    fileName = "C600" + id + ".m4a";
                    break;
                default:
                    fileName = (quality == "128" ? "M500" : "M800") + id + ".mp3";
                    break;
            }
            return MusicIp + fileName + "?vkey=" + _txToken + "&guid=YYFM&uin=123456&fromtag=53";
        }

        private const string TxCookie =
            "3g_guest_id=-9014989604440178688; sd_userid=82681464185837817; sd_cookie_crttime=1464185837817; eas_sid=f124w6b4l8a5t6R0b8N5w8z5z2; tvfe_boss_uuid=486e4f9cf0f59d2c; gid=140271907059908608; ip_limit=1; pac_uid=1_584586119; ts_refer=ADTAGCLIENT.QQ.5473_.0; mobileUV=1_1570f61f3b8_cc7cb; ts_uid=4870894033; pgv_pvid=3494530360; o_cookie=584586119; luin=o0584586119; lskey=00010000d730e3bcec69e4baffbb3ae3c73f5112b0bc2cae3fbc64dde3dec454d724de29986a091e5b986d54; login_remember=qq; ptcz=38e9f64625dc99baeb194eaa55a0e769e3306cfec50ad188b0a75ac7aeabd429; pt2gguin=o0584586119; ptisp=ctc; pgv_pvi=3200589824; pgv_si=s9409367040; qqmusic_uin=12345678; qqmusic_key=12345678; qqmusic_fromtag=30; ptag=|new_vs_feature:item; ad_play_index=87; main_login=qq; encuin=6fa312ef1c93831a6f356522325b0902|584586119; lw_nick=Shelher|584586119|//q4.qlogo.cn/g?b=qq&k=iaqb1jcGbARKSgKeicREaHMg&s=40&t=1465025590|1";

        private static string GetMvUrl(string id, string quality)
        {
            //此处使用腾讯视频会员Cookie可获取1080P资源
            var html =
                CommonHelper.GetHtmlContent(
                    "http://vv.video.qq.com/getinfo?vid=" + id + "&platform=11&charge=1&otype=json", 0, new Dictionary<string, string>
                    {
                        {"Cookie", TxCookie}
                    });
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            var json = JObject.Parse(html.Replace("QZOutputJson=", "").TrimEnd(';'));
            if (json["fl"] == null)
            {
                return null;
            }
            var dic = json["fl"]["fi"].ToDictionary(jToken => jToken["name"].ToString(),
                jToken => jToken["id"].Value<int>());
            int info;
            if (quality == "hd")
            {
                if (dic.ContainsKey("fhd"))
                {
                    info = dic["fhd"];
                }
                else if (dic.ContainsKey("shd"))
                {
                    info = dic["shd"];
                }
                else if (dic.ContainsKey("hd"))
                {
                    info = dic["hd"];
                }
                else if (dic.ContainsKey("sd"))
                {
                    info = dic["sd"];
                }
                else
                {
                    info = dic["mp4"];
                }
            }
            else
            {
                if (dic.ContainsKey("shd"))
                {
                    info = dic["shd"];
                }
                else if (dic.ContainsKey("hd"))
                {
                    info = dic["hd"];
                }
                else if (dic.ContainsKey("sd"))
                {
                    info = dic["sd"];
                }
                else
                {
                    info = dic["mp4"];
                }
            }
            var vkey = GetVkey(info, id);
            var fn = id + ".p" + (info - 10000) + ".1.mp4";
            return json["vl"]["vi"].First["ul"]["ui"].First["url"] + fn + "?vkey=" + vkey;
        }

        private static string GetVkey(int id, string videoId)
        {
            var fn = videoId + ".p" + (Convert.ToInt32(id) - 10000) + ".1.mp4";
            var url = "http://vv.video.qq.com/getkey?format=" + id + "&otype=json&vid=" + videoId +
                      "&platform=11&charge=1&filename=" + fn;
            var html = CommonHelper.GetHtmlContent(url, 0, new Dictionary<string, string>
            {
                {"Cookie", TxCookie}
            });
            return string.IsNullOrEmpty(html) ? null : Regex.Match(html, @"(?<=key"":"")[^""]+(?="")").Value;
        }

        //private static string GetKey()
        //{
        //    var html =
        //        CommonHelper.GetHtmlContent("http://base.music.qq.com/fcgi-bin/fcg_musicexpress.fcg?json=3&guid=1103396853");
        //    return Regex.Match(html, @"(?<=key"":\s*"")[^""]+").Value;
        //}

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
            return SearchArtist(id,page,size);
        }

        public List<SongResult> CollectSearch(string id, int page, int size)
        {
            return SearchCollect(id);
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