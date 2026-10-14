using AutoLuki.Application.Common;
using AutoLuki.Application.Inquiries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoLuki.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/inquiries")]
[Authorize(Policy = AuthPolicies.StaffOrAdmin)]
public class AdminInquiriesController(IInquiryService inquiryService) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<InquiryDto>> Search([FromQuery] InquiryQuery query, CancellationToken cancellationToken) =>
        inquiryService.SearchAsync(query, cancellationToken);

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<InquiryDto> Get(int id, CancellationToken cancellationToken) =>
        inquiryService.GetAsync(id, cancellationToken);

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangeStatus(int id, ChangeInquiryStatusRequest request, CancellationToken cancellationToken)
    {
        await inquiryService.ChangeStatusAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await inquiryService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
