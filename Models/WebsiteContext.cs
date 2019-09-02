using Microsoft.EntityFrameworkCore;

namespace MyWebsite.Models
{
  public class WebsiteContext : DbContext
  {
    public WebsiteContext(DbContextOptions options) : base(options) {}
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<BlogImg> BlogImgs { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectImg> ProjectImgs { get; set; }
  }
}