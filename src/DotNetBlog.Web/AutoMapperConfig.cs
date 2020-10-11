using AutoMapper;
using DotNetBlog.Entity;
using DotNetBlog.Model.Page;
using DotNetBlog.Model.Setting;
using DotNetBlog.Model.Topic;
using DotNetBlog.Model.Api.Config;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBlog.Web
{
    public static class AutoMapperConfig
    {
        public static IServiceCollection AddAutoMapper(this IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<BasicConfigModel, SettingModel>();
                config.CreateMap<SettingModel, BasicConfigModel>();
                config.CreateMap<EmailConfigModel, SettingModel>();
                config.CreateMap<SettingModel, EmailConfigModel>();
                config.CreateMap<SettingModel, CommentConfigModel>();
                config.CreateMap<CommentConfigModel, SettingModel>();
                config.CreateMap<SettingModel, AdvanceConfigModel>();
                config.CreateMap<AdvanceConfigModel, SettingModel>();

                config.CreateMap<Topic, TopicModel>().ForMember(dest => dest.Date, map => map.MapFrom(source => source.EditDate));
                config.CreateMap<Page, PageModel>().ForMember(dest => dest.Date, map => map.MapFrom(source => source.EditDate));
                config.CreateMap<Page, PageBasicModel>().ForMember(dest => dest.Date, map => map.MapFrom(source => source.EditDate));
                config.CreateMap<AddPageModel, Page>();
                config.CreateMap<EditPageModel, Page>();
            });

            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton<IMapper>(mapper);

            return services;
        }
    }
}
