﻿using System.Web.Mvc;
using MrCMS.Entities.People;
using MrCMS.Services;
using MrCMS.Web.Areas.Admin.Models;
using MrCMS.Website.Binders;

namespace MrCMS.Web.Areas.Admin.Controllers
{
    public class UserController : AdminController
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IAuthorisationService _authorisationService;
        private readonly ISiteService _siteService;

        public UserController(IUserService userService, IRoleService roleService, IAuthorisationService authorisationService, ISiteService siteService)
        {
            _userService = userService;
            _roleService = roleService;
            _authorisationService = authorisationService;
            _siteService = siteService;
        }

        public ActionResult Index(int page = 1)
        {
            return View(_userService.GetAllUsersPaged(page));
        }

        [HttpGet]
        public PartialViewResult Add()
        {
            var model = new AddUserModel();
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Add([SessionModelBinder(typeof (AddUserModelBinder))] User user)
        {
            _userService.AddUser(user);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            User user = _userService.GetUser(id);

            ViewData["AvailableRoles"] = _roleService.GetAllRoles();
            ViewData["AvailableSites"] = _siteService.GetAllSites();

            if (user == null)
                return RedirectToAction("Index");

            return View(user);
        }

        [HttpPost]
        public ActionResult Edit([SessionModelBinder(typeof (EditUserModelBinder))] User user)
        {
            _userService.SaveUser(user);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult SetPassword(int id)
        {
            return PartialView(id);
        }

        [HttpPost]
        public ActionResult SetPassword(User user, string password)
        {
            _authorisationService.SetPassword(user, password, password);
            return RedirectToAction("Edit", new {user.Id});
        }
    }
}