using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AnyListen.Helper;
using AnyListen.Models;
using Newtonsoft.Json.Linq;

namespace AnyListen.Music
{
    public class KgMusic : IMusic
    {
        //private static string _kgToken;
        //private static string _kgUsrId;
        //private static DateTime _lastUpdateTime = DateTime.MinValue;


        //{"username":"XXXXXXX","clienttime":1490882950,"clientver":8708,"p":"75BDD13CE1222A190B4EA964B24CB7A1C2BD52EC4AC46997B50F6D30CF9C6D1EED8EC7288A8C857F5343B0C423224D53E2616C5DF62446B108D697BA223226743AC5570DF6DBFFC541BC88A56D3F084B6D3624A79EFD582223E2511B263CE4B15E8E9D2BB95E69D8B51774FC6AB9F79CF42A23A8C371C86D75C1AED1A5588FF5","appid":1005,"uuid":"194411aa79054c328f5341a2f7179a20","mid":"169617645177286751451596349075271490597","key":"59f222f0b028bd80b790266ad1ac196a"}

        //private const string UsrData = "{\"username\":\"XXXXXX\",\"clienttime\":1490882950,\"clientver\":8708,\"p\":\"75BDD13CE1222A190B4EA964B24CB7A1C2BD52EC4AC46997B50F6D30CF9C6D1EED8EC7288A8C857F5343B0C423224D53E2616C5DF62446B108D697BA223226743AC5570DF6DBFFC541BC88A56D3F084B6D3624A79EFD582223E2511B263CE4B15E8E9D2BB95E69D8B51774FC6AB9F79CF42A23A8C371C86D75C1AED1A5588FF5\",\"appid\":1005,\"uuid\":\"194411aa79054c328f5341a2f7179a20\",\"mid\":\"169617645177286751451596349075271490597\",\"key\":\"59f222f0b028bd80b790266ad1ac196a\"}";

        //private static void UpdateToken()
        //{
        //    if (!string.IsNullOrEmpty(_kgToken) && (DateTime.Now - _lastUpdateTime <= new TimeSpan(1, 0, 0, 0))) return;
        //    var html = CommonHelper.PostData("http://login.user.kugou.com/v2/login_by_pwd",
        //        new Dictionary<string, string> { { "JSON", UsrData } }, 1);
        //    if (string.IsNullOrEmpty(html))
        //    {
        //        return;
        //    }
        //    _lastUpdateTime = DateTime.Now;
        //    _kgToken = JObject.Parse(html)["data"]["token"].ToString();
        //    _kgUsrId = JObject.Parse(html)["data"]["userid"].ToString();
        //    //var token = Login();
        //    //if (string.IsNullOrEmpty(token))
        //    //{
        //    //    return;
        //    //}
        //    //_lastUpdateTime = DateTime.Now;
        //    //_kgToken = token;
        //}

        //private static string Login()
        //{
        //    var username = "13061220819";
        //    var password = "caddie1100200";
        //    string text = "1234567890";
        //    string value = username + password + text + "wp001";
        //    string text2 = CommonHelper.Md5(value);
        //    string text3 = string.Concat(new string[]
        //    {
        //    "username=",
        //    username,
        //    "&password=",
        //    password,
        //    "&plat=10&version=1000&imei=",
        //    text,
        //    "&key=",
        //    text2
        //    });

        //    byte[] bytes2 = Encoding.UTF8.GetBytes(text3);
        //    PKCS5Padding pKCS5Padding = new PKCS5Padding();
        //    byte[] input = pKCS5Padding.Pad(8, bytes2);
        //    DesCipher desCipher = new DesCipher(new byte[] { 79, 46, 13, 17, 28, 39, 7, 3 }, new CbcCipherMode(new byte[] { 26, 59, 48, 68, 35, 24, 11, 99 }), new PKCS5Padding());
        //    byte[] buff = desCipher.Encrypt(input);
        //    string text4 = ByteToString(buff);
        //    var html = CommonHelper.PostData("http://wpservice.kugou.com/new/app/i/user.php", new Dictionary<string, string> {
        //        { "cmd","404"},{ "crypt",text4} });
        //    if (string.IsNullOrEmpty(html))
        //    {
        //        return "";
        //    }
        //    return Regex.Match(html, @"(?<=sign"":"")[^""]+").Value;
        //}

        //private static string ByteToString(byte[] buff)
        //{
        //    string text = "";
        //    for (int i = 0; i < buff.Length; i++)
        //    {
        //        text += buff[i].ToString("X2");
        //    }
        //    return text;
        //}

        public static List<SongResult> Search(string key, int page, int size)
        {
            var url = "http://ioscdn.kugou.com/api/v3/search/song?keyword=" + key + "&page=" + page + "&pagesize=" + size + "&showtype=10&plat=2&version=7910&tag=1&correct=1&privilege=1&sver=5";
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                if (!string.IsNullOrEmpty(json["error"].ToString()) || json["data"]["total"].ToString() == "0")
                {
                    return null;
                }
                var datas = json["data"]["info"];
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
                if (j["privilege"].ToString() == "5")
                {
                    continue;
                }
                var song = new SongResult
                {
                    SongId = j["hash"].ToString(),
                    SongName = j["filename"].ToString(),
                    SongSubName = j["alias"]?.ToString(),
                    SongLink = "",

                    ArtistId = "",
                    ArtistName = (j["singername"]?.ToString() ?? "").Replace("+", ";"),
                    ArtistSubName = "",

                    AlbumId = j["album_id"]?.ToString() ?? "",
                    AlbumName = j["album_name"]?.ToString() ?? "",
                    AlbumSubName = "",
                    AlbumArtist = (j["singername"]?.ToString() ?? "").Replace("+", ";"),

                    Length = Convert.ToInt32(j["duration"].ToString()),
                    BitRate = "128K",

                    FlacUrl = "",
                    ApeUrl = "",
                    WavUrl = "",
                    SqUrl = "",
                    HqUrl = "",
                    LqUrl = "",
                    CopyUrl = "",

                    SmallPic = CommonHelper.GetSongUrl("kg", "low", j["hash"].ToString(), "jpg"),
                    PicUrl = CommonHelper.GetSongUrl("kg", "high", j["hash"].ToString(), "jpg"),

                    LrcUrl = CommonHelper.GetSongUrl("kg", "320", j["hash"].ToString(), "lrc"),
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
                    Type = "kg"
                };
                if (!string.IsNullOrEmpty(song.AlbumId))
                {
                    song.SmallPic = CommonHelper.GetSongUrl("kg", "low", song.AlbumId, "jpg");
                    song.PicUrl = CommonHelper.GetSongUrl("kg", "high", song.AlbumId, "jpg");
                }
                if (string.IsNullOrEmpty(song.ArtistName))
                {
                    if (song.SongName.Contains("-"))
                    {
                        song.ArtistName = song.SongName.Substring(0, song.SongName.IndexOf('-')).Trim();
                        song.SongName = song.SongName.Substring(song.SongName.IndexOf('-') + 1).Trim();
                    }
                }
                else
                {
                    var name = song.SongName.Substring(0, song.SongName.IndexOf('-')).Trim();
                    if (song.ArtistName.Trim() == name)
                    {
                        song.SongName = song.SongName.Substring(song.SongName.IndexOf('-') + 1).Trim();
                    }
                }

                if (!string.IsNullOrEmpty(j["mvhash"].ToString()))
                {
                    song.MvHdUrl = CommonHelper.GetSongUrl("kg", "hd", j["mvhash"].ToString(), "mp4");
                    song.MvLdUrl = CommonHelper.GetSongUrl("kg", "ld", j["mvhash"].ToString(), "mp4");
                }
                if (!string.IsNullOrEmpty(j["hash"].ToString()))
                {
                    song.BitRate = "128K";
                    song.CopyUrl = song.LqUrl = CommonHelper.GetSongUrl("kg", "128", j["hash"].ToString(), "mp3");
                }
                if (!string.IsNullOrEmpty(j["320hash"].ToString()))
                {
                    song.BitRate = "320K";
                    song.CopyUrl = song.SqUrl = CommonHelper.GetSongUrl("kg", "320", j["320hash"].ToString(), "mp3");
                }
                if (!string.IsNullOrEmpty(j["sqhash"].ToString()))
                {
                    song.BitRate = "无损";
                    song.FlacUrl = CommonHelper.GetSongUrl("kg", "1000", j["sqhash"].ToString(), "flac");
                }
                list.Add(song);
            }
            return list;
        }

        private static List<SongResult> SearchAlbum(string id)
        {
            var url = "http://ioscdn.kugou.com/api/v3/album/song?albumid=" + id + "&page=1&pagesize=-1&plat=2&version=7910";
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                if (json["data"]["total"].ToString() == "0")
                {
                    return null;
                }
                var datas = json["data"]["info"];
                var list = GetListByJson(datas);

                html = CommonHelper.GetHtmlContent("http://ioscdn.kugou.com/api/v3/album/info?albumid=" + id + "&version=7910");
                if (string.IsNullOrEmpty(html))
                {
                    return list;
                }
                json = JObject.Parse(html);

                var time = json["data"]["publishtime"].ToString().Substring(0, 10);
                var al = json["data"]["albumname"].ToString();
                var singerId = json["data"]["singerid"].ToString();
                var singerName = json["data"]["singername"].ToString();
                var pic = json["data"]["imgurl"].ToString().Replace("{size}", "480");
                var smallPic = json["data"]["imgurl"].ToString().Replace("{size}", "120");
                for (var i = 0; i < list.Count; i++)
                {
                    list[i].ArtistId = singerId;
                    list[i].ArtistName = singerName;
                    list[i].AlbumName = al;
                    list[i].AlbumArtist = singerName;
                    list[i].TrackNum = i + 1;
                    list[i].Year = time;
                    list[i].SmallPic = smallPic;
                    list[i].PicUrl = pic;
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
            var url = "http://ioscdn.kugou.com/api/v3/singer/song?singerid=" + id + "&page=" + page + "&pagesize=" +
                      size + "&sorttype=2&plat=2&version=7910";
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                if (json["data"]["total"].ToString() == "0")
                {
                    return null;
                }
                var datas = json["data"]["info"];
                return GetListByJson(datas);
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static List<SongResult> SearchCollect(string id, int page)
        {
            var url = "http://m.kugou.com/plist/list/?specialid=" + id + "&page=" + page + "&plat=2&json=true";
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html) || html == "null")
            {
                return null;
            }
            var json = JObject.Parse(html);
            if (json["list"]["list"]["total"].ToString() == "0")
            {
                return null;
            }
            try
            {
                var datas = json["list"]["list"]["info"];
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
            var html = CommonHelper.GetHtmlContent("http://m.kugou.com/app/i/getSongInfo.php?hash=" + id + "&album_id=&cmd=playInfo");
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            try
            {
                var json = JObject.Parse(html);
                var key = json["fileName"].ToString();
                var list = Search(key, 1, 30);
                if (list == null)
                {
                    return null;
                }
                var song = list.SingleOrDefault(t => t.SongId == id);
                return song ?? list[0];
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
                return null;
            }
        }

        private static string GetUrl(string id, string quality, string format)
        {
            if (format == "jpg" && Regex.IsMatch(id, @"^\d+$"))
            {
                var html = CommonHelper.GetHtmlContent("http://ioscdn.kugou.com/api/v3/album/info?albumid=" + id + "&version=7910");
                if (string.IsNullOrEmpty(html))
                {
                    return quality == "low" ? "http://yyfm.oss-cn-qingdao.aliyuncs.com/anylisten/2311.jpg" : "http://yyfm.oss-cn-qingdao.aliyuncs.com/anylisten/23.jpg";
                }
                html = CommonHelper.UnicodeToString(html);
                var json = JObject.Parse(html);
                if (string.IsNullOrEmpty(json["data"]?.ToString()))
                {
                    return quality == "low" ? "http://yyfm.oss-cn-qingdao.aliyuncs.com/anylisten/2311.jpg" : "http://yyfm.oss-cn-qingdao.aliyuncs.com/anylisten/23.jpg";
                }
                return json["data"]["imgurl"].ToString().Replace("{size}", quality == "high" ? "480" : "120");
            }

            if (format == "lrc" || format == "jpg")
            {
                var html =
                    CommonHelper.GetHtmlContent("http://m.kugou.com/app/i/getSongInfo.php?cmd=playInfo&hash=" + id);
                if (string.IsNullOrEmpty(html) || html.Contains("hash error"))
                {
                    return null;
                }
                var json = JObject.Parse(html);
                var songName = json["fileName"].ToString();
                var len = json["timeLength"] + "000";
                if (format == "lrc")
                {
                    html =
                        CommonHelper.GetHtmlContent("http://m.kugou.com/app/i/krc.php?cmd=100&keyword=" + songName +
                                                    "&hash=" + id + "&timelength=" + len + "&d=0.38664927426725626");
                    if (string.IsNullOrEmpty(html))
                    {
                        return "";
                    }
                    return "[ti:" + songName + "]\n[by: 雅音FM]\n" + html;
                }
                html =
                    CommonHelper.GetHtmlContent("http://m.kugou.com/app/i/getSingerHead_new.php?singerName=" +
                                                songName.Split('-')[0].Trim() + "&size=" + (quality == "high" ? "480" : "120"));
                if (string.IsNullOrEmpty(html) || html.Contains("未能找到"))
                {
                    return "http://yyfm.oss-cn-qingdao.aliyuncs.com/img/mspy.jpg";
                }
                return Regex.Match(html, @"(?<=url"":"")[^""]+").Value.Replace("\\", "");
            }

            if (format == "mp4" || format == "flv")
            {
                var key = CommonHelper.Md5(id + "kugoumvcloud");
                var html =
                    CommonHelper.GetHtmlContent("http://trackermv.kugou.com/interface/index/cmd=100&hash=" + id +
                                                "&key=" + key + "&pid=6&ext=mp4");
                if (string.IsNullOrEmpty(html))
                {
                    return "";
                }
                var json = JObject.Parse(html);
                if (quality == "hd")
                {
                    if (json["mvdata"]["rq"]["downurl"] != null)
                    {
                        return json["mvdata"]["rq"]["downurl"].ToString();
                    }
                    if (json["mvdata"]["sq"]["downurl"] != null)
                    {
                        return json["mvdata"]["sq"]["downurl"].ToString();
                    }
                    if (json["mvdata"]["hd"]["downurl"] != null)
                    {
                        return json["mvdata"]["hd"]["downurl"].ToString();
                    }
                    if (json["mvdata"]["sd"]["downurl"] != null)
                    {
                        return json["mvdata"]["sd"]["downurl"].ToString();
                    }
                }
                else
                {
                    if (json["mvdata"]["sq"]["downurl"] != null)
                    {
                        return json["mvdata"]["sq"]["downurl"].ToString();
                    }
                    if (json["mvdata"]["hd"]["downurl"] != null)
                    {
                        return json["mvdata"]["hd"]["downurl"].ToString();
                    }
                    if (json["mvdata"]["sd"]["downurl"] != null)
                    {
                        return json["mvdata"]["sd"]["downurl"].ToString();
                    }
                }
            }

            //UpdateToken();
            //adb7880f0c23c54e3809000fb82d9f23efeaa1d0bb396c8940837ebbb71b8322
            //58680f9d03053a8ae7f85a1342a2b0f960c277071f9f02fed879fd43818f7304
            //var result = GetBackUrl(id);
            //if (!string.IsNullOrEmpty(result))
            //{
            //    return result;
            //}

            //http://trackercdnbj.kugou.com/i/v2/?pid=2&mid=127347821333410747944412026415356129954&cmd=26&token=&hash=540f7d06f0d4afefb5483414a02aa467&area_code=1&behavior=download&appid=1005&module=&vipType=-4&userid=0&album_id=4461429&key=b147da2705194b786bcbe849dafcd2f5&version=8918&with_res_tag=1

            var link =
                $"http://trackercdnbj.kugou.com/i/v2/?pid=2&mid=1990&cmd=26&token=&hash={id}&area_code=1&behavior=download&appid=1005&module=&vipType=-4&userid=0&key={CommonHelper.Md5(id + "kgcloudv210051990" + "0")}&version=8918&with_res_tag=1";
            var mp3Html = CommonHelper.GetHtmlContent(link);
            var result = Regex.Match(mp3Html, @"(?<=url"":\[?"")[^""]+").Value.Replace("\\", "");
            return string.IsNullOrEmpty(result) ? GetWebUrl(id) : result;
        }

        private static string GetBackUrl(string id)
        {
            var token = CommonHelper.Md5(id + "kgcloudv2");
            var url = "http://trackercdnbj.kugou.com/i/v2?cmd=25&hash=" + id + "&behavior=play&appid=1005&pid=2&key=" + token + "&version=8483";
            var html = CommonHelper.GetHtmlContent(url);
            return Regex.Match(html, @"(?<=url"":\[?"")[^""]+").Value.Replace("\\", "");
        }

        private static string GetWebUrl(string id)
        {
            var url = "http://www.kugou.com/yy/index.php?r=play/getdata&hash=" + id;
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            var result = Regex.Match(html, @"(?<=url"":"")[^""]+")
                    .Value.Replace("\\", "").Replace("http://fs.web.kugou.com/", "http://fs.android2.kugou.com/");
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
            html = CommonHelper.GetHtmlContent("http://m.kugou.com/app/i/getSongInfo.php?cmd=playInfo&hash=" + id);
            return Regex.Match(html, @"(?<=""url"":"")[^""]+")
                .Value.Replace("\\", "").Replace("http://fs.open.kugou.com/", "http://fs.android2.kugou.com/");

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
            return SearchCollect(id, page);
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