using AutoLuki.Application.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoLuki.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = AuthPolicies.StaffOrAdmin)]
public class AdminDashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    public Task<DashboardDto> Get(CancellationToken cancellationToken) => dashboardService.GetAsync(cancellationToken);
}
