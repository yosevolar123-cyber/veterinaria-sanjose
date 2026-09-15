using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetSanJose.Domain.Entities;

namespace VetSanJose.Infrastructure.Persistence.Configurations;

public class CitaConfiguration : IEntityTypeConfiguration<Cita>
{
    public void Configure(EntityTypeBuilder<Cita> builder)
    {
        builder.ToTable("citas");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.MascotaId).HasColumnName("mascota_id");
        builder.Property(c => c.DoctorId).HasColumnName("doctor_id");
        builder.Property(c => c.CreadoPor).HasColumnName("creado_por");
        builder.Property(c => c.FechaHora).HasColumnName("fecha_hora");
        builder.Property(c => c.Motivo).HasColumnName("motivo");
        builder.Property(c => c.Estado).HasColumnName("estado");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(c => c.Mascota)
            .WithMany(m => m.Citas)
            .HasForeignKey(c => c.MascotaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Doctor)
            .WithMany()
            .HasForeignKey(c => c.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CreadoPorUsuario)
            .WithMany()
            .HasForeignKey(c => c.CreadoPor)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
