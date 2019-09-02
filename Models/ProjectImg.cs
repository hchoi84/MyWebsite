using System.ComponentModel.DataAnnotations;

namespace MyWebsite.Models
{
  public class ProjectImg
  {
    [Key]
    public int ProjectImgId { get; set; }
    public int ProjectId { get; set; }
    public string ImgLoc { get; set; }
    public string Alt { get; set; }
  }
}