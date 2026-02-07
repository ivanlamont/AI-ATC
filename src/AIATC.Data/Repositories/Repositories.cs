using Microsoft.EntityFrameworkCore;
using AIATC.Domain.Models.Aviation;

namespace AIATC.Data.Repositories;

/// <summary>
/// Aircraft Type repository implementation using Entity Framework
/// </summary>
public class AircraftTypeRepository : IAircraftTypeRepository
{
    private readonly AviationDbContext _context;

    public AircraftTypeRepository(AviationDbContext context)
    {
        _context = context;
    }

    public async Task<AircraftType?> GetByIcaoCodeAsync(string icaoCode)
    {
        return await _context.AircraftTypes
            .FirstOrDefaultAsync(at => at.IcaoCode == icaoCode.ToUpper());
    }

    public async Task<IEnumerable<AircraftType>> GetAllAsync()
    {
        return await _context.AircraftTypes
            .OrderBy(at => at.IcaoCode)
            .ToListAsync();
    }

    public async Task<IEnumerable<AircraftType>> GetByManufacturerAsync(string manufacturer)
    {
        return await _context.AircraftTypes
            .Where(at => at.Name.Contains(manufacturer))
            .OrderBy(at => at.Name)
            .ToListAsync();
    }

    public async Task<AircraftType> AddAsync(AircraftType aircraftType)
    {
        _context.AircraftTypes.Add(aircraftType);
        await _context.SaveChangesAsync();
        return aircraftType;
    }

    public async Task UpdateAsync(AircraftType aircraftType)
    {
        _context.Entry(aircraftType).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string icaoCode)
    {
        var aircraftType = await _context.AircraftTypes
            .FirstOrDefaultAsync(at => at.IcaoCode == icaoCode);
        if (aircraftType != null)
        {
            _context.AircraftTypes.Remove(aircraftType);
            await _context.SaveChangesAsync();
        }
    }
}