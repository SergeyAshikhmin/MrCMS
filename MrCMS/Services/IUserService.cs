using System;
using System.Collections.Generic;
using System.Web;
using MrCMS.Entities.People;
using MrCMS.Models;
using MrCMS.Paging;

namespace MrCMS.Services
{
    public interface IUserService
    {
        void AddUser(User user);
        void SaveUser(User user);
        User GetUser(int id);
        IList<User> GetAllUsers();
        IPagedList<User> GetAllUsersPaged(int page);
        User GetUserByEmail(string email);
        User GetUserByResetGuid(Guid resetGuid);
        User GetCurrentUser(HttpContextBase context);
    }
}