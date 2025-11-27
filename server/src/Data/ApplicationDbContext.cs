using Microsoft.EntityFrameworkCore;
using WorkflowEngine.Models;

namespace WorkflowEngine.Data;

/// <summary>
/// Entity Framework DbContext for the workflow engine
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Workflow> Workflows { get; set; }
    public DbSet<WorkflowNode> WorkflowNodes { get; set; }
    public DbSet<WorkflowEdge> WorkflowEdges { get; set; }
    public DbSet<WorkflowVersion> WorkflowVersions { get; set; }
    public DbSet<WorkflowAuditLog> WorkflowAuditLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Workflow configuration
        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.GraphJson).IsRequired();
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.ModifiedDate).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.CreatedDate);
        });
        
        // WorkflowNode configuration
        modelBuilder.Entity<WorkflowNode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NodeId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NodeType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ConfigJson).IsRequired();
            entity.Property(e => e.Label).HasMaxLength(200);
            
            entity.HasOne(e => e.Workflow)
                .WithMany(w => w.Nodes)
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.WorkflowId, e.NodeId });
        });
        
        // WorkflowEdge configuration
        modelBuilder.Entity<WorkflowEdge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EdgeId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SourceNodeId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TargetNodeId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SourcePort).HasMaxLength(50);
            entity.Property(e => e.TargetPort).HasMaxLength(50);
            entity.Property(e => e.Label).HasMaxLength(100);
            
            entity.HasOne(e => e.Workflow)
                .WithMany(w => w.Edges)
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.WorkflowId);
        });
        
        // WorkflowVersion configuration
        modelBuilder.Entity<WorkflowVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VersionNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.GraphJson).IsRequired();
            entity.Property(e => e.PublishedDate).IsRequired();
            entity.Property(e => e.PublishedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ReleaseNotes).HasMaxLength(2000);
            
            entity.HasOne(e => e.Workflow)
                .WithMany(w => w.Versions)
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.WorkflowId, e.VersionNumber }).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });
        
        // WorkflowAuditLog configuration
        modelBuilder.Entity<WorkflowAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            
            entity.HasOne(e => e.Workflow)
                .WithMany(w => w.AuditLogs)
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.SessionId);
        });
    }
}
