using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YaVipCore.Helper;

namespace YaVipCore.Api.Video
{
    public class BfVip
    {
        public static string GetUrl(string size, string iid)
        {
            const string cookie =
                "bfsid=f3871d78075f11e6b97eec388f6d4e78; bfcsid=MD8D8klMLox4zP52qNnuIw%3D%3D; loginToken=39KSnGjyYVOwBEoIJ3O12NRC6ER6CbwKT2NB2chOW0075ltdA917QoiwjGsuhc947YqJxoD7MWdfOPmlZ2i_mbl-kh95k6ZUmyOK-13ECLvFY1tBuQKkslK7la9Mq60g-PwVx4uQ-R7WHBBG4_UnckFKqZ0uVetsoenG9DvPehz78zjQm-7g8yCVyp4O68XKB9rH9eosNck6dfG2WInX3HeeZkgGC9-ErioFjHLWLHEp21YAsr8Snt_AOqby0743mUJPox9--0KdakIuT1p_MT03W87Bx56HQeKSP1Wke4saFNT90IxMC1GVi3-0uKst; st=Eg17MfrNXdwJMr_ZT-xrG5fqjuDrjyZNdkuu_njztpgAV19GPmxVuoUN-PT5nFrsKePtgKteMmjuRoRFu_-aHYVdAnZydPfLPfhCeNKb65cDcn2q3SibvBcQA55phnPr; SSOStatus=1463793937; bfuid=135601920096767035; bfuname=1346_9521_6; bf_user_name=1346_9521_6; bfmbind=1";
            var url = "http://rd.p2p.baofeng.net/queryvp.php?type=3&gcid=" + iid + "&callback=_callbacks_._5ijo0ago";
            var html = CommonHelper.GetHtmlContent(url, 0, new Dictionary<string, string>
            {
                {"Cookie", cookie}
            });
            for (var i = 0; i < 10; i++)
            {
                if (!string.IsNullOrEmpty(html))
                {
                    break;
                }
                html = CommonHelper.GetHtmlContent(url, 0 , new Dictionary<string, string>
                {
                    {"Cookie", cookie}
                });
            }
            var match = Regex.Match(html,
                "(?<=ip':')([^']+)(?:','port':')([^']+)(?:','path':')([^']+)(?:','key':')([^']+)");
            var playUrl = "http://" + GetIp(match.Groups[1].ToString().Trim()) + ":" + match.Groups[2].Value + "/" +
                          match.Groups[3].Value + "?key=" + match.Groups[4].Value + "&filelen=" + size;
            return playUrl;
        }

        private static string GetIp(string ip)
        {
            var dic = new Dictionary<string, string>
            {
                {"b", "0"},
                {"a", "1"},
                {"o", "2"},
                {"f", "3"},
                {"e", "4"},
                {"n", "5"},
                {"g", "6"},
                {"h", "7"},
                {"t", "8"},
                {"m", "9"},
                {"l", "."},
                {"c", "A"},
                {"p", "B"},
                {"z", "C"},
                {"r", "D"},
                {"y", "E"},
                {"s", "F"}
            };
            var c = ip.Split(',');
            var j = new List<string>();
            foreach (var r in c)
            {
                var cd = r.Length;
                var temp = "";
                for (var fb = 0; fb < cd; fb++)
                {
                    temp += dic[r.Substring(fb, 1)];
                }
                j.Add(temp);
            }
            return j[new Random(DateTime.Now.Millisecond).Next(0, j.Count)];
        }
    }
}