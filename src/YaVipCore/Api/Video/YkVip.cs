using System;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using YaVipCore.Helper;

namespace YaVipCore.Api.Video
{
    public class YkVip
    {
        public static string Cookie;
        public static string GetUrl(string vid, string quality, string format)
        {
            //var html = CommonHelper.GetHtmlContent($"http://play.youku.com/play/get.json?vid={vid}&ct=62&dv=mobile&uid=766821211");
            //var match = Regex.Match(html, @"(?<=security[\s\S]+?encrypt_string"":"")([^""]+)(?:""[\s\S]+?ip"":)(\d+)");
            //var h = GetJ1(false);
            //var v = GetJ1(true);
            //var res = Rc4(h, Convert.FromBase64String(match.Groups[1].Value), false);
            //var part = res.Split('_');
            //var sid = part[0];
            //var token = part[1];
            //var whole = $"{sid}_{vid}_{token}";
            //var oip = match.Groups[2].Value;
            //var newbytes = Encoding.ASCII.GetBytes(whole);
            //var ep = WebUtility.UrlEncode(Rc4(v, newbytes, true));

            if (quality == "bhd")
            {
                quality = "hd3";
            }
            else if (quality == "fhd")
            {
                quality = "hd3";
            }
            else if (quality == "shd")
            {
                quality = "hd2";
            }
            else if (quality == "hd")
            {
                quality = "mp4";
            }
            else
            {
                quality = "flv";
            }

            var html = CommonHelper.GetHtmlContent( "http://tv.api.3g.youku.com/layout/smarttv/play/detail?id=" + vid + "&guid=54084d39376f0030fab0c54aaf80eb68&pid=a51da0d1b3ac7f85");
            var isNeedPay = true;
            var type = "";
            var name = "";
            if (!string.IsNullOrEmpty(html))
            {
                var json = JObject.Parse(html);
                isNeedPay = json["detail"]["paid"].ToString() == "1";
                name = json["detail"]["title"].ToString();
                type = json["detail"]["tag_type"].ToString();
            }
            if ((isNeedPay || type == "电影") && !string.IsNullOrEmpty(name))
            {
                var pinyin = HanzToPinyin.GetFirstLetter(name);
                if (pinyin.Length > 4)
                {
                    pinyin = pinyin.Substring(0, 4);
                }
                html =
                    CommonHelper.GetHtmlContent(
                        "http://s.epg.ott.cibntv.net/epg/web/v40/program!getSearchProgram.action?searchType=0&parentCatgId=&searchValue=" + pinyin + "&templateId=00080000000000000000000000000050&pageNumber=1&pageSize=100&biType=1&uId=U0177100002234162");
                if (!string.IsNullOrEmpty(html) && html != "{}")
                {
                    var json = JObject.Parse(html);
                    var jArray = json["programList"];
                    JToken result;
                    try
                    {
                        result = jArray.SingleOrDefault(j => (j["name"].ToString().Replace("：", ":") == name && !j["id"].ToString().StartsWith("CIBN")));
                    }
                    catch (Exception)
                    {
                        result = jArray.Last(j => (j["name"].ToString().Replace("：", ":") == name && !j["id"].ToString().StartsWith("CIBN")));
                    }
                    if (result != null)
                    {
                        html =
                            CommonHelper.GetHtmlContent(
                                "http://s.epg.ott.cibntv.net/epg/web/v40/program!getMovieDetail.action?programSeriesId=" +
                                result["id"] +
                                "&templateId=00080000000000000000000000000050");
                        if (!string.IsNullOrEmpty(html))
                        {
                            var link = Regex.Match(html, @"(?<=playurl"":"")[^""]+").Value;
                            if (!string.IsNullOrEmpty(link))
                            {
                                return link;
                            }
                        }
                    }
                }
            }
            html = CommonHelper.GetHtmlContent("https://openapi.youku.com/v2/videos/m3u8.json?client_id=e57bc82b1a9dcd2f&client_secret=a361608273b857415ee91a8285a16b4a&type=play&video_id=" + vid);
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }
            var matches = Regex.Matches(html, @"(?<=mp4"":\["")[^""]+");
            if (matches.Count <= 0)
            {
                return "";
            }
            var url = matches[0].Value.Replace("&type=mp4", "&type=flv").Replace("&type=hd2", "&type=flv").Replace("&type=flv", "&type=" + quality);
            //if (!isNeedPay)
            //{
            //    return url;
            //}

            //html = CommonHelper.GetHtmlContent(url, 1, new Dictionary<string, string>
            //    {
            //        {"Cookie", Cookie}
            //    });
            //return html;
            return url;
        }

        //private static string GetJ1(bool t)
        //{
        //    var strArr = GetK1(t ? "MzczMjMxMzc2" : "MzU2NDY1MzIz", t);
        //    return strArr.Aggregate("", (current, t1) => current + (Convert.ToChar(Convert.ToInt32(t1, 16))));
        //}

        //private static string[] GetK1(string e, bool t)
        //{
        //    var str1 = e + (t ? "NjMxMzAzMw==" : "NTM0NjYzOA==");
        //    var n = CommonHelper.DecodeBase64(Encoding.UTF8, str1);
        //    var strArr = new string[n.Length / 2];
        //    var index = 0;
        //    for (var a = 0; a < n.Length; a += 2)
        //    {
        //        strArr[index] = n.Substring(a, 2);
        //        index++;
        //    }
        //    return strArr;
        //}

        //private static string Rc4(string a, byte[] c, bool isToBase64)
        //{
        //    var result = "";
        //    var bytesR = new List<byte>();
        //    int f = 0, h = 0;
        //    var b = new int[256];
        //    for (var i = 0; i < 256; i++)
        //        b[i] = i;
        //    while (h < 256)
        //    {
        //        f = (f + b[h] + a[h % a.Length]) % 256;
        //        var temp = b[h];
        //        b[h] = b[f];
        //        b[f] = temp;
        //        h++;
        //    }
        //    f = 0; h = 0; var q = 0;
        //    while (q < c.Length)
        //    {
        //        h = (h + 1) % 256;
        //        f = (f + b[h]) % 256;
        //        var temp = b[h];
        //        b[h] = b[f];
        //        b[f] = temp;
        //        byte[] bytes = { (byte)(c[q] ^ b[(b[h] + b[f]) % 256]) };
        //        bytesR.Add(bytes[0]);
        //        result += Encoding.ASCII.GetString(bytes);
        //        q++;
        //    }
        //    if (!isToBase64) return result;
        //    var byteR = bytesR.ToArray();
        //    result = Convert.ToBase64String(byteR);
        //    return result;
        //}
    }
}