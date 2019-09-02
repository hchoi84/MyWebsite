using System.ComponentModel.DataAnnotations;

namespace MyWebsite.Models
{
  public class BlogImg
  {
    [Key]
    public int BlogImgId { get; set; }

    public int BlogId { get; set; }
    public string ImgLoc { get; set; }
    public string Alt { get; set; }
  }
}