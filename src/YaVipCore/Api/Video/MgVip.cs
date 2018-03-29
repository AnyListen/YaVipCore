using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using YaVipCore.Helper;

namespace YaVipCore.Api.Video
{
    public class MgVip
    {
        public static string GetUrl(string vid, string quality, string format)
        {
            var html = CommonHelper.GetHtmlContent("http://pcweb.api.mgtv.com/player/video?video_id=" + vid);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            var json = JObject.Parse(html);
            if (json["code"].ToString() != "200")
            {
                return null;
            }
            var dic = new Dictionary<int, string>();
            foreach (JToken jToken in json["data"]["stream"])
            {
                switch (jToken["name"].ToString())
                {
                    case "蓝光":
                        dic.Add(3, jToken["url"].ToString());
                        break;
                    case "超清":
                        dic.Add(2, jToken["url"].ToString());
                        break;
                    case "高清":
                        dic.Add(1, jToken["url"].ToString());
                        break;
                    default:
                        dic.Add(0, jToken["url"].ToString());
                        break;
                }
            }
            string url;
            if (dic.Count <= 0)
            {
                return null;
            }
            switch (quality)
            {
                case "bhd":
                case "fhd":
                    url = dic[dic.Keys.Count - 1];
                    break;
                case "shd":
                    if (dic.Keys.Count >= 3)
                    {
                        url = dic[2];
                    }
                    else if (dic.Keys.Count == 2)
                    {
                        url = dic[1];
                    }
                    else
                    {
                        url = dic[0];
                    }
                    break;
                case "hd":
                    url = dic.Keys.Count == 2 ? dic[1] : dic[0];
                    break;
                default:
                    url = dic[0];
                    break;
            }
            html = CommonHelper.GetHtmlContent("http://disp.titan.mgtv.com" + url);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            json = JObject.Parse(html);
            var match = Regex.Match(WebUtility.UrlDecode(json["info"].ToString()), @"(?<=http://[^/]+)(/[\s\S]+?/([^_]+)[^/]+_mp4)");
            if (!string.IsNullOrEmpty(format) && format == "m3u8")
            {
                match = Regex.Match(WebUtility.UrlDecode(json["info"].ToString()), @"(?<=http://[^/]+)(/[\s\S]+?/([^_]+)[^/]+_mp4/\w+.m3u8)");
                return
                "http://disp.titan.mgtv.com/vod.do?fmt=4&pno=1031&fid=" + match.Groups[2].Value + "&file=" +
                match.Groups[1].Value;
            }
            return
                "http://disp.titan.mgtv.com/vod.do?fmt=4&pno=1031&fid=" + match.Groups[2].Value + "&file=" +
                match.Groups[1].Value.Replace("_mp4", ".mp4");
        }
    }
}