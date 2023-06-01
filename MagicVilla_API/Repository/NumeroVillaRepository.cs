using MagicVilla_API.Datos;
using MagicVilla_API.Models;
using MagicVilla_API.Repository.IRepository;

namespace MagicVilla_API.Repository
{
    public class NumeroVillaRepository : Repository<NumeroVilla>, INumeroVillaRepository
    {
        private readonly ApplicationDbContext _db;
        public NumeroVillaRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<NumeroVilla> Actualizar(NumeroVilla entidad)
        {
            entidad.FechaActualizacion = DateTime.Now;
            _db.NumeroVillas.Update(entidad);
            await _db.SaveChangesAsync();
            return entidad;
        }
    }
}
