using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YaVipCore.Api.Video;

namespace YaVipCore.Controllers
{
    [Route("[controller]")]
    public class VideoController : Controller
    {
        [HttpGet]
        public void Get([FromQuery]string vid, [FromQuery]string quality, [FromQuery]string type, [FromQuery]string format)
        {
            if (string.IsNullOrEmpty(vid) || string.IsNullOrEmpty(type))
            {
                Response.StatusCode = 404;
                return;
            }
            //var ip = Request.Headers["X-Original-For"].ToArray().FirstOrDefault();
            //if (string.IsNullOrEmpty(ip))
            //{
            //    ip = HttpContext.Connection.RemoteIpAddress.ToString();
            //}
            //if (!string.IsNullOrEmpty(ip))
            //{
            //    ip = ip.Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries)[0];
            //}
            string link;
            switch (type)
            {
                case "tx":
                    link = TxVip.GetUrl(vid, quality, format);
                    break;
                case "qy":
                    link = QyVip.GetUrl(vid, quality, format);
                    break;
                case "yk":
                    link = YkVip.GetUrl(vid, quality, format);
                    break;
                case "le":
                    link = LeVip.GetUrl(vid, quality, format);
                    break;
                case "mg":
                    link = MgVip.GetUrl(vid, quality, format);
                    break;
                case "pp":
                    link = PpVip.GetUrl(vid, quality, format);
                    break;
                default:
                    link = "";
                    break;
            }
            if (string.IsNullOrEmpty(link))
            {
                Response.StatusCode = 404;
                return;
            }
            if (link.StartsWith("http"))
            {
                Response.StatusCode = 302;
                Response.Headers.Add("Location", link);
                return;
            }
            Response.StatusCode = 200;
            Response.ContentType = "audio/x-mpegurl";
            Response.WriteAsync(link);
        }
    }
}
