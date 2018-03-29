using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.DotNet.PlatformAbstractions;
using Newtonsoft.Json;

namespace YaVipCore.Helper
{
    public class HanzToPinyin
    {
        private static readonly Dictionary<string, string> WordsDictionary;
        static HanzToPinyin()
        {
            //var lines = File.ReadAllLines(Path.Combine(ApplicationEnvironment.ApplicationBasePath, "data", "pinyin.txt"));
            //foreach (var s in lines)
            //{
            //    var arr = s.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //    WordsDictionary.Add(arr[0], arr[1]);
            //}

            //var dir = new DirectoryInfo(Path.Combine(ApplicationEnvironment.ApplicationBasePath, "data"));
            //foreach (var fileInfo in dir.GetFiles())
            //{
            //    var text = File.ReadAllText(fileInfo.FullName, Encoding.UTF8);
            //    var matches = Regex.Matches(text, @"(?<=')(\w+)(?:'\s*=>\s*')([^']+)");
            //    foreach (Match match in matches)
            //    {
            //        WordsDictionary.Add(match.Groups[1].Value, match.Groups[2].Value);
            //    }
            //}

            var text = File.ReadAllText(Path.Combine(ApplicationEnvironment.ApplicationBasePath, "pinyin.txt"), Encoding.UTF8);
            WordsDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
        }

        public static string GetFirstLetter(string input)
        {
            input = input.Split(new[] { ':', '-' }, StringSplitOptions.RemoveEmptyEntries)[0];
            input = Regex.Replace(input, @"[^\u4e00-\u9fa5]", "");
            var strArr = GetFullPinyin(input).Split(new[] {'\t', ' '}, StringSplitOptions.RemoveEmptyEntries);
            return strArr.Aggregate("", (current, s) => current + s[0]).ToUpper();
        }

        private static string GetFullPinyin(string input)
        {
            var result = "";
            var index = 0;
            while (index < input.Length)
            {
                var strLength = 1;
                while (true)
                {
                    var str = input.Substring(index, strLength);
                    if (WordsDictionary.ContainsKey(str))
                    {
                        if (index + strLength >= input.Length)
                        {
                            result += WordsDictionary[str];
                            index += strLength;
                            break;
                        }
                        strLength++;
                    }
                    else
                    {
                        str = input.Substring(index, strLength - 1);
                        result += WordsDictionary[str];
                        index += (strLength - 1);
                        break;
                    }
                }
            }
            return result;
        }



        //public static string GetFirstLetter(string input)
        //{
        //    input = input.Split(new[] { ':', '-' }, StringSplitOptions.RemoveEmptyEntries)[0];
        //    input = Regex.Replace(input, @"[^\u4e00-\u9fa5]", "");
        //    var result = "";
        //    for (int i = 0; i < input.Length; i++)
        //    {
        //        var key = ((int)input[i]).ToString("x").ToUpper();
        //        if (WordsDictionary.ContainsKey(key))
        //        {
        //            result += WordsDictionary[key][0];
        //        }
        //    }
        //    return result;
        //}

        //public static string GetFullPinyin(string input)
        //{
        //    input = input.Split(new[] {':', '-'}, StringSplitOptions.RemoveEmptyEntries)[0];
        //    input = Regex.Replace(input, @"[^\u4e00-\u9fa5]", "");
        //    var result = "";
        //    for (int i = 0; i < input.Length; i++)
        //    {
        //        var key = ((int)input[i]).ToString("x").ToUpper();
        //        if (WordsDictionary.ContainsKey(key))
        //        {
        //            result += (WordsDictionary[key] + " ");
        //        }
        //    }
        //    return result.Trim();
        //}
    }
}