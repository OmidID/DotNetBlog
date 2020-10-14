using DotNetBlog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace DotNetBlog.Web.Controllers
{
    [Route("manager")]
    [Filters.RequireLoginFilter]
    [Filters.ErrorHandleFilter]
    [Authorize(Policy = Policies.AdminAccess)]
    public class ManagerController : Controller
    {
        [Route("{*path}")]
        public IActionResult Index()
        {
            return View();
        }
    }
}