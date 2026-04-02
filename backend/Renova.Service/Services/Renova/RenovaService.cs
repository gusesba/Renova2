using Renova.Domain.Model;
using Renova.Persistence;
using Renova.Service.Commands.Renova;
using Renova.Service.Queries.Renova;

namespace Renova.Service.Services.Renova;

public class RenovaService(RenovaDbContext context) : IRenovaService
{
    private readonly RenovaDbContext _context = context;

    public async Task<RenovaModel?> GetAsync(RenovaQuery request, CancellationToken cancellationToken = default)
    {
        return await _context.Renova.FindAsync([request.CampoQuery], cancellationToken);
    }

    public async Task<RenovaModel> CreateAsync(RenovaCommand request, CancellationToken cancellationToken = default)
    {
        RenovaModel renovaModel = new()
        {
            Campo2 = request.Campo2,
            Campo3 = request.Campo3
        };

        var result = await _context.Renova.AddAsync(renovaModel, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return result.Entity;
    }
}
