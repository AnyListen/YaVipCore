using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using AnyListen.Models;
using Newtonsoft.Json.Linq;

namespace FishMusic.Helper
{
    public class CommonHelper
    {

        public static string GetDownloadUrl(SongResult song, int bitRate, int prefer, bool isFormat)
        {
            string link;
            switch (bitRate)
            {
                case 0:
                    switch (prefer)
                    {
                        case 0:
                            if (!string.IsNullOrEmpty(song.FlacUrl))
                            {
                                link = song.FlacUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.ApeUrl))
                            {
                                link = song.ApeUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.WavUrl))
                            {
                                link = song.WavUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.SqUrl))
                            {
                                link = song.SqUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.HqUrl))
                            {
                                link = song.HqUrl;
                            }
                            else
                            {
                                link = song.LqUrl;
                            }
                            break;
                        case 1:
                            if (!string.IsNullOrEmpty(song.ApeUrl))
                            {
                                link = song.ApeUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.FlacUrl))
                            {
                                link = song.FlacUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.WavUrl))
                            {
                                link = song.WavUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.SqUrl))
                            {
                                link = song.SqUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.HqUrl))
                            {
                                link = song.HqUrl;
                            }
                            else
                            {
                                link = song.LqUrl;
                            }
                            break;
                        default:
                            if (!string.IsNullOrEmpty(song.WavUrl))
                            {
                                link = song.WavUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.FlacUrl))
                            {
                                link = song.FlacUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.ApeUrl))
                            {
                                link = song.ApeUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.SqUrl))
                            {
                                link = song.SqUrl;
                            }
                            else if (!string.IsNullOrEmpty(song.HqUrl))
                            {
                                link = song.HqUrl;
                            }
                            else
                            {
                                link = song.LqUrl;
                            }
                            break;
                    }
                    break;
                case 1:
                    if (!string.IsNullOrEmpty(song.SqUrl))
                    {
                        link = song.SqUrl;
                    }
                    else if (!string.IsNullOrEmpty(song.HqUrl))
                    {
                        link = song.HqUrl;
                    }
                    else
                    {
                        link = song.LqUrl;
                    }
                    break;
                case 2:
                    if (!string.IsNullOrEmpty(song.HqUrl))
                    {
                        link = song.HqUrl;
                    }
                    else if (!string.IsNullOrEmpty(song.SqUrl))
                    {
                        link = song.SqUrl;
                    }
                    else
                    {
                        link = song.LqUrl;
                    }
                    break;
                default:
                    link = song.LqUrl;
                    break;
            }
            if (isFormat)
            {
                if (link.ToLower().Contains(".flac"))
                {
                    link = "flac";
                }
                else if (link.ToLower().Contains(".ape"))
                {
                    link = "ape";
                }
                else if (link.ToLower().Contains(".wav"))
                {
                    link = "wav";
                }
                else if (link.ToLower().Contains(".ogg"))
                {
                    link = "ogg";
                }
                else if (link.ToLower().Contains(".aac"))
                {
                    link = "acc";
                }
                else if (link.ToLower().Contains(".wma"))
                {
                    link = "wma";
                }
                else
                {
                    link = "mp3";
                }
            }
            return link;
        }

        public static string GetFormat(string url)
        {
            string link;
            if (url.ToLower().Contains(".flac"))
            {
                link = "flac";
            }
            else if (url.ToLower().Contains(".ape"))
            {
                link = "ape";
            }
            else if (url.ToLower().Contains(".wav"))
            {
                link = "wav";
            }
            else if (url.ToLower().Contains(".ogg"))
            {
                link = "ogg";
            }
            else if (url.ToLower().Contains(".aac"))
            {
                link = "acc";
            }
            else if (url.ToLower().Contains(".wma"))
            {
                link = "wma";
            }
            else
            {
                link = "mp3";
            }
            return "." + link;
        }

        public static string NumToTime(int num)
        {
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

        public static string RemoveSpecialChar(string input)
        {
            return Regex.Replace(input, @"[?:*""<>|\/]", "");
        }


        public static string GetLrc(string name, string artist, int length)
        {
            var url = "http://lyrics.kugou.com/search?ver=1&man=yes&client=pc&keyword=" + artist + "-" +
                          name + "&duration=" + length + "&hash=";
            var html = AnyListen.Helper.CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }
            var json = JObject.Parse(html);
            if (json["status"].ToString() == "404")
            {
                return "";
            }
            var hash = json["candidates"].First["accesskey"].ToString();
            var mid = json["candidates"].First["id"].ToString();
            url =
                "http://lyrics.kugou.com/download?ver=1&client=pc&id=" + mid + "&accesskey=" + hash + "&fmt=lrc&charset=utf8";
            html = AnyListen.Helper.CommonHelper.GetHtmlContent(url);
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }
            json = JObject.Parse(html);
            var str = DecodeBase64(Encoding.UTF8, json["content"].ToString());
            return str;
        }

        public static string DecodeBase64(Encoding encode, string result)
        {
            return encode.GetString(Convert.FromBase64String(result));
        }

        public static string GetCurrentAssemblyVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            return $"{fvi.ProductMajorPart}.{fvi.ProductMinorPart}.{fvi.ProductBuildPart}";
        }

        public static string SecondsToTime(int total)
        {
            var hour = total / 3600;
            var min = (total % 3600) / 60;
            var sec = (total % 3600) % 60;
            return hour.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0') + ":" +
                   sec.ToString().PadLeft(2, '0');
        }
    }
}