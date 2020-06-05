using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MyCourse.Models.Entities;
using MyCourse.Models.ValueTypes;

namespace MyCourse.Models.Services.Infrastucture
{
    public partial class MyCourseDbContext : DbContext
    {
        public MyCourseDbContext(DbContextOptions<MyCourseDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Lesson> Lessons { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");
            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Courses");/*--Superfluo se Courses è indicata nel DBSet*/
                entity.HasKey(courses => courses.Id);/*--Superfluo se la proprietà si chiama Id o courseId*/
                /*--Se abbiamo una chiave composta va indicata nella seguente maniera*/
                //entity.HasKey(courses => new {courses.Id, courses.Author});
                
                //MAPPING PER GLI OWNED TYPES
                entity.OwnsOne(course => course.CurrentPrice, builder => 
                {
                   builder.Property(money => money.Currency)
                   .HasConversion<string>()
                   .HasColumnName("CurrentPrice_Currency");
                   builder.Property(money => money.Amount).HasColumnName("CurrentPrice_Amount");
                });
                 entity.OwnsOne(course => course.FullPrice, builder => 
                {
                   builder.Property(money => money.Currency)
                   .HasConversion<string>()
                   .HasColumnName("FullPrice_Currency");
                   builder.Property(money => money.Amount).HasColumnName("FullPrice_Amount");
                });
                /*--Va a cercare nel database CurrentPrice_Amount e CurrentPrice_Currency quindi basta una sola riga di mapping. */
                //MAPPING PER LE RELAZIONI
                entity.HasMany(course => course.Lessons)
                      .WithOne(lesson => lesson.Course)
                      .HasForeignKey(lesson => lesson.CourseId);
           
            });   
            #region "Mapping generato da EF con l'approccio Database First"
            /*   
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Author)
                    .IsRequired()
                    .HasColumnType("TEXT (100)");

                entity.Property(e => e.CurrentPriceAmount)
                    .IsRequired()
                    .HasColumnName("CurrentPrice_Amount")
                    .HasColumnType("NUMERIC")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.CurrentPriceCurrency)
                    .IsRequired()
                    .HasColumnName("CurrentPrice_Currency")
                    .HasColumnType("TEXT (3)")
                    .HasDefaultValueSql("'EUR'");

                entity.Property(e => e.Description).HasColumnType("TEXT (10000)");

                entity.Property(e => e.Email).HasColumnType("TEXT (100)");

                entity.Property(e => e.FullPriceAmount)
                    .IsRequired()
                    .HasColumnName("FullPrice_Amount")
                    .HasColumnType("NUMERIC")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.FullPriceCurrency)
                    .IsRequired()
                    .HasColumnName("FullPrice_Currency")
                    .HasColumnType("TEXT (3)")
                    .HasDefaultValueSql("'EUR'");

                entity.Property(e => e.ImagePath).HasColumnType("TEXT (100)");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnType("TEXT (100)");
            });
            */ 
            #endregion
            modelBuilder.Entity<Lesson>(entity =>
            {
            });
            #region  "Mapping generato da EF con l'approccio Database First"
            /*
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasColumnType("TEXT (10000)");

                entity.Property(e => e.Duration)
                    .IsRequired()
                    .HasColumnType("TEXT (8)")
                    .HasDefaultValueSql("'00:00:00'");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnType("TEXT (100)");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Lessons)
                    .HasForeignKey(d => d.CourseId);
            });
            */
             #endregion
        }
    }
}
