using DotNetBlog;
using DotNetBlog.Web.Areas.Api.Filters;
using DotNetBlog.Model.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace DotNetBlog.Web.Areas.Api.Controllers
{
    [ErrorHandlerFilter]
    [RequireLoginApiFilter]
    [ValidateRequestApiFilter]
    [Authorize(Policy = Policies.AdminAccess)]
    public class ControllerBase : Controller
    {
        private static readonly JsonSerializerSettings _DefaultJsonSerializerSettings = new JsonSerializerSettings
        {
            DateFormatString = "s",
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        IStringLocalizer<Shared> localizer;
        private IStringLocalizer<Shared> L
        {
            get
            {
                if (localizer == null)
                {
                    localizer = this.HttpContext.RequestServices.GetService(typeof(IStringLocalizer<Shared>)) as IStringLocalizer<Shared>;
                }
                return localizer;
            }
        }

        [NonAction]
        public IActionResult Success()
        {
            var response = new ApiResponse
            {
                Success = true
            };

            return Json(response);
        }

        [NonAction]
        public IActionResult Error(string errorMessage)
        {
            var response = new ApiResponse
            {
                ErrorMessage = errorMessage
            };

            return Json(response);
        }

        [NonAction]
        public IActionResult Success<T>(T data)
        {
            var response = new ApiResponse<T>
            {
                Success = true,
                Data = data
            };

            return Json(response);
        }

        [NonAction]
        public IActionResult PagedData<T>(List<T> data, int total)
        {
            var response = new PagedApiResponse<T>
            {
                Success = true,
                Data = data,
                Total = total
            };

            return Json(response);
        }

        [NonAction]
        public new IActionResult Json(object data)
        {
            return base.Json(data, _DefaultJsonSerializerSettings);
        }

        [NonAction]
        public IActionResult InvalidRequest()
        {
            string errorMessage;

            if (ModelState.IsValid)
            {
                errorMessage = L["Bad request"].Value;
            }
            else
            {
                errorMessage = ModelState.Where(t => t.Value.Errors.Any()).Select(t => t.Value).FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage;
            }

            errorMessage = string.IsNullOrWhiteSpace(errorMessage) ? L["Bad request"].Value : errorMessage;

            return this.Error(errorMessage);
        }
    }
}
