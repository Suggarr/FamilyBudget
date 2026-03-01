//using FamilyBudgetBot.Infrastructure.Entities;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FamilyBudget.Infrastructure.Configurations
//{
//    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
//    {
//        public void Configure(EntityTypeBuilder<Category> builder)
//        {
//            builder.HasKey(c => c.Id);

//            builder.Property(c => c.Name)
//                .IsRequired()
//                .HasMaxLength(Category.MAX_NAME_LENGTH);

//            builder.HasIndex(c => new { c.FamilyId, c.Name })
//                .IsUnique();

//            builder.HasOne(c => c.Family)
//                .WithMany(c => c.Categories)
//                .HasForeignKey(c => c.FamilyId)
//                .OnDelete(DeleteBehavior.Cascade);
//        }
//    }
//}
