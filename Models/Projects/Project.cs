using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyWebsite.Models
{
  public class Project
  {
    [Key]
    public int ProjectId { get; set; }

    [Required(ErrorMessage="Required")]
    [Display(Name="Title")]
    public string Title { get; set; }
    
    [Required(ErrorMessage="Required")]
    [Display(Name="Languages and Technologies")]
    public string LangAndTech { get; set; }
    
    [Required(ErrorMessage="Required")]
    [Display(Name="What I Learned")]
    public string Learned { get; set; }
    
    [Required(ErrorMessage="Required")]
    [Display(Name="The Difficult Part")]
    public string Difficult { get; set; }
    
    [Display(Name="Repository URL (Optional)")]
    public string RepoURL { get; set; }
    
    [Display(Name="Live URL (Optional)")]
    public string LiveURL { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public List<ProjectImg> ProjectImgs { get; set; }

    public void Add(ProjectViewModel newProject)
    {
      Title = newProject.Title;
      LangAndTech = newProject.LangAndTech;
      Learned = newProject.Learned;
      Difficult = newProject.Difficult;
      RepoURL = newProject.RepoURL;
      LiveURL = newProject.LiveURL;
    }

    public void Update(int projectId, ProjectViewModel editProject)
    {
      this.ProjectId = projectId;
      this.Title = editProject.Title;
      this.LangAndTech = editProject.LangAndTech;
      this.Learned = editProject.Learned;
      this.Difficult = editProject.Difficult;
      this.RepoURL = editProject.RepoURL;
      this.LiveURL = editProject.LiveURL;
      this.UpdatedAt = DateTime.Now;
    }
  }
}