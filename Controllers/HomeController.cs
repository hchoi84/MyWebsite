using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyWebsite.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace MyWebsite.Controllers
{
  public class HomeController : Controller
  {
    public int? _uid
    {
      get { return HttpContext.Session.GetInt32("uid"); }
      set { HttpContext.Session.SetInt32("uid", (int)value); }
    }
    public string _tempMsg
    {
      get { return HttpContext.Session.GetString("TempMsg"); }
      set { HttpContext.Session.SetString("TempMsg", value); }
    }
    private WebsiteContext dbContext;
    private IHostingEnvironment hostingEnvironment;
    public HomeController(WebsiteContext context, IHostingEnvironment hostingEnvironment)
    {
      dbContext = context;
      this.hostingEnvironment = hostingEnvironment;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
      ViewBag.isAdmin = _uid != null ? true : false;
      BlogsProjectsViewModel BPVM = new BlogsProjectsViewModel();

      List<Blog> blogs = dbContext.Blogs
        .OrderByDescending(b => b.CreatedAt)
        .Take(3)
        .ToList();
      foreach (Blog blog in blogs)
      {
        DateTime localTime = TimeZoneInfo.ConvertTime(blog.CreatedAt, TimeZoneInfo.Local);
        blog.CreatedAt = localTime;
      }
      BPVM.Blogs = blogs;

      List<Project> projects = dbContext.Projects
        .OrderByDescending(p => p.CreatedAt)
        .Take(3)
        .Include(p => p.ProjectImgs)
        .ToList();
      BPVM.Projects = projects;
      return View(BPVM);
    }

    [HttpGet("login")]
    public IActionResult Login() => View();
    [HttpPost("login")]
    public IActionResult LoginUser(Login loginUser)
    {
      if (ModelState.IsValid)
      {
        User emailCheck = dbContext.Users.FirstOrDefault(user => user.Email == loginUser.Email);
        if (emailCheck != null)
        {
          PasswordHasher<Login> Hasher = new PasswordHasher<Login>();
          var result = Hasher.VerifyHashedPassword(loginUser, emailCheck.Password, loginUser.Password);
          if (result != 0)
          {
            _uid = emailCheck.UserId;
            return RedirectToAction("Index");
          }
        }
        ModelState.AddModelError("Login.Email", "Email/Password is incorrect");
        return View("Login");
      }
      return View("Login");
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
      User userExists = dbContext.Users.FirstOrDefault();
      if (userExists != null) { return RedirectToAction("Login"); }
      return View();
    }
    [HttpPost("register")]
    public IActionResult CreateUser(User newUser)
    {
      if (ModelState.IsValid)
      {
        User emailCheck = dbContext.Users.FirstOrDefault(user => user.Email == newUser.Password);
        if (emailCheck == null)
        {
          PasswordHasher<User> Hasher = new PasswordHasher<User>();
          newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
          var nu = dbContext.Users.Add(newUser);
          dbContext.SaveChanges();
          _uid = nu.Entity.UserId;
          HttpContext.Session.SetString("UserName", newUser.FirstName);
          return RedirectToAction("Index");
        }
        ModelState.AddModelError("Email", "Email already exists");
        return View("Register");
      }
      return View("Register");
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
      HttpContext.Session.Clear();
      _tempMsg = "Goodbye!";
      return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
