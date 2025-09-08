using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
//using TaskManager.Projects.Entities;
using Microsoft.EntityFrameworkCore.SqlServer;
using TaskManager.Shared.Entities;

namespace TaskManager.Projects.Data
{
    public class ProjectDbContext : DbContext
    {
        public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options) { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }

    }
}
