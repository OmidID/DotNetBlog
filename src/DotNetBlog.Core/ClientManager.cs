using DotNetBlog.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace DotNetBlog
{
    public class ClientManager
    {
        public HttpContext HttpContext { get; private set; }
        public UserManager<User> UserManager { get; private set; }
        public User CurrentUser { get; internal set; }

        private string _clientIP;
        public string ClientIP =>
            _clientIP ??=
                this.HttpContext
                    .Features
                    .Get<IHttpConnectionFeature>()
                    .RemoteIpAddress
                    .ToString();

        public bool IsLogin =>
            this.CurrentUser != null;

        public ClientManager(UserManager<User> userManager)
        {
            this.UserManager = userManager;
        }

        public Task Init(HttpContext context)
        {
            this.HttpContext = context;
            return this.InitUser();
        }

        internal async Task InitUser(string username = null) =>
            this.CurrentUser = await
                (username == null
                 ? UserManager.GetUserAsync(HttpContext.User)
                 : UserManager.FindByNameAsync(username));
    }
}
