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
  }
}