using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcBlog.Models
{
    public class ProfilePasswordViewModel
    {
        public UserProfileViewModel UserProfileViewModel { get; set; }
        public ResetPasswordModel ResetPasswordModel { get; set; }

    }
}