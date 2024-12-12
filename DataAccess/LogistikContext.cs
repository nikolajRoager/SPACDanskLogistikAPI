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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(C => C.Id);
                entity.Property(C => C.Name).HasMaxLength(64);
                entity.Property(C => C.R).HasColumnType("tinyint");
                entity.Property(C => C.G).HasColumnType("tinyint");
                entity.Property(C => C.B).HasColumnType("tinyint");
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
        }


        public DbSet<Country> Countries { get; set; }
        public DbSet<Municipality> municipalities { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Node> Nodes { get; set; }

        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Consumer> Consumers { get; set; }

    }
}
