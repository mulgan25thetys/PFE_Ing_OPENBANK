using Identity.API.Applications.Models.Entities;
using System;

namespace Identity.API.Utils
{
    public static class UserProviderDetails
    {
        public static UserModel GetUriProviderDetails(HttpRequest request, UserModel user)
        {
            Uri uri = new Uri(request.Host.ToString());
            user.Provider = uri.ToString();
            user.Provider_id = uri.Host == null ? "localhost" : uri.Host.ToString();
            return user;
        }
    }
}
