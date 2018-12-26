namespace AnyListen.Models
{
    public class SongResult
    {
        /// <summary>
        /// 歌曲ID
        /// </summary>
        public string SongId { get; set; }
        /// <summary>
        /// 曲名
        /// </summary>
        public string SongName { get; set; }
        /// <summary>
        /// 歌曲别名
        /// </summary>
        public string SongSubName { get; set; }
        /// <summary>
        /// 艺术家ID
        /// </summary>
        public string ArtistId { get; set; }
        /// <summary>
        /// 歌手名字
        /// </summary>
        public string ArtistName { get; set; }
        /// <summary>
        /// 艺术家别名
        /// </summary>
        public string ArtistSubName { get; set; }
        /// <summary>
        /// 专辑ID
        /// </summary>
        public string AlbumId { get; set; }
        /// <summary>
        /// 专辑名
        /// </summary>
        public string AlbumName { get; set; }
        /// <summary>
        /// 专辑别名
        /// </summary>
        public string AlbumSubName { get; set; }
        /// <summary>
        /// 专辑艺术家
        /// </summary>
        public string AlbumArtist { get; set; }
        /// <summary>
        /// 歌曲链接/来源
        /// </summary>
        public string SongLink { get; set; }
        /// <summary>
        /// 时长
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// 比特率
        /// </summary>
        public string BitRate { get; set; }
        /// <summary>
        /// Flac无损地址
        /// </summary>
        public string FlacUrl { get; set; }
        /// <summary>
        /// Ape无损地址
        /// </summary>
        public string ApeUrl { get; set; }
        /// <summary>
        /// Wav地址
        /// </summary>
        public string WavUrl { get; set; }
        /// <summary>
        /// 320K
        /// </summary>
        public string SqUrl { get; set; }
        /// <summary>
        /// 192K
        /// </summary>
        public string HqUrl { get; set; }
        /// <summary>
        /// 128K
        /// </summary>
        public string LqUrl { get; set; }
        /// <summary>
        /// 复制链接
        /// </summary>
        public string CopyUrl { get; set; }
        /// <summary>
        /// 歌曲小封面120*120
        /// </summary>
        public string SmallPic { get; set; }
        /// <summary>
        /// 歌曲封面
        /// </summary>
        public string PicUrl { get; set; }
        /// <summary>
        /// LRC歌词
        /// </summary>
        public string LrcUrl { get; set; }
        /// <summary>
        /// TRC歌词
        /// </summary>
        public string TrcUrl { get; set; }
        /// <summary>
        /// KRC歌词
        /// </summary>
        public string KrcUrl { get; set; }
        /// <summary>
        /// MV Id
        /// </summary>
        public string MvId { get; set; }
        /// <summary>
        /// 高清MV地址
        /// </summary>
        /// 
        public string MvHdUrl { get; set; }
        /// <summary>
        /// 普清MV地址
        /// </summary>
        public string MvLdUrl { get; set; }
        /// <summary>
        /// 语种
        /// </summary>
        public string Language { get; set; }
        /// <summary>
        /// 发行公司
        /// </summary>
        public string Company { get; set; }
        /// <summary>
        /// 歌曲发行日期
        /// </summary>
        public string Year { get; set; }
        /// <summary>
        /// 碟片
        /// </summary>
        public int Disc { get; set; }
        /// <summary>
        /// 曲目编号
        /// </summary>
        public int TrackNum { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }
    }
}