using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using YaVipCore.Helper;

namespace YaVipCore.Api.Video
{
    public class QyVip
    {
        //14  1280*536
        //10  3840*1608
        //5   1920*1080
        //4   1280*536
        //2   1200*540
        //1   856*360
        //96  512*216

        public static string GetUrl(string vid, string quality, string format)
        {
            var url = "http://www.iqiyi.com/v_"+vid+".html";
            var html = CommonHelper.GetHtmlContent(url);
            var match = Regex.Match(html,
                @"(?<=data-player-ismember="")([^""]+)(?:""[\s\S]+?videoid="")([^""]+)(?:""\s*data-player-tvid="")([^""]+)(?:""\s*data-player-albumid="")([^""]+)(?="")");
            var videoid = match.Groups[2].Value;
            var tvid = match.Groups[3].Value;

            if (format == "mp4")
            {
                return GetWxUrl(tvid, videoid, quality, "mp4");
            }
            return GetIpadM3U8(tvid, videoid, quality, format);
        }

        private static string GetWxUrl(string tvid, string videoid, string quality, string format)
        {
            if (format == "mp4")
            {
                quality = quality == "sd" ? "1" : "2";
            }
            else
            {
                switch (quality)
                {
                    case "bhd":
                        quality = "18";
                        break;
                    case "fhd":
                        quality = "5";
                        break;
                    case "shd":
                        quality = "4";
                        break;
                    case "hd":
                        quality = "3";
                        break;
                    default:
                        quality = "2";
                        break;
                }
                format = "m3u8";
            }

            var input = $"/tmts/{tvid}/{videoid}/?uid=&qyid=38a8b96e11fb1c223d3185eade32e6f9&agenttype=28&type={format}&nolimit=&k_ft1=1&rate={quality}&qdv=1&qdx=n&qdy=x&qds=0&__jsT=sgve&t={CommonHelper.GetTimeSpan(true)}&src=02020031010000000000";

            var url = "http://cache.m.iqiyi.com" + input + "&vf=" + CommonHelper.GetCmd5(input);
            var html = CommonHelper.GetHtmlContent(url, 2, new Dictionary<string, string>
            {
                {"Cookie", "P00001=95IOzlwXGhF77v7oWN53PtKuDJiU5jfZFpqlzbOQz1j5Pu8I6e187"}
            });
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            return JObject.Parse(html)["data"]["m3u"].ToString();
        }

        private static string GetIpadM3U8(string tvid, string videoid, string quality, string format)
        {

            var input = $"/jp/tmts/{tvid}/{videoid}/?uid=2349381937&cupid=qc_100001_100103&src=03020031010000000000&platForm=h5&qdv=1&qdx=n&qdy=x&qds=0&__jsT=sgve&t={CommonHelper.GetTimeSpan(true)}&type={format}&qypid={tvid}_03030021010000000000";

            var url = "http://cache.m.iqiyi.com" + input + "&vf=" + CommonHelper.GetCmd5(input);
            var html = CommonHelper.GetHtmlContent(url, 6, new Dictionary<string, string>
            {
                {"Cookie", "P00001=95IOzlwXGhF77v7oWN53PtKuDJiU5jfZFZ3P3ilHEMf5Pu8I6e187"}
            });
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            html = html.Replace("var tvInfoJs=", "");
            var datas = JObject.Parse(html)["data"]["vidl"];
            var dic = datas.ToDictionary(jToken => jToken["vd"].Value<int>(), jToken => jToken["m3u"].ToString());
            string link;
            switch (quality)
            {
                case "bhd":
                    if (dic.ContainsKey(19))
                    {
                        link = dic[19];
                    }
                    else if (dic.ContainsKey(10))
                    {
                        link = dic[10];
                    }
                    else if (dic.ContainsKey(5))
                    {
                        link = dic[5];
                    }
                    else if (dic.ContainsKey(4))
                    {
                        link = dic[4];
                    }
                    else if (dic.ContainsKey(14))
                    {
                        link = dic[14];
                    }
                    else if (dic.ContainsKey(21))
                    {
                        link = dic[21];
                    }
                    else if (dic.ContainsKey(2))
                    {
                        link = dic[2];
                    }
                    else if (dic.ContainsKey(1))
                    {
                        link = dic[1];
                    }
                    else
                    {
                        link = dic[96];
                    }
                    break;
                case "fhd":
                    if (dic.ContainsKey(5))
                    {
                        link = dic[5];
                    }
                    else if (dic.ContainsKey(4))
                    {
                        link = dic[4];
                    }
                    else if (dic.ContainsKey(14))
                    {
                        link = dic[14];
                    }
                    else if (dic.ContainsKey(21))
                    {
                        link = dic[21];
                    }
                    else if (dic.ContainsKey(2))
                    {
                        link = dic[2];
                    }
                    else if (dic.ContainsKey(1))
                    {
                        link = dic[1];
                    }
                    else
                    {
                        link = dic[96];
                    }
                    break;
                case "shd":
                    if (dic.ContainsKey(4))
                    {
                        link = dic[4];
                    }
                    else if (dic.ContainsKey(14))
                    {
                        link = dic[14];
                    }
                    else if (dic.ContainsKey(21))
                    {
                        link = dic[21];
                    }
                    else if (dic.ContainsKey(2))
                    {
                        link = dic[2];
                    }
                    else if (dic.ContainsKey(1))
                    {
                        link = dic[1];
                    }
                    else
                    {
                        link = dic[96];
                    }
                    break;
                case "hd":
                    if (dic.ContainsKey(21))
                    {
                        link = dic[21];
                    }
                    else if (dic.ContainsKey(2))
                    {
                        link = dic[2];
                    }
                    else if (dic.ContainsKey(1))
                    {
                        link = dic[1];
                    }
                    else
                    {
                        link = dic[96];
                    }
                    break;
                default:
                    link = dic.ContainsKey(1) ? dic[1] : dic[96];
                    break;
            }

            return link;
        }


        //private static string GetVipM3U8(string tvid, string videoid, string quality)
        //{
        //    var html =
        //        CommonHelper.GetHtmlContent(
        //            $"http://cache.video.qiyi.com/vps?tvid={tvid}&vid={videoid}&um=0&pf=b6c13e26323c537d&thdk=&thdt=&rs=1&k_tag=1&qdv=1&v=1&src=3_31_312&vf=e7f5c5c1d2e9dc99330b6bc82a200aa4");
        //    if (string.IsNullOrEmpty(html))
        //    {
        //        return null;
        //    }
        //    var datas = JObject.Parse(html)["data"]["vp"]["tkl"].First["vs"];
        //    var dic = datas.ToDictionary(jToken => jToken["bid"].Value<int>(), jToken => jToken["m3u8Url"].ToString());
        //    string link;
        //    switch (quality)
        //    {
        //        case "bhd":
        //            if (dic.ContainsKey(10))
        //            {
        //                link = dic[10];
        //            }
        //            else if (dic.ContainsKey(5))
        //            {
        //                link = dic[5];
        //            }
        //            else if (dic.ContainsKey(4))
        //            {
        //                link = dic[4];
        //            }
        //            else if (dic.ContainsKey(14))
        //            {
        //                link = dic[14];
        //            }
        //            else if (dic.ContainsKey(2))
        //            {
        //                link = dic[2];
        //            }
        //            else if (dic.ContainsKey(1))
        //            {
        //                link = dic[1];
        //            }
        //            else
        //            {
        //                link = dic[96];
        //            }
        //            break;
        //        case "fhd":
        //            if (dic.ContainsKey(5))
        //            {
        //                link = dic[5];
        //            }
        //            else if (dic.ContainsKey(4))
        //            {
        //                link = dic[4];
        //            }
        //            else if (dic.ContainsKey(14))
        //            {
        //                link = dic[14];
        //            }
        //            else if (dic.ContainsKey(2))
        //            {
        //                link = dic[2];
        //            }
        //            else if (dic.ContainsKey(1))
        //            {
        //                link = dic[1];
        //            }
        //            else
        //            {
        //                link = dic[96];
        //            }
        //            break;
        //        case "shd":
        //            if (dic.ContainsKey(4))
        //            {
        //                link = dic[4];
        //            }
        //            else if (dic.ContainsKey(14))
        //            {
        //                link = dic[14];
        //            }
        //            else if (dic.ContainsKey(2))
        //            {
        //                link = dic[2];
        //            }
        //            else if (dic.ContainsKey(1))
        //            {
        //                link = dic[1];
        //            }
        //            else
        //            {
        //                link = dic[96];
        //            }
        //            break;
        //        case "hd":
        //            link = dic.ContainsKey(1) ? dic[1] : dic[96];
        //            break;
        //        default:
        //            link = dic[96];
        //            break;
        //    }

        //    return GetDefaultM3U8Url().Replace("qd_originate=tmts_py", "qd_originate=tmts")
        //        .Replace("bossStatus=2", "bossStatus=0")
        //        .Replace("/20161209/d2/9c/00ba3446725fa427e5444b68e12c7bc2.m3u8", link);
        //}

        //private static string GetDefaultM3U8Url()
        //{
        //    var html = CommonHelper.GetHtmlContent("http://cache.m.ptqy.gitv.tv/tmts/581712200/78a24e48203bf727016f707dae2c3e62/?t=1467015313979&sc=52315044ac5964e099d7c72590ece71e&src=76f90cbd92f94a2e925d83e8ccd22cb7");
        //    return JObject.Parse(html)["data"]["m3utx"].ToString();
        //}

    }
}