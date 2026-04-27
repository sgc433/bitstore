using Bitstore.Application.Abstractions;
using Bitstore.Application.DTO.License;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bitstore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LicenseController(ILicenseService licenseService) : Controller
{
    private readonly ILicenseService _licenseService = licenseService;

    [HttpGet("beat/{beatId}")]
    public async Task<ActionResult<List<LicenseResponse>>> GetLicensesByBeat(Guid beatId)
    {
        if (beatId == Guid.Empty)
            throw new ArgumentException("Beat id cannot be empty");

        var licenses = await _licenseService.GetLicensesByBeat(beatId);
        return Ok(licenses);
    }

    [HttpGet("{licenseId}")]
    public async Task<ActionResult<LicenseResponse>> GetLicenseById(Guid licenseId)
    {
        if (licenseId == Guid.Empty)
            throw new ArgumentException("License id cannot be empty");

        var license = await _licenseService.GetLicenseById(licenseId);
        return Ok(license);
    }

    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> CreateLicense(Guid beatId, LicenseRequest request)
    {
        if (beatId == Guid.Empty)
            throw new ArgumentException("Beat id cannot be empty");
            
        await _licenseService.CreateLicense(beatId, request);
        return Ok();
    }

    [Authorize]
    [HttpPut("{licenseId}")]
    public async Task<IActionResult> UpdateLicense(Guid licenseId, LicenseRequest request)
    {
        if (licenseId == Guid.Empty)
            throw new ArgumentException("License id cannot be empty");

        await _licenseService.UpdateLicense(licenseId, request);
        return Ok();
    }

    [Authorize]
    [HttpDelete("{licenseId}")]
    public async Task<ActionResult<bool>> DeleteLicense(Guid licenseId)
    {
        if (licenseId == Guid.Empty)
            throw new ArgumentException("License id cannot be empty");

        var result = await _licenseService.DeleteLicense(licenseId);
        return Ok(result);
    }
}