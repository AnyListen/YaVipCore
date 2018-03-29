using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JSPool;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace YaVipCore.Helper
{
    public static class CommonHelper
    {
        public static string IpAddr = "127.0.0.1";
        public static string SignKey = "itwusun.com";
        private static readonly ILogger Logger = new LoggerFactory().AddConsole(LogLevel.Warning).AddNLog().CreateLogger("WebError");

        public static void AddLog(string msg)
        {
            Logger.LogError(msg);
        }

        public static string GetHtmlContent(string url, int userAgent = 0, Dictionary<string, string> headers = null)
        {
            try
            {
                using (var myHttpWebRequest = new HttpClient { Timeout = new TimeSpan(0, 0, 15) })
                {
                    myHttpWebRequest.DefaultRequestHeaders.Add("Method", "GET");
                    myHttpWebRequest.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                    switch (userAgent)
                    {
                        case 1:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 9_3_2 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Mobile/13F69 MicroMessenger/6.3.16 NetType/WIFI Language/zh_CN");
                            break;
                        case 2:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; U; Android 2.2; en-gb; GT-P1000 Build/FROYO) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1");
                            break;
                        case 3:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows Phone 10.0; Android 4.2.1; NOKIA; Lumia 930) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Mobile Safari/537.36 Edge/13.10586");
                            break;
                        case 4:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "NativeHost");
                            break;
                        case 5:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Dalvik/1.6.0 (Linux; U; Android 4.4.2; NoxW Build/KOT49H) ITV_5.7.1.46583");
                            break;
                        case 6:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "qqlive");
                            break;
                        case 7:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Dalvik/1.6.0 (Linux; U; Android 4.2.2; 6S Build/JDQ39E)");
                            break;
                        case 8:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) XIAMI-MUSIC/3.0.2 Chrome/51.0.2704.106 Electron/1.2.8 Safari/537.36");
                            break;
                        default:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36");
                            break;
                    }
                    if (headers != null)
                    {
                        foreach (var k in headers)
                        {
                            myHttpWebRequest.DefaultRequestHeaders.Add(k.Key, k.Value);
                        }
                    }
                    return myHttpWebRequest.GetStringAsync(url).Result;
                }
            }
            catch (Exception ex)
            {
                AddLog(ex.ToString());
                return null;
            }
        }

        public static string PostData(string url, Dictionary<string, string> data, int contentType = 0, int userAgent = 0, Dictionary<string, string> headers = null, bool isDecode = true)
        {
            try
            {
                using (var myHttpWebRequest = new HttpClient { Timeout = new TimeSpan(0, 0, 10) })
                {
                    myHttpWebRequest.DefaultRequestHeaders.Add("Method", "POST");
                    myHttpWebRequest.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                    myHttpWebRequest.DefaultRequestHeaders.Add("ContentType",
                        userAgent == 0 ? "application/x-www-form-urlencoded" : "application/json;charset=UTF-8");
                    switch (userAgent)
                    {
                        case 1:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 5_0 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9A334 Safari/7534.48.3");
                            break;
                        case 2:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 4.0.4; Galaxy Nexus Build/IMM76B) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.133 Mobile Safari/535.19");
                            break;
                        case 3:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; IEMobile/10.0; ARM; Touch; NOKIA; Lumia 920)");
                            break;
                        case 4:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "NativeHost");
                            break;
                        case 5:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Apache-HttpClient/UNAVAILABLE (java 1.4)");
                            break;
                        case 6:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPad; CPU OS 8_1_3 like Mac OS X) AppleWebKit/600.1.4 (KHTML, like Gecko) Version/8.0 Mobile/12B466 Safari/600.1.4");
                            break;
                        default:
                            myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36");
                            break;
                    }
                    if (headers != null)
                    {
                        foreach (var k in headers)
                        {
                            myHttpWebRequest.DefaultRequestHeaders.Add(k.Key, k.Value);
                        }
                    }
                    var response = contentType == 0
                        ? myHttpWebRequest.PostAsync(url, new FormUrlEncodedContent(data)).Result
                        : myHttpWebRequest.PostAsync(url, new StringContent(data[data.Keys.First()], Encoding.UTF8)).Result;
                    return response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception ex)
            {
                AddLog(ex.ToString());
                return null;
            }
        }

        public static string GetPostCookie(string url, Dictionary<string,string> data)
        {
            try
            {
                var webRequestHandler = new HttpClientHandler {AllowAutoRedirect = false};
                var myHttpWebRequest = new HttpClient(webRequestHandler) { Timeout = new TimeSpan(0, 0, 15) };
                myHttpWebRequest.DefaultRequestHeaders.Add("Method", "POST");
                return myHttpWebRequest.PostAsync(url, new FormUrlEncodedContent(data)).Result.Headers.GetValues("Set-Cookie").Aggregate("", (current, c) => current + c + ";");
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetHtmlCookie(string url, string cookie)
        {
            try
            {
                var myHttpWebRequest = new HttpClient { Timeout = new TimeSpan(0, 0, 15) };
                myHttpWebRequest.DefaultRequestHeaders.Add("Method", "GET");
                myHttpWebRequest.DefaultRequestHeaders.Add("Cookie", cookie);
                myHttpWebRequest.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                myHttpWebRequest.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) XIAMI-MUSIC/3.0.2 Chrome/51.0.2704.106 Electron/1.2.8 Safari/537.36");
                return myHttpWebRequest.GetAsync(url).Result.Headers.GetValues("Set-Cookie").Aggregate("", (current, c) => current + c + ";");
            }
            catch (Exception ex)
            {
                AddLog(ex.ToString());
                return null;
            }
        }

        public static string GetSongUrl(string type, string quality, string id, string format)
        {
            var key = type + "_" + quality + "_" + id + "." + format;
            var md5 = Md5(key + SignKey);
            return "http://" + IpAddr + "/music/ymusic/" + key + "?sign=" + md5;   //需要将yourdomain替换成IP或者域名
        }

        public static string GetPaperUrl(string type, string webUrl)
        {
            var url = EncodeBase64(Encoding.UTF8, webUrl).Replace("=", "%3D");
            var md5 = Md5(type + SignKey + webUrl);
            return "http://" + IpAddr + "/paper/fulltext?t=" + type + "&u=" + url + "&s=" + md5;
        }

        public static string Md5(string input)
        {
            using (var md5 = MD5.Create())
            {
                var strs = Encoding.UTF8.GetBytes(input);
                var output = md5.ComputeHash(strs);
                return BitConverter.ToString(output).Replace("-", "").ToLower();
            }
        }

        public static string Sha1(string str)
        {
            using (var sha1 = SHA1.Create())
            {
                var strs = Encoding.UTF8.GetBytes(str);
                var output = sha1.ComputeHash(strs);
                return BitConverter.ToString(output).Replace("-", "").ToLower();
            }
        }
        public static long GetTimeSpan(bool isMills = false)
        {
            var startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64((DateTime.UtcNow.Ticks - startTime.Ticks) / (isMills ? 10000 : 10000000));
        }

        public static DateTime UnixTimestampToDateTime(long timestamp)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return start.AddSeconds(timestamp);
        }

        public static string NumToTime(string originalTime)
        {
            if (originalTime.Contains("."))
            {
                originalTime = originalTime.Split('.')[0].Trim();
            }
            if (string.IsNullOrEmpty(originalTime))
            {
                return "00:00";
            }
            var num = Convert.ToInt32(originalTime);
            var mins = num / 60;
            var seds = num % 60;
            string time;
            if (mins.ToString(CultureInfo.InvariantCulture).Length == 1)
            {
                time = @"0" + mins;
            }
            else
            {
                time = mins.ToString(CultureInfo.InvariantCulture);
            }
            time += ":";
            if (seds.ToString(CultureInfo.InvariantCulture).Length == 1)
            {
                time += @"0" + seds;
            }
            else
            {
                time += seds.ToString(CultureInfo.InvariantCulture);
            }
            return time;
        }

        public static int TimeToNum(string originalTime)
        {
            originalTime = Regex.Replace(originalTime, "[^\\d:]+", "");
            var arr = originalTime.Split(new[] {':', '.'}, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length == 3)
            {
                return Convert.ToInt32(arr[0]) * 3600 + Convert.ToInt32(arr[1]) * 60 + Convert.ToInt32(arr[2]);
            }
            return Convert.ToInt32(arr[0])*60 + Convert.ToInt32(arr[1]);
        }

        public static string UnicodeToString(string input)
        {
            var matches = Regex.Matches(input, @"\\u([0-9a-f]{4})");
            foreach (Match match in matches)
            {
                var str = (char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber);
                input = input.Replace(match.Value, str.ToString());
            }
            return input;
        }

        public static string DecodeBase64(Encoding encode, string result)
        {
            return encode.GetString(Convert.FromBase64String(result));
        }

        public static string EncodeBase64(Encoding encode, string source)
        {
            return Convert.ToBase64String(encode.GetBytes(source));
        }

        public static string HtmlDecode(this string html)
        {
            return WebUtility.HtmlDecode(html);
        }


        private static JsPool _myJsPool;

        public static void InitJsPool()
        {
            JsEngineSwitcher.Instance.EngineFactories.AddChakraCore();
            JsEngineSwitcher.Instance.DefaultEngineName = ChakraCoreJsEngine.EngineName;
            _myJsPool = new JsPool(new JsPoolConfig
            {
                Initializer = initEngine =>
                {
                    initEngine.ExecuteFile(Path.Combine(ApplicationEnvironment.ApplicationBasePath, "cmd5.js"),
                        Encoding.UTF8);
                }
            });
        }

        public static string GetCmd5(string input)
        {
            var engine = _myJsPool.GetEngine();
            var message = engine.CallFunction<string>("cmd5x", input);
            _myJsPool.ReturnEngineToPool(engine);
            _myJsPool.Dispose();
            Console.WriteLine();
            return message;
        }

        public static bool CompareStr(string s1, string s2)
        {
            s1 = s1.ToLower();
            s2 = s2.ToLower();
            s1 = Regex.Replace(s1, "\\W+", "");
            s2 = Regex.Replace(s2, "\\W+", "");
            return s1 == s2;
        }

    }
}