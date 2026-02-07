using AIATC.Domain.Models.Aviation;

namespace AIATC.Data.Repositories;

/// <summary>
/// Repository interface for Aircraft Type data access
/// </summary>
public interface IAircraftTypeRepository
{
    Task<AircraftType?> GetByIcaoCodeAsync(string icaoCode);
    Task<IEnumerable<AircraftType>> GetAllAsync();
    Task<IEnumerable<AircraftType>> GetByManufacturerAsync(string manufacturer);
    Task<AircraftType> AddAsync(AircraftType aircraftType);
    Task UpdateAsync(AircraftType aircraftType);
    Task DeleteAsync(string icaoCode);
}