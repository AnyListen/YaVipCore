using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AnyListen.Helper;
using AnyListen.Models;
using Newtonsoft.Json.Linq;

namespace AnyListen.Music
{
    public class SonyMusic : IMusic
    {
        private static string GetDes3EncryptedText(string input)
        {
            var k = new byte[24];
            var data = Encoding.UTF8.GetBytes(input);
            var len = data.Length;
            if (data.Length % 8 != 0)
            {
                len = data.Length - data.Length % 8 + 8;
            }
            byte[] needData = null;
            if (len != 0)
                needData = new byte[len];
            for (var i = 0; i < len; i++)
            {
                if (needData != null) needData[i] = 0x00;
            }
            Buffer.BlockCopy(data, 0, needData, 0, data.Length);
            var key = Encoding.UTF8.GetBytes("1!QAZ2@WSXCDE#3$4RFVB7GT%5^6YYUMJU7&8*IK<.LO9(0P");
            if (key.Length == 16)
            {
                Buffer.BlockCopy(key, 0, k, 0, key.Length);
                Buffer.BlockCopy(key, 0, k, 16, 8);
            }
            else
            {
                Buffer.BlockCopy(key, 0, k, 0, 24);
            }
            var des3 = TripleDES.Create();
            des3.Key = k;
            des3.IV = Encoding.UTF8.GetBytes("12481632");
            des3.Mode = CipherMode.CBC;
            des3.Padding = PaddingMode.PKCS7;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, des3.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private static List<SongResult> Search(string key, int page, int size)
        {
            key = key.Replace("\"", "");
            var text = "keyword="+key+"&"+ "pageNo=" + (page - 1) + "&" + "pageSize=" + size + "&" + "type=TRACK";
            var url = "http://api.sonyselect.com.cn/es/search/v1/android/?sign=" + GetDes3EncryptedText(text);
            var data =
                "{\"content\":{\"type\":\"TRACK\",\"pageSize\":\""+size+ "\",\"pageNo\":\"" + (page-1) + "\",\"keyword\":\""+key+"\"},\"header\":{\"sdkNo\":\"4.2.2\",\"model\":\"X9Plus\",\"manufacturer\":\"vivo\",\"imei\":\"133524532901500\"}}";
            var html = CommonHelper.PostData(url, new Dictionary<string, string>{{"JSON", data}},1);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            var json = JObject.Parse(html);
            if (json["content"]["trackPage"]["totalElements"].Value<int>() <= 0)
            {
                return null;
            }
            var datas = json["content"]["trackPage"]["content"];
            var list = new List<SongResult>();
            foreach (JToken j in datas)
            {
                try
                {
                    var song = new SongResult
                    {
                        SongId = j["id"].ToString(),
                        SongName = j["name"].ToString(),
                        SongSubName = "",
                        SongLink = "",

                        ArtistId = j["albums"]?.First?["artistId"].ToString() ?? "",
                        ArtistName = j["albums"]?.First?["aritst"].ToString() ?? "",
                        ArtistSubName = "",

                        AlbumId = j["albums"]?.First?["id"].ToString() ?? "0",
                        AlbumName = j["albums"]?.First?["name"].ToString() ?? "",
                        AlbumSubName = "",
                        AlbumArtist = j["albums"]?.First?["aritst"].ToString() ?? "",

                        Length = CommonHelper.TimeToNum(j["duration"].ToString()),
                        BitRate = "无损",

                        FlacUrl = "",
                        ApeUrl = "",
                        WavUrl = "",
                        SqUrl = "",
                        HqUrl = "",
                        LqUrl = "",
                        CopyUrl = "",

                        SmallPic = j["albums"]?.First?["smallIcon"].ToString() ?? "",
                        PicUrl = j["albums"]?.First?["largeIcon"].ToString() ?? "",

                        LrcUrl = "",
                        TrcUrl = "",
                        KrcUrl = "",

                        MvId = "",
                        MvHdUrl = "",
                        MvLdUrl = "",

                        Language = "",
                        Company = "",
                        Year = j["createTime"]?.ToString(),
                        Disc = 1,
                        TrackNum = 0,
                        Type = "sn"
                    };
                    if (j["auditionUrl"] != null)
                    {
                        var link = j["auditionUrl"].ToString();
                        song.LqUrl = song.HqUrl = song.SqUrl = song.CopyUrl = link;
                        song.FlacUrl = CommonHelper.GetSongUrl("sn", CommonHelper.EncodeBase64(Encoding.UTF8, j["downloadUrl"].ToString()), song.SongId, "flac");

//                        link = link.Replace("/Audition/", "/Audio/").Replace(".mp3", "");
//                        if (j["downloadUrl"] != null)
//                        {
//                            if (j["downloadUrl"].ToString().StartsWith("http"))
//                            {
//                                link = j["downloadUrl"] + "/44100/001.flac";
//                            }
//                        }
//                        if (link.Contains(".flac"))
//                        {
//                            song.FlacUrl = link;
//                        }
//                        else if (link.Contains(".ape"))
//                        {
//                            song.ApeUrl = link;
//                        }
//                        else
//                        {
//                            song.WavUrl = link;
//                        }
                    }
                    list.Add(song);
                }
                catch (Exception e)
                {
                    CommonHelper.AddLog(j.ToString());
                    CommonHelper.AddLog(e.ToString());
                }
            }
            return list;
        }

        private static List<SongResult> SearchAlbum(string id)
        {
            var text = "albumId="+id;
            var url = "http://api.sonyselect.com.cn/albumDetail/v1/android/?sign=" + GetDes3EncryptedText(text);
            var data =
                "{\"content\":{\"albumId\":\""+id+"\"},\"header\":{\"sdkNo\":\"4.2.2\",\"model\":\"X9Plus\",\"manufacturer\":\"vivo\",\"imei\":\"133524532901500\"}}";
            var html = CommonHelper.PostData(url, new Dictionary<string, string> { { "JSON", data } }, 1);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            var json = JObject.Parse(html);
            if (json["content"]["album"]["musicNum"].Value<int>() <= 0)
            {
                return null;
            }
            var datas = json["content"]["album"]["tracks"];
            var list = new List<SongResult>();
            var ar = json["content"]["album"]["aritst"].ToString();
            var arId = json["content"]["album"]["artistId"].ToString();
            var an = json["content"]["album"]["name"].ToString();
            var sm = json["content"]["album"]["smallIcon"].ToString();
            var lg = json["content"]["album"]["largeIcon"].ToString();
            var index = 0;
            foreach (JToken j in datas)
            {
                index++;
                var song = new SongResult
                {
                    SongId = j["id"].ToString(),
                    SongName = j["name"].ToString(),
                    SongSubName = "",
                    SongLink = "",

                    ArtistId = arId,
                    ArtistName = ar,
                    ArtistSubName = "",

                    AlbumId = id,
                    AlbumName = an,
                    AlbumSubName = "",
                    AlbumArtist = ar,

                    Length = CommonHelper.TimeToNum(j["duration"].ToString()),
                    BitRate = "无损",

                    FlacUrl = "",
                    ApeUrl = "",
                    WavUrl = "",
                    SqUrl = "",
                    HqUrl = "",
                    LqUrl = "",
                    CopyUrl = "",

                    SmallPic = sm,
                    PicUrl = lg,

                    LrcUrl = "",
                    TrcUrl = "",
                    KrcUrl = "",

                    MvId = "",
                    MvHdUrl = "",
                    MvLdUrl = "",

                    Language = "",
                    Company = "",
                    Year = j["createTime"]?.ToString(),
                    Disc = 1,
                    TrackNum = index,
                    Type = "sn"
                };
                if (j["auditionUrl"] != null)
                {
                    var link = j["auditionUrl"].ToString();
                    song.LqUrl = song.HqUrl = song.SqUrl = song.CopyUrl = link;
                    song.FlacUrl = CommonHelper.GetSongUrl("sn", "1000", song.SongId, "flac");

                    //                    link = link.Replace("/Audition/", "/Audio/").Replace(".mp3", "");
                    //                    if (j["downloadUrl"] != null)
                    //                    {
                    //                        if (j["downloadUrl"].ToString().StartsWith("http"))
                    //                        {
                    //                            link = j["downloadUrl"].ToString();
                    //                        }
                    //                    }
                    //                    if (link.Contains(".flac"))
                    //                    {
                    //                        song.FlacUrl = link;
                    //                    }
                    //                    else if (link.Contains(".ape"))
                    //                    {
                    //                        song.ApeUrl = link;
                    //                    }
                    //                    else
                    //                    {
                    //                        song.WavUrl = link;
                    //                    }
                }
                list.Add(song);
            }
            return list;
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
            return null;
        }

        public List<SongResult> CollectSearch(string id, int page, int size)
        {
            return null;
        }

        public List<SongResult> GetSingleSong(string id)
        {
            return null;
        }

        public string GetSongUrl(string id, string quality, string format)
        {
            return GetUrl(id, quality, format);
        }

        private static string GetUrl(string id, string quality, string format)
        {
            var data =
                "{\"content\":{\"musicId\":\"" + id + "\"},\"header\":{\"sonySelectId\":\"ffffffff-ffff-ffff-ffff-ffffffffffff\",\"sdkNo\":22,\"manufacturer\":\"oppo\",\"imei\":\"865166029384834\",\"channel\":\"xiaomi\",\"model\":\"r8207\",\"version\":\"2.2.6\"}}";

            var html = CommonHelper.PostData("https://api.sonyselect.com.cn/streaming/music/get_detail/v1/android",
                new Dictionary<string, string>
                {
                    {"JSON", data}
                }, 1);
            var json = JObject.Parse(html)["content"];
            if (format == "jpg")
            {
                return json["icon"].Value<string>();
            }
            var link = CommonHelper.DecodeBase64(Encoding.UTF8, quality);
            link = link.Replace("", "").Replace("", "").Replace("", "");
            return link;
        }



    }
}