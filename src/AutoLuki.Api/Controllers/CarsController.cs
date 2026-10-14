using AutoLuki.Application.Brands;
using AutoLuki.Application.Cars;
using AutoLuki.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace AutoLuki.Api.Controllers;

[ApiController]
[Route("api")]
public class CarsController(ICarService carService, IBrandService brandService) : ControllerBase
{
    [HttpGet("cars")]
    public Task<PagedResult<CarListItemDto>> Search([FromQuery] CarQuery query, CancellationToken cancellationToken) =>
        carService.SearchPublicAsync(query with { Status = null }, cancellationToken);

    [HttpGet("cars/{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<CarDetailDto> Get(int id, CancellationToken cancellationToken) =>
        carService.GetPublicAsync(id, cancellationToken);

    [HttpGet("brands")]
    public Task<IReadOnlyList<BrandDto>> GetBrands(CancellationToken cancellationToken) =>
        brandService.GetAllAsync(cancellationToken);
}
