using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;

namespace Shopping.Helpers
{
    public class CombosHelper : ICombosHelper
    {
        private readonly DataContext _context;

        public CombosHelper(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync()
        {
            List<SelectListItem> categories = await _context.Categories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
            })
                .OrderBy(c => c.Text)
                .ToListAsync();
            categories.Insert(0, new SelectListItem { Text = "Seleccione una categoria", Value = "0" });
            return categories;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCountriesAsync()
        {
            List<SelectListItem> countries = await _context.Countries.Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id.ToString(),
            })
                .OrderBy(p => p.Text)
                .ToListAsync();
            countries.Insert(0, new SelectListItem { Text = "Seleccione un país", Value = "0" });
            return countries;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboStatesAsync(int countriId)
        {
            List<SelectListItem> states = await _context.States
                .Where(s => s.Country.Id == countriId)
                .Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString(),
            })
                .OrderBy(s => s.Text)
                .ToListAsync();
            states.Insert(0, new SelectListItem { Text = "Seleccione un Departamento / Estado", Value = "0" });
            return states;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCitiesAsync(int stateId)
        {
            List<SelectListItem> states = await _context.Cities
                .Where(c => c.State.Id == stateId)
                .Select(s => new SelectListItem
                {
                    Text = s.Name,
                    Value = s.Id.ToString(),
                })
                .OrderBy(s => s.Text)
                .ToListAsync();
            states.Insert(0, new SelectListItem { Text = "Seleccione una ciudad", Value = "0" });
            return states;

        }

        

        
    }
}
