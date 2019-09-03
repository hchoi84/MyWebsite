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
  public class ProjectController : Controller
  {
		private WebsiteContext dbContext;
    private IHostingEnvironment hostingEnvironment;
    public ProjectController(WebsiteContext context, IHostingEnvironment hostingEnvironment)
    {
      dbContext = context;
      this.hostingEnvironment = hostingEnvironment;
    }
		
    [HttpGet("projects")]
    public IActionResult Index()
		{
			List<Project> projects = dbContext.Projects
				.OrderByDescending(p => p.CreatedAt)
				.Include(p => p.ProjectImgs)
				.ToList();

			return View(projects);
		}

    [HttpGet("projects/{title}")]
    public IActionResult Info(string title)
		{
			Project project = dbContext.Projects
				.Include(p => p.ProjectImgs)
				.FirstOrDefault(p => p.Title == title);

			return View(project);
		}

    [HttpGet("projects/create")]
    public IActionResult CreateForm() => View("Create");

    [HttpPost("projects/create")]
    public IActionResult Create(ProjectViewModel newProject)
    {
      if (ModelState.IsValid)
      {
        if(!AreImagesValid(newProject.Imgs)) { return View(); }

        Project project = new Project();
        project.Add(newProject);

        dbContext.Projects.Add(project);
        dbContext.SaveChanges();

        if (newProject.Imgs != null && newProject.Imgs.Count > 0)
        {
          CreateProjectImgRows(newProject.Imgs, project.ProjectId);
        }
        return RedirectToAction("Index");
      }
      return View();
    }
    public bool AreImagesValid(List<IFormFile> newProjImgs)
    {
      foreach(IFormFile img in newProjImgs)
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
    public void CreateProjectImgRows(List<IFormFile> newProjImgs, int projectId)
    {
      string uniqueFileName = null;
      string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "img");
      foreach (IFormFile img in newProjImgs)
      {
        uniqueFileName = Guid.NewGuid().ToString() + "_" + img.FileName;
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
        img.CopyTo(new FileStream(filePath, FileMode.Create));

        ProjectImg projectImg = new ProjectImg();
        projectImg.Add(projectId, uniqueFileName, img.FileName);

        dbContext.ProjectImgs.Add(projectImg);
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
