using DotNetBlog.Core.Service;
using DotNetBlog.Web.ViewModels.Exception;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DotNetBlog.Web.Controllers
{
    [Route("exception")]
    public class ExceptionController : Controller
    {
        private SettingService SettingService { get; set; }

        public ExceptionController(SettingService settingService)
        {
            SettingService = settingService;
        }

        [HttpGet("{code:int}")]
        public async Task<IActionResult> Error(int code)
        {
            if (code == StatusCodes.Status500InternalServerError)
            {
                return await this.RenderErrorPage();
            }
            else if (code == StatusCodes.Status404NotFound)
            {
                return this.RenderNotFoundPage();
            }
            else
            {
                return this.Content("");
            }
        }

        [NonAction]
        private IActionResult RenderNotFoundPage()
        {
            return this.View("NotFound");
        }

        [NonAction]
        private async Task<IActionResult> RenderErrorPage()
        {
            ErrorViewModel vm = new ErrorViewModel();

            var config = await SettingService.GetAsync();
            vm.Title = config.ErrorPageTitle;
            vm.Content = config.ErrorPageContent;

            return this.View("Error", vm);
        }
    }
}
