using MagicVilla_API.Datos;
using MagicVilla_API.Models;
using MagicVilla_API.Repository.IRepository;

namespace MagicVilla_API.Repository
{
    public class VillaRepository : Repository<Villa>, IVillaRepository
    {
        private readonly ApplicationDbContext _db;
        public VillaRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<Villa> Actualizar(Villa entidad)
        {
            entidad.FechaActualizacion = DateTime.Now;
            _db.Villas.Update(entidad);
            await _db.SaveChangesAsync();
            return entidad;
        }
    }
}
