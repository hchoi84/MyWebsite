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
      List<Blog> blogs = dbContext.Blogs
        .OrderByDescending(b => b.CreatedAt)
        .ToList();
      CountBlogs(blogs);
      return View(blogs);
    }
    public void CountBlogs(List<Blog> blogs)
    {
      Dictionary<int, int> blogCountByYear = new Dictionary<int, int>();
      Dictionary<DateTime, int> blogCountByMonth = new Dictionary<DateTime, int>();
      int firstBlogYear = blogs.Last().CreatedAt.Year;

      for (int year = DateTime.Now.Year; year >= firstBlogYear; year--)
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
        if(!AreImagesValid(newBlog.Imgs)) { return View(); }
				Blog blog = new Blog();
        blog.Add(newBlog);

				dbContext.Blogs.Add(blog);
				dbContext.SaveChanges();

        if (newBlog.Imgs != null && newBlog.Imgs.Count > 0)
        {
					CreateBlogImgRow(blog.BlogId, newBlog.Imgs);
        }
				return RedirectToAction("Index");
      }
      return View();
    }
    public bool AreImagesValid(List<IFormFile> newBlogImgs)
    {
      foreach(IFormFile img in newBlogImgs)
        {
          if (img.ContentType.Split("/")[0] != "image")
          {
            ModelState.AddModelError("Imgs", "Only image files are allowed");
            return false;
          }
          if(img.Length > 5000000)
          {
            ModelState.AddModelError("Imgs", "Maximum file size is 5Mb");
            return false;
          }
        }
      return true;
    }
    public void CreateBlogImgRow(int blogId, List<IFormFile> newBlogImgs)
    {
      string uniqueFileName = null;
      string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "img");
      foreach (IFormFile img in newBlogImgs)
      {
        uniqueFileName = Guid.NewGuid().ToString() + "_" + img.FileName;
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
        img.CopyTo(new FileStream(filePath, FileMode.Create));

        BlogImg blogImg = new BlogImg();
        blogImg.Add(blogId, uniqueFileName, img.FileName);

        dbContext.BlogImgs.Add(blogImg);
        dbContext.SaveChanges();
      }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
