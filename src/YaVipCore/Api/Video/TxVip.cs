using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using YaVipCore.Helper;

namespace YaVipCore.Api.Video
{
    public class TxVip
    {
        private const string TxCookie = "sd_userid=9385148746019; pac_uid=1_584586119; tvfe_boss_uuid=5a8990ded12342; RK=4D2KuOqPUj; uid=2605259; guid=10z56z123456789qwertyuiop; _qpsvr_localtk=0.030312426657994607; pt2gguin=o05886119; uin=o0584119; skey=@65KK06j; ptisp=cm; luin=o05586119; lskey=0001000061548cdb1a71ce340b6148a3441392e27b82c29ad98c608dbc794e5ce7bc3cb3bed4d; ptcz=62c0b9f60362d443e289c5e337a0273109193f2f; main_login=qq; o_cookie=5845119; pgv_info=ssid=s73745712; pgv_pvid=5286720";

        /// <summary>
        /// 获取视频地址
        /// </summary>
        /// <param name="vid"></param>
        /// <param name="quality">
        /// fhd 蓝光;(1080P)
        /// shd 超清;(720P)
        /// hd 高清;(480P)
        /// sd 标清;(270P)
        /// </param>
        /// <param name="format">类型</param>
        /// <returns></returns>
        public static string GetUrl(string vid, string quality, string format)
        {
            #region TV版代码，已启用

            //const string priKeyStr =
            //    "MIICdwIBADANBgkqhkiG9w0BAQEFAASCAmEwggJdAgEAAoGBALOVeoEZMi4RPw82\noaS6otfKY483iU2fMfZUN8K2IYUF8FOUy97HYmrbHjO1hIMMpY9HMUcqqtXz7F8P\n06Wwv7VuxozO49TCLfvhp7dbfz6aqK4NgaU0s2K2L5oeAS1BL2ScJDDdtg37GIFr\nu86r4z+RTTsSdv9+N8fTbNUQ2bm5AgMBAAECgYAGI1XMk8/jQzOkkXl05+wo9AHz\nIzLONGLAyKAfR5pdsZZFRRCyzJ3QiSy/F7UvxX7jJsvIYuzz4yJxHVlekGv8+ONT\n32MsGfNfjpsTY7FCZ9cApXKVKMGzQFYK989K4hEd+3N+q2Osw7CRCpPqvHgZBqgT\n3SxJi5dobuRAqIw8AQJBANq0rkwAMEJiJxq9Qqy1nfrAesPkZz4HYUJJG6rgNJsM\nt+Hl2OE5BZNYq1v/sMBhBfVPv+NadfmgoKnqiEB8Ri0CQQDSNPt1SLrFV4tocYrE\nMvzXhHzWC5KcSbgTwKFwIKndcyBR2J2ZekW8Ky5jSE8/TJGF69Ja+trnOW+VxgI1\nSqU9AkAYTsSghdTXS/l0q1xhvb3VRNdgNl6TMlbI+z8r+sdeBEfbv6QfRCsueUhy\nbTTD7QSwgzCcoE1EdWnl+L80C5vxAkEArZz00rlvCO51RZ4BbmpuSdIzCNYmEM8S\nKb4/l8xif3RGjVLLV6eVUQSZG4btbOpghqtu4ZWulqrpblpMGJe+QQJBAK2zLson\nFRi4OBlTFJZbH4hYhrqNo5WWnIFtC5qG4xu/yCcpq/pB2jr8KF/GmF07Ug29iiz/\nnL/s5dZXm4eLvhI=\n";
            //var privateKey = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(priKeyStr));
            //var cipher = CipherUtilities.GetCipher("RSA/NONE/PKCS1Padding");
            //cipher.Init(true, privateKey);
            ////var text = string.Format("vid:{0}[{1}];aVer:V3.9.245.1500;guid:06908DED00E081E142ED006B56167EA5", vid, CommonHelper.GetTimeSpan());
            //var text = string.Format("vid:{0}[{1}];aVer:V4.0.245.1601;guid:1EDA8B535163C7B51F85371E1706A721", vid, CommonHelper.GetTimeSpan());
            //var result = cipher.DoFinal(Encoding.UTF8.GetBytes(text));
            //var ckey = WebUtility.UrlEncode(@"++90" + Convert.ToBase64String(result));
            //if (quality == "bhd")
            //{
            //    quality = "fhd";
            //}

            //var url = string.Format("http://tv.ott.video.qq.com/openqq/getvinfo?sphls=1&oauth_consumer_key=101249687&platform=920603&openid=C5EADBD01F4B16491984BD643AE8C7C0&appVer=V4.0.245.1601&defn={0}&thirdAppVer=4.1.1.1&clip=0&randnum=0.8221596940507854&play_start_time=0&pf=qzone&device=26&sdtfrom=8057&encryptVer=4.1&access_token=3571974D9E045D15A0799C100DA2C2B0&otype=json&guid=1EDA8B535163C7B51F85371E1706A721&cKey={1}&newplatform=920603&charge=1&play_end_time=0&dtype=3&vid={2}", quality, ckey, vid);

            #endregion

            if (quality == "bhd")
            {
                quality = "fhd";
            }
            var ts = CommonHelper.GetTimeSpan();
            var soltStr = CommonHelper.Md5("10801" + vid + ts + "#$#@#*ad" + "v4138" + "heherand");
            var url =
                $"https://h5vv.video.qq.com/getinfo?encver=2&_qv_rmtv2={soltStr}&defn={quality}&platform=10801&otype=json&sdtfrom=v4138&_rnd={ts}&appVer=0.2.1&dtype=3&vid={vid}&newnettype=1";
            var html = CommonHelper.GetHtmlContent(url, 0, new Dictionary<string, string> {{"Cookie", TxCookie}});
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }
            var json = JObject.Parse(html.Replace("QZOutputJson=", "").TrimEnd(';'));
            var j = json["vl"]["vi"].First["ul"]["ui"].Last;
            return j["url"].ToString() + j["hls"]["pname"] +
                   (j["hls"]["pname"].ToString().EndsWith(".ts") ? ".m3u8?ver=4" : "");
        }

    }
}