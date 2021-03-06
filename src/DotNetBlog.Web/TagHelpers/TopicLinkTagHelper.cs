﻿using DotNetBlog.Core.Model.Topic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DotNetBlog.Web.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "blog-topic")]
    [HtmlTargetElement("topic-link")]
    public class TopicLinkTagHelper : TagHelper
    {
        [HtmlAttributeName("blog-topic")]
        public ITopicModel Topic { get; set; }

        [HtmlAttributeName("blog-topic-fragment")]
        public string Fragment { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rendering.ViewContext"/> for the current request.
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        private IHtmlGenerator HtmlGenerator { get; set; }

        private IUrlHelperFactory UrlHelperFactory { get; set; }

        public TopicLinkTagHelper(IHtmlGenerator htmlGenerator, IUrlHelperFactory urlHelperFactory)
        {
            this.UrlHelperFactory = urlHelperFactory;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Topic == null)
            {
                base.Process(context, output);
            }
            else
            {
                var urlHelper = this.UrlHelperFactory.GetUrlHelper(this.ViewContext);

                string url;
                if (!string.IsNullOrWhiteSpace(this.Topic.Alias))
                {
                    url = urlHelper.Action("TopicByAlias", "Home", new { alias = this.Topic.Alias });
                }
                else
                {
                    url = urlHelper.Action("Topic", "Home", new { id = this.Topic.Id });
                }

                if (!string.IsNullOrWhiteSpace(this.Fragment))
                {
                    url = string.Format("{0}#{1}", url, this.Fragment);
                }

                if (output.TagName == "topic-link")
                {
                    output.TagName = "";
                    output.Content.SetHtmlContent(url);
                }
                else
                {
                    output.Attributes.Add("href", url);
                    output.Attributes.Add("title", this.Topic.Title);
                }
            }
        }
    }
}
