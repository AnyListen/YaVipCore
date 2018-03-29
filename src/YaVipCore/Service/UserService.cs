using System;
using System.Collections.Generic;

namespace YaVipCore.Service
{
    public class UserService
    {
        public static Dictionary<string,DateTime> UserInfoList = new Dictionary<string, DateTime>(); 

        public static void InitUser()
        {
            UserInfoList.Add("ee90de6060a7d750cd0fdeb7ba00d78d", DateTime.MaxValue);
        }

        public static bool CheckSign(string sign)
        {
            if (string.IsNullOrEmpty(sign))
            {
                return false;
            }
            sign = sign.ToLower().Trim();
            if (UserInfoList == null || UserInfoList.Count <= 0)
            {
                return true;
            }
            if (!UserInfoList.ContainsKey(sign))
            {
                return false;
            }
            return DateTime.Now <= UserInfoList[sign];
        }
    }
}