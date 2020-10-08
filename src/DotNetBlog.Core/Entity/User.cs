using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetBlog.Entity
{
    public class User : IdentityUser<long>
    {
        [MaxLength(20)]
        public string Nickname { get; set; }

        public DateTime? LoginDate { get; set; }

        [MaxLength(100)]
        public string Avatar { get; set; }
    }
}
