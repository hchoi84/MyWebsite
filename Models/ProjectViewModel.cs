using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace MyWebsite.Models
{
  public class ProjectViewModel
  {
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

    [Required(ErrorMessage="Required")]
    [Display(Name="Images")]
    public List<IFormFile> Imgs { get; set; }
  }
}