using CoreEntities.CustomModels.AdminPanel;
using System.Collections.Generic;
using System.Web.Security;
using System.Web;
using System;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Lead411.Controllers
{
    public class AdminController : Controller
    {
        private static readonly List<LoginUser> Users = new List<LoginUser>()
        {
            new LoginUser {Username = "admin" , Password = "1234" }
        };

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// For Authenticating Admin
        /// </summary>
        /// <param name="model">AdminDetails</param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public ActionResult Authenticate(LoginUser model)
        {
            // Lets first check if the Model is valid or not
            if (model.Username != null || model.Username != " " || model.Password != null || model.Password != " ")
            {
                // User found in the List
                var user = Users.Find( x=> x.Username == model.Username);
                    
                    if ( user != null && user.Password.Equals(model.Password))
                    {
                        FormsAuthentication.SetAuthCookie(model.Username, false);
                       return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        ModelState.AddModelError("ModelError", "The user name or password provided is incorrect.");
                    }
            }
            ModelState.Remove("Password");
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// For Logout Admin
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {

            Session.Abandon();
            FormsAuthentication.SignOut();

            // clear authentication cookie
            HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie1.Expires = DateTime.Now.AddYears(-1);

            // Clear session cookie 
            HttpCookie rSessionCookie = new HttpCookie("user@Lead411.com", "");
            rSessionCookie.Expires = DateTime.Now.AddYears(-1);


            SessionStateSection sessionStateSection = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");
            HttpCookie cookie2 = new HttpCookie(sessionStateSection.CookieName, "");
            cookie2.Expires = DateTime.Now.AddYears(-1);


            FormsAuthentication.RedirectToLoginPage();

            return RedirectToAction("Index", "Home");
        }

        [System.Web.Http.HttpGet]
        public ActionResult Login()
        {
            return View();
        }
    }
}