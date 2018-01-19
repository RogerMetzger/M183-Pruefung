using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Web.Mvc;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Data.SqlClient;
using Pruefung_Praktisch_Musterloesung.Models;

namespace Pruefung_Praktisch_Musterloesung.Controllers
{
    public class Lab2Controller : Controller
    {

        /**
        * 
        * ANTWORTEN BITTE HIER
        * 
        * Aufgabe 2.1
        * Möglichkeit 1: Keine Prüfung ob, der User derjenige ist, der er vorgibt zu sein. (Browser und IP des einzuloggenden Users wird nicht gecheckt)
        * Möglichkeit 2: Die SessionID wird nicht gecheckt.
        * 
        * URL 1: http://localhost:50374/Lab2/login?sid=5804128a98f00e9a80245eb2710b62f0b8c9413d 
        * Erklärung 1: Der Angreifer kann eine SessionID abfangen und sich mit dieser versuchen einzuloggen. Der Angreifer versucht sich auf einem anderen Browser 
        *              und einer anderen IP mithilfe der SessionID einzuloggen. Wenn nicht geprüft wird, ob dieser Browser und diese IP von dem ursprünglichen User 
        *              benutzt wurden, kann er also vorgeben er sei bereits eingeloggt.
        * URL 2: http://localhost:50374/Lab2/login?password=<sql_injection>select * from user where 1=1</sql_injection>
        * Erklärung 2: Der Angreifer kann als passwort etwas eingeben, was immer true gibt also 1=1 OR 'TRUE', so kann er sich einloggen ohne Passwort zu wissen
        * 
        * */

        public ActionResult Index() {

            var sessionid = Request.QueryString["sid"];

            if (string.IsNullOrEmpty(sessionid))
            {
                var hash = (new SHA1Managed()).ComputeHash(Encoding.UTF8.GetBytes(DateTime.Now.ToString()));
                sessionid = string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
            }

            ViewBag.sessionid = sessionid;

            return View();
        }

        [HttpPost]
        public ActionResult Login()
        {
            var username = Request["username"];
            var password = Request["password"];
            var sessionid = Request.QueryString["sid"];

            // hints:
            //var used_browser = Request.Browser.Platform;
            //var ip = Request.UserHostAddress;

            Lab2Userlogin model = new Lab2Userlogin();

            if (model.checkCredentials(username, password))
            {
                model.storeSessionInfos(username, password, sessionid);

                HttpCookie c = new HttpCookie("sid");
                c.Expires = DateTime.Now.AddMonths(2);
                c.Value = sessionid;
                Response.Cookies.Add(c);

                return RedirectToAction("Backend", "Lab2");
            }
            else
            {
                ViewBag.message = "Wrong Credentials";
                return View();
            }
        }

        public ActionResult Backend()
        {
            var sessionid = "";

            if (Request.Cookies.AllKeys.Contains("sid"))
            {
                sessionid = Request.Cookies["sid"].Value.ToString();
            }           

            if (!string.IsNullOrEmpty(Request.QueryString["sid"]))
            {
                sessionid = Request.QueryString["sid"];
            }

            // hints:
            //var used_browser = Request.Browser.Platform;
            //var ip = Request.UserHostAddress;

            var browser = Request.Browser.Platform;
            var ip = Request.UserHostAddress;

            Lab2Userlogin model = new Lab2Userlogin();

            if (model.checkSessionInfos(sessionid, browser, ip))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Lab2");
            }              
        }
    }
}