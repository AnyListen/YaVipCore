using System.Text.RegularExpressions;
using YaVipCore.Helper;

namespace YaVipCore.Api.Video
{
    public class LeVip
    {

        public static string GetUrl(string vid, string quality, string format)
        {
            var url =
                "http://tvepg.letv.com/apk/data/common/security/playurl/geturl/byvid.shtml?vid="+vid+"&platid=6&key=$KEY$&vtype=";
            switch (quality)
            {
                case "bhd":
                case "fhd":
                    url += "18";
                    break;
                case "shd":
                    url += "17";
                    break;
                case "hd":
                    url += "16";
                    break;
                default:
                    url += "1";
                    break;
            }
            var key = CommonHelper.Md5(vid + "afiuuehrfdagfif98kfd" + "6");
            url = url.Replace("$KEY$", key);
            var html = CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }
            var link = Regex.Match(html, @"(?<=mainUrl"":"")[^""]+").Value.Replace("\\","");
            if (!string.IsNullOrEmpty(format) && format == "m3u8")
            {
                return link;
            }
            return link.Replace("tss=tvts", "tss=mp4");
        }

        //private static int GenerateKeyRor(int value, int key)
        //{
        //    var i = 0;
        //    while (i < key)
        //    {
        //        value = (0x7fffffff & (value >> 1)) | ((value & 1) << 31);
        //        ++i;
        //    }
        //    return value;
        //}

        //private static int GetTkey()
        //{
        //    var stime = Convert.ToInt32(CommonHelper.GetTimeSpan());
        //    const int key = 185025305;
        //    var value = GenerateKeyRor(stime, key % 17);
        //    value = value ^ key;
        //    return value;
        //}
    }
}