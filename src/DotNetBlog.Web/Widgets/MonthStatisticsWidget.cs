﻿using DotNetBlog.Model.Widget;
using DotNetBlog.Service;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DotNetBlog.Web.Widgets
{
    public class MonthStatisticsWidget : ViewComponent
    {
        private TopicService TopicService { get; set; }

        public MonthStatisticsWidget(TopicService topicService)
        {
            TopicService = topicService;
        }

        public async Task<IViewComponentResult> InvokeAsync(MonthStatisticeWidgetConfigModel config)
        {
            ViewBag.Config = config;

            var list = await TopicService.QueryMonthStatistics();

            return View(list);
        }
    }
}
