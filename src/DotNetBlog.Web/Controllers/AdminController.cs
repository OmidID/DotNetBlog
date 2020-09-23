using DotNetBlog.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBlog.Web.Controllers
{
    [Route("admin")]
    [Filters.RequireLoginFilter]
    [Filters.ErrorHandleFilter]
    [Authorize(Policy = Policies.AdminAccess)]
    public class AdminController : Controller
    {
        [Route("{*path}")]
        public IActionResult Index()
        {
            return this.View();
        }
    }
}
