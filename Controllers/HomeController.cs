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
			BlogsProjectsViewModel BPVM = new BlogsProjectsViewModel();

      List<Blog> blogs = dbContext.Blogs
				.OrderByDescending(b => b.CreatedAt)
				.Take(3)
				.ToList();
			BPVM.Blogs = blogs;

      List<Project> projects = dbContext.Projects
        .OrderByDescending(p => p.CreatedAt)
        .Take(3)
        .Include(p => p.ProjectImgs)
        .ToList();
      BPVM.Projects = projects;
      return View(BPVM);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }

  public static class SessionExtensions
  {
    public static void SetObjectAsJson(this ISession session, string key, object value)
    {
      session.SetString(key, JsonConvert.SerializeObject(value));
    }
    public static T GetObjectFromJson<T>(this ISession session, string key)
    {
      string value = session.GetString(key);
      return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
    }
  }
}
