using Andy.Agentic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Andy.Agentic.Infrastructure.Data
{
    /// <summary>
    /// Main Entity Framework database context for the Andy Agentic application.
    /// Manages database connections and entity configurations for all domain entities
    /// including agents, prompts, tools, chat messages, and their relationships.
    /// </summary>
    public class AndyDbContext(DbContextOptions<AndyDbContext> options) : DbContext(options)
    {
        public DbSet<AgentEntity> Agents { get; set; }
        public DbSet<LlmConfigEntity> LlmConfigs { get; set; }
        public DbSet<PromptEntity> Prompts { get; set; }
        public DbSet<PromptVariableEntity> PromptVariables { get; set; }
        public DbSet<AgentToolEntity> AgentTools { get; set; }
        public DbSet<AgentMcpServerEntity> AgentMcpServers { get; set; }
        public DbSet<TagEntity> Tags { get; set; }
        public DbSet<AgentTagEntity> AgentTags { get; set; }
        public DbSet<ToolEntity> Tools { get; set; }
        public DbSet<ChatMessageEntity> ChatMessages { get; set; }
        public DbSet<ToolExecutionLogEntity> ToolExecutionLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AgentEntity>()
                .HasOne(a => a.LlmConfig)
                .WithMany() 
                .HasForeignKey(a => a.LlmConfigId);

            modelBuilder.Entity<PromptEntity>()
                .HasOne(p => p.Agent)
                .WithMany(a => a.Prompts)
                .HasForeignKey(p => p.AgentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PromptVariableEntity>()
                .HasOne(pv => pv.Prompt)
                .WithMany(p => p.Variables)
                .HasForeignKey(pv => pv.PromptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AgentToolEntity>()
                .HasOne(at => at.Agent)
                .WithMany(a => a.Tools)
                .HasForeignKey(at => at.AgentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AgentToolEntity>()
                .HasOne(at => at.Tool)
                .WithMany()
                .HasForeignKey(at => at.ToolId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AgentTagEntity>()
                .HasOne(at => at.Agent)
                .WithMany(a => a.AgentTags)
                .HasForeignKey(at => at.AgentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AgentTagEntity>()
                .HasOne(at => at.Tag)
                .WithMany(t => t.AgentTags)
                .HasForeignKey(at => at.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMessageEntity>()
                .HasOne(cm => cm.Agent)
                .WithMany()
                .HasForeignKey(cm => cm.AgentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ToolExecutionLogEntity>()
                .HasOne(tel => tel.Agent)
                .WithMany()
                .HasForeignKey(tel => tel.AgentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AgentEntity>()
                .HasIndex(a => a.Name)
                .IsUnique();

            modelBuilder.Entity<TagEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();

            modelBuilder.Entity<TagEntity>()
                .Property(t => t.Id)
                .ValueGeneratedNever(); 

            modelBuilder.Entity<ToolEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();

            modelBuilder.Entity<McpServerEntity>()
                .HasIndex(m => m.Name)
                .IsUnique();

            modelBuilder.Entity<AgentToolEntity>()
                .HasKey(at => new { at.AgentId, at.ToolId });

        }
    }
}
