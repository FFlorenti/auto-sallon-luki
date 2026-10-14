using AutoLuki.Application.Common;

namespace AutoLuki.Application.Cars;

public interface ICarService
{
    Task<PagedResult<CarListItemDto>> SearchPublicAsync(CarQuery query, CancellationToken cancellationToken = default);

    Task<PagedResult<CarListItemDto>> SearchAdminAsync(CarQuery query, CancellationToken cancellationToken = default);

    Task<CarDetailDto> GetPublicAsync(int id, CancellationToken cancellationToken = default);

    Task<AdminCarDetailDto> GetAdminAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateAsync(SaveCarRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(int id, SaveCarRequest request, CancellationToken cancellationToken = default);

    Task ChangeStatusAsync(int id, ChangeCarStatusRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
