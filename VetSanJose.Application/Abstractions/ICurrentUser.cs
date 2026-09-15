namespace VetSanJose.Application.Abstractions;

public interface ICurrentUser
{
    long Id { get; }
    string Rol { get; }
    bool EsAdministrador { get; }
    bool EsSecretaria { get; }
    bool EsDoctor { get; }
    bool EsCliente { get; }
}
