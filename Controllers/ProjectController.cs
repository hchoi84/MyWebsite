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
        Project project = new Project()
        {
          Title = newProject.Title,
          LangAndTech = newProject.LangAndTech,
					Learned = newProject.Learned,
					Difficult = newProject.Difficult,
					RepoURL = newProject.RepoURL,
					LiveURL = newProject.LiveURL
        };
        dbContext.Projects.Add(project);
        dbContext.SaveChanges();

        string uniqueFileName = null;
        if (newProject.Imgs != null && newProject.Imgs.Count > 0)
        {
          string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "img");
          foreach (IFormFile img in newProject.Imgs)
          {
            uniqueFileName = Guid.NewGuid().ToString() + "_" + img.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            img.CopyTo(new FileStream(filePath, FileMode.Create));

            ProjectImg projectImg = new ProjectImg()
            {
              ProjectId = project.ProjectId,
              ImgLoc = uniqueFileName,
              Alt = img.FileName
            };
            dbContext.ProjectImgs.Add(projectImg);
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
