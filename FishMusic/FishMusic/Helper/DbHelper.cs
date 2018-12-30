using System;
using System.IO;
using LiteDB;

namespace FishMusic.Helper
{
    public class DbHelper
    {
        private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fm.db");

        public static LiteDatabase GetDatabase()
        {
            return new LiteDatabase(DbPath);
        }
    }
}