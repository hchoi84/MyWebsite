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
    private int? _projectId
    {
      get { return HttpContext.Session.GetInt32("projectId"); }
      set { HttpContext.Session.SetInt32("projectId", (int)value); }
    }
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
      ViewBag.isAdmin = _uid != null ? true : false;
			List<Project> projects = dbContext.Projects
				.OrderByDescending(p => p.CreatedAt)
				.Include(p => p.ProjectImgs)
				.ToList();

			return View(projects);
		}

    [HttpGet("projects/{title}")]
    public IActionResult Info(string title)
		{
      ViewBag.isAdmin = _uid != null ? true : false;
			Project project = dbContext.Projects
				.Include(p => p.ProjectImgs)
				.FirstOrDefault(p => p.Title == title);

			return View(project);
		}

    [HttpGet("projects/create")]
    public IActionResult CreateForm()
    {
      ViewBag.isAdmin = _uid != null ? true : false;
      if (_uid == null){ return RedirectToAction("Login", "Home"); }
      return View("Create");
    }

    [HttpPost("projects/create")]
    public IActionResult Create(ProjectViewModel newProject)
    {
      if (_uid == null){ return RedirectToAction("Login", "Home"); }
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
    public bool AreImagesValid(List<IFormFile> projImgs)
    {
      if (projImgs == null) { return true; }
      foreach(IFormFile img in projImgs)
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
    public void CreateProjectImgRows(List<IFormFile> projImgs, int projectId)
    {
      if (projImgs != null && projImgs.Count > 0)
      {
        string uniqueFileName = null;
        string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "img");
        foreach (IFormFile img in projImgs)
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
    }

    [HttpGet("projects/edit/{id}")]
    public IActionResult EditForm(int id)
    {
      ViewBag.isAdmin = _uid != null ? true : false;
      if (_uid == null){ return RedirectToAction("Login", "Home"); }
      return View("Edit", GetEditInfo(id));
    }
    public ProjectViewModel GetEditInfo(int id)
    {
      _projectId = id;
      Project project = dbContext.Projects.Include(p => p.ProjectImgs).FirstOrDefault(p => p.ProjectId == id);
      ProjectViewModel editProject = new ProjectViewModel();
      editProject.AddFieldValues(project);
      
      // TODO consider adding List<ProjectImgs> to ProjectViewModel
      ViewBag.Imgs = project.ProjectImgs;
      return editProject;
    }

    [HttpPost("projects/edit")]
    public IActionResult Edit(ProjectViewModel editProject)
    {
      if (_uid == null){ return RedirectToAction("Login", "Home"); }
      if(!ModelState.IsValid) { return View(); }
      if(!AreImagesValid(editProject.Imgs)) { return View(GetEditInfo((int)_projectId)); }
      
      Project project = dbContext.Projects.Include(p => p.ProjectImgs).FirstOrDefault(p => p.ProjectId == _projectId);
      project.Update((int)_projectId, editProject);
      dbContext.SaveChanges();

      CreateProjectImgRows(editProject.Imgs, (int)_projectId);
      HttpContext.Session.Remove("projectId");
      
      return RedirectToAction("Info", new { title = project.Title });
    }

    [HttpPost("projects/edit/deleteimg/{id}")]
    public IActionResult DeleteImg(int id)
    {
      if (_uid == null){ return RedirectToAction("Login", "Home"); }
      ProjectImg img = dbContext.ProjectImgs.FirstOrDefault(b => b.ProjectImgId == id);
      // TODO remove images in batch using Checkboxes(?)
      // TODO prevent other edits from getting lost (i.e. title, content)
      dbContext.ProjectImgs.Remove(img);
      dbContext.SaveChanges();

      string fileLocation = Path.Combine(hostingEnvironment.WebRootPath, "img", img.ImgLoc);
      System.IO.File.Delete(fileLocation);

      return RedirectToAction("EditForm", new { id = _projectId });
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
