using AutoLuki.Application.Cars;
using AutoLuki.Application.Cars.Photos;
using AutoLuki.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoLuki.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/cars")]
[Authorize(Policy = AuthPolicies.StaffOrAdmin)]
public class AdminCarsController(ICarService carService, ICarPhotoService photoService) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<CarListItemDto>> Search([FromQuery] CarQuery query, CancellationToken cancellationToken) =>
        carService.SearchAdminAsync(query, cancellationToken);

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<AdminCarDetailDto> Get(int id, CancellationToken cancellationToken) =>
        carService.GetAdminAsync(id, cancellationToken);

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(SaveCarRequest request, CancellationToken cancellationToken)
    {
        var id = await carService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, SaveCarRequest request, CancellationToken cancellationToken)
    {
        await carService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeStatus(int id, ChangeCarStatusRequest request, CancellationToken cancellationToken)
    {
        await carService.ChangeStatusAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await carService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/photos")]
    [RequestSizeLimit(100 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
    public async Task<IReadOnlyList<CarPhotoDto>> UploadPhotos(
        int id, [FromForm] IFormFileCollection files, CancellationToken cancellationToken)
    {
        var streams = files.Select(f => f.OpenReadStream()).ToList();
        try
        {
            var uploads = files.Select((f, i) => new PhotoUpload(streams[i], f.Length)).ToList();
            return await photoService.UploadAsync(id, uploads, cancellationToken);
        }
        finally
        {
            foreach (var stream in streams)
            {
                await stream.DisposeAsync();
            }
        }
    }

    [HttpDelete("{id:int}/photos/{photoId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeletePhoto(int id, int photoId, CancellationToken cancellationToken)
    {
        await photoService.DeleteAsync(id, photoId, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:int}/photos/order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReorderPhotos(int id, ReorderPhotosRequest request, CancellationToken cancellationToken)
    {
        await photoService.ReorderAsync(id, request, cancellationToken);
        return NoContent();
    }
}
