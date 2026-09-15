using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetSanJose.Domain.Entities;

namespace VetSanJose.Infrastructure.Persistence.Configurations;

public class HistorialMedicoConfiguration : IEntityTypeConfiguration<HistorialMedico>
{
    public void Configure(EntityTypeBuilder<HistorialMedico> builder)
    {
        builder.ToTable("historial_medico");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id");
        builder.Property(h => h.CitaId).HasColumnName("cita_id");
        builder.Property(h => h.MascotaId).HasColumnName("mascota_id");
        builder.Property(h => h.DoctorId).HasColumnName("doctor_id");
        builder.Property(h => h.Diagnostico).HasColumnName("diagnostico");
        builder.Property(h => h.Tratamiento).HasColumnName("tratamiento");
        builder.Property(h => h.Observaciones).HasColumnName("observaciones");
        builder.Property(h => h.Fecha).HasColumnName("fecha");
        builder.Property(h => h.CreatedAt).HasColumnName("created_at");

        builder.HasOne(h => h.Cita)
            .WithMany(c => c.HistorialesMedicos)
            .HasForeignKey(h => h.CitaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(h => h.Mascota)
            .WithMany(m => m.HistorialesMedicos)
            .HasForeignKey(h => h.MascotaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.Doctor)
            .WithMany()
            .HasForeignKey(h => h.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
