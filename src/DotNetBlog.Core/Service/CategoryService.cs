using DotNetBlog.Data;
using DotNetBlog.Entity;
using DotNetBlog.Extensions;
using DotNetBlog.Model;
using DotNetBlog.Model.Category;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetBlog.Service
{
    public class CategoryService
    {
        private BlogContext BlogContext { get; set; }
        private IHtmlLocalizer<CategoryService> L { get; set; }

        public CategoryService(BlogContext blogContext, IHtmlLocalizer<CategoryService> localizer)
        {
            BlogContext = blogContext;
            L = localizer;
        }

        public async Task<List<CategoryModel>> All()
        {
            List<CategoryModel> list = BlogContext.QueryAllCategoryFromCache();
            return await Task.FromResult(list);
        }

        public async Task<OperationResult<int>> Add(string name, string description)
        {
            if (await BlogContext.Categories.AnyAsync(t => t.Name == name))
            {
                return OperationResult<int>.Failure(L["Duplicate category name"].Value);
            }

            var entity = new Category
            {
                Description = description,
                Name = name
            };
            BlogContext.Categories.Add(entity);
            await BlogContext.SaveChangesAsync();

            BlogContext.RemoveCategoryCache();

            return new OperationResult<int>(entity.Id);
        }

        public async Task<OperationResult> Edit(int id, string name, string description)
        {
            if (await BlogContext.Categories.AnyAsync(t => t.Name == name && t.Id != id))
            {
                return OperationResult.Failure(L["Duplicate category name"].Value);
            }

            Category entity = await BlogContext.Categories.SingleOrDefaultAsync(t => t.Id == id);
            if (entity == null)
            {
                return OperationResult.Failure(L["Category does not exists"].Value);
            }

            entity.Name = name;
            entity.Description = description;
            await BlogContext.SaveChangesAsync();

            BlogContext.RemoveCategoryCache();

            return new OperationResult();
        }

        public async Task Remove(int[] idList)
        {
            List<CategoryTopic> categoryTopics = await BlogContext.CategoryTopics.Where(t => idList.Contains(t.CategoryId)).ToListAsync();
            List<Category> categories = await BlogContext.Categories.Where(t => idList.Contains(t.Id)).ToListAsync();

            BlogContext.RemoveRange(categoryTopics);
            BlogContext.RemoveRange(categories);

            await BlogContext.SaveChangesAsync();

            BlogContext.RemoveCategoryCache();
        }
    }
}
