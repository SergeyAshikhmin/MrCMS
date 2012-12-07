using System;
using System.Collections.Generic;
using System.Web;
using MrCMS.Entities.Multisite;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Paging;
using NHibernate;
using NHibernate.Criterion;

namespace MrCMS.Services
{
    public class UserService : IUserService
    {
        private readonly ISession _session;
        private readonly ISiteService _siteService;

        public UserService(ISession session, ISiteService siteService)
        {
            _session = session;
            _siteService = siteService;
        }

        public void AddUser(User user)
        {
            _session.Transact(session =>
                                  {
                                      var site = _siteService.GetCurrentSite();

                                      if (user.Sites != null)
                                          user.Sites.Add(site);
                                      else
                                          user.Sites = new List<Site> { site };

                                      if (site.Users != null)
                                          site.Users.Add(user);
                                      else
                                          site.Users = new List<User> { user };

                                      session.Save(user);
                                      session.Update(site);
                                  });
        }

        public void SaveUser(User user)
        {
            _session.Transact(session => session.Update(user));
        }

        public User GetUser(int id)
        {
            return _session.Get<User>(id);
        }

        public IList<User> GetAllUsers()
        {
            return _session.QueryOver<User>().Cacheable().List();
        }

        public IPagedList<User> GetAllUsersPaged(int page)
        {
            return _session.QueryOver<User>().Paged(page, 10);
        }

        public User GetUserByEmail(string email)
        {
            return
                _session.QueryOver<User>().Where(user => user.Email.IsLike(email, MatchMode.Exact)).Cacheable().
                    SingleOrDefault();
        }

        public User GetUserByResetGuid(Guid resetGuid)
        {
            return
                _session.QueryOver<User>()
                    .Where(user => user.ResetPasswordGuid == resetGuid && user.ResetPasswordExpiry >= DateTime.UtcNow)
                    .Cacheable().SingleOrDefault();
        }

        public User GetCurrentUser(HttpContextBase context)
        {
            return context.User != null ? GetUserByEmail(context.User.Identity.Name) : null;
        }
    }
}