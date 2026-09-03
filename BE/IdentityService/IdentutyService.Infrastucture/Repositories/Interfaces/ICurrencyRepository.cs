using IdentityService.Infrastructure.Models;

namespace IdentityService.Infrastructure.Repositories.Interfaces;

public interface ICurrencyRepository
{
    Task<IEnumerable<Currency>> GetAllActiveAsync();
    Task<bool>                  ExistsActiveAsync(long id);
}
