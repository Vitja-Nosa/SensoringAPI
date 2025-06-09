using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoringAPI.Attributes;
using SensoringAPI.Data;
using SensoringAPI.Models;
using SensoringAPI.Repositories;

namespace SensoringAPI.Controllers;

[ApiController]
[Route("api/wastedetection")]
public class WasteDetectionController : ControllerBase
{
    private readonly WasteDetectionRepository wasteDetectionRepository;
    private readonly WasteDetectionDBContext _context;
    public WasteDetectionController(WasteDetectionDBContext context, WasteDetectionRepository wasteDetectionRepository)
    {
        _context = context;
        this.wasteDetectionRepository = wasteDetectionRepository;
    }

    // POST /api/wastedetection
    [ApiPasswordAuthorize(writeRequired: true)]
    [HttpPost]
    public async Task<IActionResult> AddWasteDetection([FromBody] WasteDetection wasteDetection)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        wasteDetection = wasteDetectionRepository.EnrichWithWeather(wasteDetection);

        _context.WasteDetections.Add(wasteDetection);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWasteDetections), new { id = wasteDetection.Id }, wasteDetection);
    }

    // POST api/wastedetection/bulk
    [ApiPasswordAuthorize(writeRequired: true)]
    [HttpPost("bulk")]
    public async Task<IActionResult> AddWasteDetections([FromBody] List<WasteDetection> wasteDetections)
    {
        if (wasteDetections == null || wasteDetections.Count == 0)
            return BadRequest("No waste detections provided.");

        // Validate and enrich each item
        wasteDetections = wasteDetectionRepository.EnrichWithWeather(wasteDetections); 

        _context.WasteDetections.AddRange(wasteDetections);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWasteDetections), null, wasteDetections);
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

        // Validate query parameters
        var errors = wasteDetectionRepository.ValidateQueryParameters(
            pageNumber,
            pageSize,
            confidenceMin,
            confidenceMax,
            fromDate,
            toDate,
            type,
            weatherCondition);
        if(errors != null)
            return BadRequest(new { errors });

        // Creating the query with filtering and pagination
        var query = _context.WasteDetections.AsNoTracking().AsQueryable();
        query = wasteDetectionRepository.ApplyFilters(
            query,
            cameraId,
            fromWasteDetectionId,
            type,
            weatherCondition,
            fromDate,
            toDate,
            confidenceMin,
            confidenceMax
        );

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
