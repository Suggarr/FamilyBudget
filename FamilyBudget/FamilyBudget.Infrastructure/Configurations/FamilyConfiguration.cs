using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Infrastructure.Configurations
{
    public class FamilyConfiguration : IEntityTypeConfiguration<FamilyEntity>
    {
        public void Configure(EntityTypeBuilder<FamilyEntity> builder)
        {
            builder.HasKey(f => f.Id);

            builder.Property(f => f.Name)
                .IsRequired()
                .HasMaxLength(150);
        }
    }
}
