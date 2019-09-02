using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MyWebsite.Models
{
  public class BlogViewModel
  {
    [Required(ErrorMessage="Required")]
    [Display(Name="Title")]
    public string Title { get; set; }

    [Required(ErrorMessage="Required")]
    [Display(Name="Content")]
    public string Content { get; set; }

    [Display(Name="Images")]
    public List<IFormFile> Imgs { get; set; }
  }
}