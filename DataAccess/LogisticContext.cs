using DanskLogistikAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DanskLogistikAPI.DataAccess
{
    public class LogisticContext : DbContext
    {
        public LogisticContext()
        {

        }
        public LogisticContext(DbContextOptions<LogisticContext> options) : base(options)
        {

        }
        
        /// <summary>
        /// Overwrite a few properties from defaults, such as lengths of strings
        /// In general I use 64 chars for names, and longtext for svg snippets 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(C => C.Id);
                entity.Property(C => C.Name).HasMaxLength(64);
            });

            modelBuilder.Entity<Municipality>(entity =>
            {
                entity.HasKey(M => M.Id);
                entity.Property(M => M.Name).HasMaxLength(64);
            });

            modelBuilder.Entity<Node>(entity =>
            {
                entity.HasKey(N => N.Id);
                entity.Property(N => N.Name).HasMaxLength(64);
            });

            modelBuilder.Entity<Connection>(entity =>
            {
                entity.HasKey(C => C.Id);
                //Store mode as integer, this is more efficient than strings
                entity.Property(C => C.mode).HasConversion<int>().HasDefaultValue(Connection.Mode.Rail);
                entity.Property(C => C.Name).HasMaxLength(64).IsRequired(false);
            });

            modelBuilder.Entity<NodeMapping>(entity =>
            {
                entity.HasKey(entity => entity.Id);
                entity.HasOne(entity => entity.Start).WithMany(n => n.Neighbors).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(entity => entity.Connection);
            });

            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(W => W.Id);
                entity.Property(W => W.Name).HasMaxLength(64).IsRequired(false);
            });


            modelBuilder.Entity<Consumer>(entity =>
            {
                entity.HasKey(C => C.Id);
                entity.Property(C => C.Name).HasMaxLength(64).IsRequired(false);
            });

            modelBuilder.Entity<SVGSnippet>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).HasMaxLength(64);
            });
        }


        public DbSet<Country> Countries { get; set; }
        public DbSet<Municipality> municipalities { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Node> Nodes { get; set; }
        public DbSet<SVGSnippet> Snippets { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Consumer> Consumers { get; set; }
        public DbSet<NodeMapping> NodeMapping { get; set; }

    }
}
