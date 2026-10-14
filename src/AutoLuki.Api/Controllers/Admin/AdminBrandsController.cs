using AutoLuki.Application.Brands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoLuki.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/brands")]
[Authorize(Policy = AuthPolicies.StaffOrAdmin)]
public class AdminBrandsController(IBrandService brandService) : ControllerBase
{
    [HttpPost]
    public Task<BrandDto> CreateBrand(CreateNameRequest request, CancellationToken cancellationToken) =>
        brandService.CreateBrandAsync(request, cancellationToken);

    [HttpPost("{brandId:int}/models")]
    public Task<ModelDto> CreateModel(int brandId, CreateNameRequest request, CancellationToken cancellationToken) =>
        brandService.CreateModelAsync(brandId, request, cancellationToken);
}
