using DotNetBlog.Model.Category;
using DotNetBlog.Model.Widget;
using DotNetBlog.Service;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetBlog.Web.Widgets
{
    public class CategoryWidget : ViewComponent
    {
        private CategoryService CategoryService { get; set; }

        public CategoryWidget(CategoryService categoryService)
        {
            CategoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync(CategoryWidgetConfigModel config)
        {
            ViewBag.Config = config;

            List<CategoryModel> categoryList = await CategoryService.All();
            return this.View(categoryList);
        }
    }
}
