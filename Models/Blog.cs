using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyWebsite.Models
{
  public class Blog
  {
    [Key]
    public int BlogId {get;set;}

    [Required(ErrorMessage="Required")]
    [Display(Name="Title")]
    public string Title { get; set; }

    [Required(ErrorMessage="Required")]
    [Display(Name="Content")]
    public string Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public List<BlogImg> BlogImgs { get; set; }

    public void Add(BlogViewModel newBlog)
    {
      this.Title = newBlog.Title;
      this.Content = newBlog.Content;
    }

    public void Update(int blogId, BlogViewModel editBlog)
    {
      this.BlogId = blogId;
      this.Title = editBlog.Title;
      this.Content = editBlog.Content;
      this.UpdatedAt = DateTime.Now;
    }
  }
}