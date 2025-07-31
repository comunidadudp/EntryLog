using EntryLog.Entities.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntryLog.Data.SqlLegacy.Configs
{
    public class EmployeeConfig : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("empleado");

            builder.HasKey(x => x.Code);

            builder.Property(x => x.Code).HasColumnName("codigo_empleado");
            builder.Property(x => x.FullName).HasColumnName("nombres");
            builder.Property(x => x.PositionId).HasColumnName("id_cargo");
            builder.Property(x => x.DateOfBirthday)
                .HasColumnName("fecha_nacimiento")
                .HasColumnType("datetime2");
            builder.Property(x => x.TownName).HasColumnName("ciudad");

            builder.HasOne(x => x.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(x => x.PositionId)
                .HasConstraintName("FK_Position_Employees");
        }
    }
}
