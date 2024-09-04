using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Postgre_Procedures.Models;

public partial class PROCEDURContext : DbContext
{
    public PROCEDURContext()
    {
    }

    public PROCEDURContext(DbContextOptions<PROCEDURContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryRelation> CategoryRelations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Trust Server Certificate=True;Password=123;Username=postgres;Database=PROCEDUR;Host=localhost");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("Categories_pkey");

            entity.Property(e => e.CategoryId)
                .ValueGeneratedNever()
                .HasColumnName("CategoryID");
        });

        modelBuilder.Entity<CategoryRelation>(entity =>
        {
            entity.HasKey(e => new { e.ParentCategoryId, e.ChildCategoryId }).HasName("CategoryRelations_pkey");

            entity.Property(e => e.ParentCategoryId).HasColumnName("ParentCategoryID");
            entity.Property(e => e.ChildCategoryId).HasColumnName("ChildCategoryID");
        });


        modelBuilder.Entity<Prosedur>().HasNoKey();
        modelBuilder.Entity<Fonksiyon>().HasNoKey();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
