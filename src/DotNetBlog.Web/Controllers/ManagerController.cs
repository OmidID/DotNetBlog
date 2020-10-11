using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBlog.Web.Controllers
{
    [Route("manager")]
    [Filters.RequireLoginFilter]
    [Filters.ErrorHandleFilter]
    public class ManagerController : Controller
    {
        [Route("{*path}")]
        public IActionResult Index()
        {
            return View();
        }
    }
}