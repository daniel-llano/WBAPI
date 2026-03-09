using WBAPI.Domain.Entities;

namespace WBAPI.Application.Interfaces;

/// <summary>
/// Puerto de entrada (driving port) para generación de tokens JWT.
/// </summary>
public interface IJwtService
{
    string GenerateToken(User user);
}
