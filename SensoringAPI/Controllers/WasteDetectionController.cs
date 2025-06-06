using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoringAPI.Attributes;
using SensoringAPI.Data;
using SensoringAPI.Models;

namespace SensoringAPI.Controllers;

[ApiController]
[Route("wastedetection")]
public class WasteDetectionController : ControllerBase
{
    private readonly WasteDetectionDBContext _context;
    public WasteDetectionController(WasteDetectionDBContext context)
    {
        _context = context;
    }

    // POST /api/wastedetection
    [ApiPasswordAuthorize(writeRequired: true)]
    [HttpPost]
    public async Task<IActionResult> AddWasteDetection([FromBody] WasteDetection wasteDetection)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Verrijking (mock voorbeeld)
        wasteDetection.Temperature = GetTemperature(wasteDetection.Location);
        wasteDetection.WeatherCondition = GetWeatherCondition(wasteDetection.Location);

        _context.WasteDetections.Add(wasteDetection);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWasteDetections), new { id = wasteDetection.Id }, wasteDetection);
    }

    // GET /api/wastedetection
    [ApiPasswordAuthorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WasteDetection>>> GetWasteDetections(
        // Query parameters for filtering and pagination`
        [FromQuery] string? cameraId,
        [FromQuery] int? fromWasteDetectionId,
        [FromQuery] string? type,
        [FromQuery] string? weatherCondition,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] float? confidenceMin,
        [FromQuery] float? confidenceMax,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 100)
    {
        // Make sure pageSize does not exceed a maximum limit
        const int maxPageSize = 200;
        pageSize = Math.Min(pageSize, maxPageSize);

        // Creating the query with filtering and pagination
        var query = _context.WasteDetections.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(cameraId))
            query = query.Where(w => w.CameraId == cameraId);

        if (fromWasteDetectionId.HasValue)
            query = query.Where(w => w.Id >= fromWasteDetectionId.Value);

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(w => w.Type == type);

        if (!string.IsNullOrWhiteSpace(weatherCondition))
            query = query.Where(w => w.WeatherCondition == weatherCondition);

        if (fromDate.HasValue)
            query = query.Where(w => w.DateTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(w => w.DateTime <= toDate.Value);

        if (confidenceMin.HasValue)
            query = query.Where(w => w.Confidence >= confidenceMin.Value);

        if (confidenceMax.HasValue)
            query = query.Where(w => w.Confidence <= confidenceMax.Value);


        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var results = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = new
        {
            pageNumber,
            pageSize,
            totalPages,
            totalCount,
            data = results
        };

        return Ok(response);
    }
}
