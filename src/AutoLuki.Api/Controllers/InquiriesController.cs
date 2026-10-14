using AutoLuki.Application.Inquiries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoLuki.Api.Controllers;

[ApiController]
[Route("api/inquiries")]
public class InquiriesController(IInquiryService inquiryService) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.Inquiry)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Create(CreateInquiryRequest request, CancellationToken cancellationToken)
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        await inquiryService.CreateAsync(request, remoteIp, cancellationToken);

        return Accepted();
    }
}
