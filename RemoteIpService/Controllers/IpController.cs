using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace RemoteIpService.Controllers
{
    [Route("/")]
    [ApiController]
    public class IpController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IpResponse> Get()
        {
            string header = (Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? Request.Headers["X-Forwarded-For"].FirstOrDefault());
            if (IPAddress.TryParse(header, out IPAddress ip))
            {
                return new IpResponse()
                {
                    publicIp = ip.ToString()
                };
            }
            else
            {
                return new IpResponse()
                {
                    publicIp = Request.HttpContext.Connection.RemoteIpAddress.ToString()
                };
            }
        }
    }
}