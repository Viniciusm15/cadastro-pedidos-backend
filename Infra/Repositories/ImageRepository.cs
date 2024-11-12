using Domain.Interfaces;
using Domain.Models.Entities;
using Infra.Data;
using Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class ImageRepository : GenericRepository<Image>, IImageRepository
    {
        private readonly ApplicationDbContext _context;

        public ImageRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Image?> GetImageByIdAsync(int id)
        {
            return await _context.Images
                .WhereActive()
                .FirstOrDefaultAsync(image => image.Id == id);
        }
    }
}
