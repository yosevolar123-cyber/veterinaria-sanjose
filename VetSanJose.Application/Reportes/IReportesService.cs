using VetSanJose.Shared.Reportes;

namespace VetSanJose.Application.Reportes;

public interface IReportesService
{
    Task<ReporteFinancieroDto> GetReporteFinancieroAsync(DateOnly? desde, DateOnly? hasta, CancellationToken cancellationToken);

    Task<byte[]> GetReporteFinancieroPdfAsync(DateOnly? desde, DateOnly? hasta, CancellationToken cancellationToken);
}
