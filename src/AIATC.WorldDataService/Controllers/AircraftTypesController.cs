using Microsoft.AspNetCore.Mvc;
using AIATC.Domain.Models.Aviation;
using AIATC.Data.Repositories;

namespace AIATC.WorldDataService.Controllers;

/// <summary>
/// REST API controller for Aircraft Type data
/// Provides endpoints for retrieving aircraft type information including performance characteristics
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AircraftTypesController : ControllerBase
{
    private readonly IAircraftTypeRepository _aircraftTypeRepository;
    private readonly ILogger<AircraftTypesController> _logger;

    public AircraftTypesController(
        IAircraftTypeRepository aircraftTypeRepository,
        ILogger<AircraftTypesController> logger)
    {
        _aircraftTypeRepository = aircraftTypeRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all aircraft types with their performance characteristics
    /// </summary>
    /// <returns>List of all aircraft types</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AircraftType>>> GetAllAircraftTypes()
    {
        try
        {
            var aircraftTypes = await _aircraftTypeRepository.GetAllAsync();
            return Ok(aircraftTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all aircraft types");
            return StatusCode(500, "Internal server error occurred while retrieving aircraft types");
        }
    }

    /// <summary>
    /// Get aircraft type by ICAO code
    /// </summary>
    /// <param name="icaoCode">ICAO aircraft type designator (e.g., B738, A320)</param>
    /// <returns>Aircraft type information</returns>
    [HttpGet("{icaoCode}")]
    public async Task<ActionResult<AircraftType>> GetAircraftType(string icaoCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(icaoCode))
            {
                return BadRequest("ICAO code is required");
            }

            var aircraftType = await _aircraftTypeRepository.GetByIcaoCodeAsync(icaoCode);
            
            if (aircraftType == null)
            {
                return NotFound($"Aircraft type with ICAO code '{icaoCode}' not found");
            }

            return Ok(aircraftType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving aircraft type with ICAO code: {IcaoCode}", icaoCode);
            return StatusCode(500, "Internal server error occurred while retrieving aircraft type");
        }
    }

    /// <summary>
    /// Get aircraft types by manufacturer name
    /// </summary>
    /// <param name="manufacturer">Manufacturer name (e.g., Boeing, Airbus)</param>
    /// <returns>List of aircraft types from the specified manufacturer</returns>
    [HttpGet("manufacturer/{manufacturer}")]
    public async Task<ActionResult<IEnumerable<AircraftType>>> GetAircraftTypesByManufacturer(string manufacturer)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(manufacturer))
            {
                return BadRequest("Manufacturer name is required");
            }

            var aircraftTypes = await _aircraftTypeRepository.GetByManufacturerAsync(manufacturer);
            return Ok(aircraftTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving aircraft types for manufacturer: {Manufacturer}", manufacturer);
            return StatusCode(500, "Internal server error occurred while retrieving aircraft types");
        }
    }

    /// <summary>
    /// Create a new aircraft type
    /// </summary>
    /// <param name="aircraftType">Aircraft type data to create</param>
    /// <returns>Created aircraft type</returns>
    [HttpPost]
    public async Task<ActionResult<AircraftType>> CreateAircraftType([FromBody] AircraftType aircraftType)
    {
        try
        {
            if (aircraftType == null)
            {
                return BadRequest("Aircraft type data is required");
            }

            if (string.IsNullOrWhiteSpace(aircraftType.IcaoCode))
            {
                return BadRequest("ICAO code is required");
            }

            // Check if aircraft type already exists
            var existingType = await _aircraftTypeRepository.GetByIcaoCodeAsync(aircraftType.IcaoCode);
            if (existingType != null)
            {
                return Conflict($"Aircraft type with ICAO code '{aircraftType.IcaoCode}' already exists");
            }

            var createdType = await _aircraftTypeRepository.AddAsync(aircraftType);
            return CreatedAtAction(nameof(GetAircraftType), new { icaoCode = createdType.IcaoCode }, createdType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating aircraft type");
            return StatusCode(500, "Internal server error occurred while creating aircraft type");
        }
    }

    /// <summary>
    /// Update an existing aircraft type
    /// </summary>
    /// <param name="icaoCode">ICAO code of aircraft type to update</param>
    /// <param name="aircraftType">Updated aircraft type data</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{icaoCode}")]
    public async Task<IActionResult> UpdateAircraftType(string icaoCode, [FromBody] AircraftType aircraftType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(icaoCode))
            {
                return BadRequest("ICAO code is required");
            }

            if (aircraftType == null)
            {
                return BadRequest("Aircraft type data is required");
            }

            if (icaoCode != aircraftType.IcaoCode)
            {
                return BadRequest("ICAO code in path must match aircraft type ICAO code");
            }

            var existingType = await _aircraftTypeRepository.GetByIcaoCodeAsync(icaoCode);
            if (existingType == null)
            {
                return NotFound($"Aircraft type with ICAO code '{icaoCode}' not found");
            }

            await _aircraftTypeRepository.UpdateAsync(aircraftType);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating aircraft type with ICAO code: {IcaoCode}", icaoCode);
            return StatusCode(500, "Internal server error occurred while updating aircraft type");
        }
    }

    /// <summary>
    /// Delete an aircraft type
    /// </summary>
    /// <param name="icaoCode">ICAO code of aircraft type to delete</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{icaoCode}")]
    public async Task<IActionResult> DeleteAircraftType(string icaoCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(icaoCode))
            {
                return BadRequest("ICAO code is required");
            }

            var existingType = await _aircraftTypeRepository.GetByIcaoCodeAsync(icaoCode);
            if (existingType == null)
            {
                return NotFound($"Aircraft type with ICAO code '{icaoCode}' not found");
            }

            await _aircraftTypeRepository.DeleteAsync(icaoCode);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting aircraft type with ICAO code: {IcaoCode}", icaoCode);
            return StatusCode(500, "Internal server error occurred while deleting aircraft type");
        }
    }
}