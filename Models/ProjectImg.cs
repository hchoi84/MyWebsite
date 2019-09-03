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

    public void Add(int projectId, string uniqueFileName, string alt)
    {
      ProjectId = projectId;
      ImgLoc = uniqueFileName;
      Alt = alt;
    }
  }
}