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
using System.IO;

namespace MyWebsite.Controllers
{
  public class BlogController : Controller
  {
    private WebsiteContext dbContext;
    private IHostingEnvironment hostingEnvironment;
    public BlogController(WebsiteContext context, IHostingEnvironment hostingEnvironment)
    {
      dbContext = context;
      this.hostingEnvironment = hostingEnvironment;
    }

    [HttpGet("blogs")]
    public IActionResult Index()
    {
      ViewBag.Now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
      Dictionary<int, int> blogCountByYear = new Dictionary<int, int>();
      Dictionary<DateTime, int> blogCountByMonth = new Dictionary<DateTime, int>();
      
      List<Blog> blogs = dbContext.Blogs
        .OrderByDescending(b => b.CreatedAt)
        .ToList();
      DateTime firstBlogDateTime = blogs.Last().CreatedAt;
      for (int year = DateTime.Now.Year; year >= firstBlogDateTime.Year; year--)
      {
        int yearCount = blogs.Where(b => b.CreatedAt.Year == year).Count();
        blogCountByYear.Add(year, yearCount);
        for (int month = DateTime.Now.Month; month >= 1; month--)
        {
          int monthCount = blogs.Where(b => b.CreatedAt.Year == year && b.CreatedAt.Month == month).Count();
          blogCountByMonth.Add(new DateTime(year, month, 1), monthCount);
        }
      }
      ViewBag.blogCountByYear = blogCountByYear;
      ViewBag.blogCountByMonth = blogCountByMonth;
      return View(blogs);
    }

    [HttpGet("blogs/{title}")]
    public IActionResult Info(string title)
    {
      Blog blog = dbContext.Blogs
        .Include(b => b.BlogImgs)
        .FirstOrDefault(b => b.Title == title);

      return View(blog);
    }

    [HttpGet("blogs/create")]
    public IActionResult CreateForm() => View("Create");

    [HttpPost("blogs/create")]
    public IActionResult Create(BlogViewModel newBlog)
    {
      if (ModelState.IsValid)
      {
				Blog blog = new Blog()
				{
					Title = newBlog.Title,
					Content = newBlog.Content
				};
				dbContext.Blogs.Add(blog);
				dbContext.SaveChanges();

        string uniqueFileName = null;
        if (newBlog.Imgs != null && newBlog.Imgs.Count > 0)
        {
					string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "img");
					foreach (IFormFile img in newBlog.Imgs)
					{
						uniqueFileName = Guid.NewGuid().ToString() + "_" + img.FileName;
						string filePath = Path.Combine(uploadsFolder, uniqueFileName);
						img.CopyTo(new FileStream(filePath, FileMode.Create));

						BlogImg blogImg = new BlogImg()
						{
							BlogId = blog.BlogId,
							ImgLoc = uniqueFileName,
							Alt = img.FileName
						};
						dbContext.BlogImgs.Add(blogImg);
				    dbContext.SaveChanges();
					}
        }
				return RedirectToAction("Index");
      }
      return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
