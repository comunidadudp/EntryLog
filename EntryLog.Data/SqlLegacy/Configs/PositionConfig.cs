using EntryLog.Entities.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntryLog.Data.SqlLegacy.Configs
{
    public class PositionConfig : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            builder.ToTable("cargo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasColumnName("nombre");
            builder.Property(x => x.Descripcion).HasColumnName("descripcion");
        }
    }
}
