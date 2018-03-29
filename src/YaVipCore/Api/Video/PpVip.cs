using System;
using System.Linq;
using System.Text.RegularExpressions;
using YaVipCore.Helper;

namespace YaVipCore.Api.Video
{
    public class PpVip
    {

        public static string GetUrl(string vid, string quality, string format)
        {
            if (!Regex.IsMatch(vid, @"^\d+$"))
            {
                var content =
                    CommonHelper.GetHtmlContent(
                        "http://mtbu.api.pptv.com/sdk/?link=http%3A%2F%2Fv.pptv.com%2Fshow%2F" + vid +
                        ".html&appid=45cca108edbdc2a844a15f6c94740e97",7);
                if (string.IsNullOrEmpty(content))
                {
                    return null;
                }
                vid = Regex.Match(content, @"(?<=""id"":)\d+").Value;
            }


            //var url = "http://client-play.pptv.com/v3/chplay3-0-" + vid
            //          +
            //          ".xml&param=type%3Dclient.vip%26userType%3D1%26areaType%3D67%26dns%3D16885952%26gslbversion%3D2%26k_ver%3D0.0.0.0%26h265%3D2&zone=8&version=5&username=admin&ppi=302c3637&appplt=clt&appid=PPTVWINCLIENT";

            var url = $"http://play.api.cp61.ott.cibntv.net/webplay3-0-{vid}.xml?channel=161&username=1&type=tv.android&gslbversion=2&ft=2&version=4&userLevel=1&content=need_drag&sv=1.5.1";
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            var files = Regex.Matches(html, @"(?<=rid="")([^""]+)(?:""\s*bitrate[\s\S]+?ft="")(\d)");
            if (files.Count <= 0)
            {
                return null;
            }
            var dic = files.Cast<Match>().ToDictionary(match => Convert.ToInt32(match.Groups[2].Value), match => match.Groups[1].Value);
            string file;
            int index;
            switch (dic.Count)
            {
                case 5:
                    switch (quality)
                    {
                        case "bhd":
                            index = 4;
                            break;
                        case "fhd":
                            index = 3;
                            break;
                        case "shd":
                            index = 2;
                            break;
                        case "hd":
                            index = 1;
                            break;
                        default:
                            index = 0;
                            break;
                    }
                    file = dic[index];
                    break;
                case 4:
                    switch (quality)
                    {
                        case "bhd":
                        case "fhd":
                            index = 3;
                            break;
                        case "shd":
                            index = 2;
                            break;
                        case "hd":
                            index = 1;
                            break;
                        default:
                            index = 0;
                            break;
                    }
                    file = dic[index];
                    break;
                case 3:
                    switch (quality)
                    {
                        case "bhd":
                        case "fhd":
                        case "shd":
                            index = 2;
                            break;
                        case "hd":
                            index = 1;
                            break;
                        default:
                            index = 0;
                            break;
                    }
                    file = dic[index];
                    break;
                case 2:
                    switch (quality)
                    {
                        case "bhd":
                        case "fhd":
                        case "shd":
                        case "hd":
                            index = 1;
                            break;
                        default:
                            index = 0;
                            break;
                    }
                    file = dic[index];
                    break;
                default:
                    index = 0;
                    file = dic[index];
                    break;
            }
            url = "http://jump.synacast.com/"+file+"dt?type=ppbox&key=yyfm";
            html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }
            //var dt = Regex.Match(html, @"(?<=dt\s*ft=""" + index + @""">\s*<sh>)([^<]+)(?:</sh>[\s\S]+?>)([^<]+)(?=</key>)");
            var dt = Regex.Match(html, @"(?<=server_host>)([\d.]+)(?:[\S\s]+?"">)([^<]+)(?=</key>)");
            url = format == "m3u8"
                ? "http://" + dt.Groups[1].Value + "/" + file.Replace(".mp4", ".m3u8") + "?type=ppbox&key=yyfm&k=" +
                  dt.Groups[2].Value
                : "http://" + dt.Groups[1].Value + "/w/" + file + "?type=ppbox&key=yyfm&k=" +
                  dt.Groups[2].Value;
            return url;
        }
    }
}