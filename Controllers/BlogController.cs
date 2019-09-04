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
    private int? _blogId
    {
      get { return HttpContext.Session.GetInt32("blogID"); }
      set { HttpContext.Session.SetInt32("blogID", (int)value); }
    }
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
      ViewBag.isAdmin = _uid != null ? true : false;
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
      ViewBag.isAdmin = _uid != null ? true : false;
      Blog blog = dbContext.Blogs
        .Include(b => b.BlogImgs)
        .FirstOrDefault(b => b.Title == title);
      return View(blog);
    }

    [HttpGet("blogs/create")]
    public IActionResult CreateForm()
    {
      ViewBag.isAdmin = _uid != null ? true : false;
      if (_uid == null){ return RedirectToAction("Login", "Home"); }
      return View("Create");
    }

    [HttpPost("blogs/create")]
    public IActionResult Create(BlogViewModel newBlog)
    {
      if (_uid == null){ return RedirectToAction("Login", "Home"); }
      if (ModelState.IsValid)
      {
        if(!AreImagesValid(newBlog.Imgs)) { return View(); }
        
				Blog blog = new Blog();
        blog.Add(newBlog);

				dbContext.Blogs.Add(blog);
				dbContext.SaveChanges();

        CreateBlogImgRows(blog.BlogId, newBlog.Imgs);
				return RedirectToAction("Index");
      }
      return View();
    }
    public bool AreImagesValid(List<IFormFile> blogImgs)
    {
      if (blogImgs == null) { return true; }
      foreach(IFormFile img in blogImgs)
        {
          if (img.ContentType.Split("/")[0] != "image")
          {
            ModelState.AddModelError("Imgs", "Only image files are allowed");
            return false;
          }
          if(img.Length > 5000000)
          {
            ModelState.AddModelError("Imgs", "Maximum file size is 5MB");
            return false;
          }
        }
      return true;
    }
    public void CreateBlogImgRows(int blogId, List<IFormFile> blogImgs)
    {
      if (blogImgs != null && blogImgs.Count > 0)
      {
        string uniqueFileName = null;
        string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "img");
        foreach (IFormFile img in blogImgs)
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
    }

    [HttpGet("blogs/edit/{id}")]
    public IActionResult EditForm(int id)
    {
      ViewBag.isAdmin = _uid != null ? true : false;
      if (_uid == null){ return RedirectToAction("Login", "Home"); }
      return View("Edit", GetEditInfo(id));
    }
    public BlogViewModel GetEditInfo(int id)
    {
      _blogId = id;
      Blog blog = dbContext.Blogs.Include(b => b.BlogImgs).FirstOrDefault(b => b.BlogId == id);
      BlogViewModel editBlog = new BlogViewModel();
      editBlog.AddFieldValues(blog);
      
      ViewBag.Imgs = blog.BlogImgs;
      return editBlog;
    }
    
    [HttpPost("blogs/edit")]
    public IActionResult Edit(BlogViewModel editBlog)
    {
      if (_uid == null){ return RedirectToAction("Login", "Home"); }
      if(!ModelState.IsValid) { return View(); }
      if(!AreImagesValid(editBlog.Imgs)) { return View(GetEditInfo((int)_blogId)); }
      
      Blog blog = dbContext.Blogs.Include(b => b.BlogImgs).FirstOrDefault(b => b.BlogId == _blogId);
      blog.Update((int)_blogId, editBlog);
      dbContext.SaveChanges();

      CreateBlogImgRows((int)_blogId, editBlog.Imgs);
      HttpContext.Session.Remove("blogId");
      
      return RedirectToAction("Info", new { title = blog.Title });
    }

    [HttpPost("blogs/edit/deleteimg/{id}")]
    public IActionResult DeleteImg(int id)
    {
      if (_uid == null){ return RedirectToAction("Login", "Home"); }
      BlogImg img = dbContext.BlogImgs.FirstOrDefault(b => b.BlogImgId == id);
      // TODO remove images in batch using Checkboxes(?)
      // TODO prevent other edits from getting lost (i.e. title, content)
      dbContext.BlogImgs.Remove(img);
      dbContext.SaveChanges();

      string fileLocation = Path.Combine(hostingEnvironment.WebRootPath, "img", img.ImgLoc);
      System.IO.File.Delete(fileLocation);

      return RedirectToAction("EditForm", new { id = _blogId });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
