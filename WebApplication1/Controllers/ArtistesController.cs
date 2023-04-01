using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class ArtistesController : Controller
    {
        private readonly Lab09_EmployesContext _context;

        public ArtistesController(Lab09_EmployesContext context)
        {
            _context = context;
        }

        // GET: Artistes
        public async Task<IActionResult> Index()
        {
            var lab09_EmployesContext = _context.Artistes.Include(a => a.Employe);
            return View(await lab09_EmployesContext.ToListAsync());
        }

        // GET: Artistes
        public async Task<IActionResult> Index2()
        {
            IEnumerable< VwListeArtiste > artiste = await _context.VwListeArtistes.ToArrayAsync();
            return View(artiste);
        }

        // GET: Artistes
        public async Task<IActionResult> Query1()
        {
            IEnumerable<VwListeArtiste> artiste = await _context.VwListeArtistes.Where( a => a.DateEmbauche.Year == 2023 ).ToListAsync();
            return View(artiste);
        }

        // GET: Artistes
        public async Task<IActionResult> Query2()
        {
            IEnumerable<VwListeArtiste> artiste = await _context.VwListeArtistes.Where(a => a.Specialite == "modélisation 3D ").ToListAsync();
            return View(artiste);
        }

        // GET: Artistes
        public async Task<IActionResult> Query3()
        {
            IEnumerable<string> artiste = await _context.VwListeArtistes.OrderBy( o => o.Prenom ).Select( s => $"{s.Prenom} {s.Nom}"  ).ToListAsync();
            return View(artiste);
        }

        // GET: Artistes
        public async Task<IActionResult> Query4()
        {
            IEnumerable<ArtisteEmployeViewModel> artisteVM =    from a in _context.Artistes
                                                                join e in _context.Employes
                                                                on a.EmployeId equals e.EmployeId
                                                                select new ArtisteEmployeViewModel { Artiste = a, Employe = e };

            IEnumerable<ArtisteEmployeViewModel> artisteVM2 = await _context.Artistes.Join(
                _context.Employes,                                                  // INNER JOIN
                a => a.EmployeId, e => e.EmployeId,                                 // ON
                (a,e) => new ArtisteEmployeViewModel { Artiste = a, Employe = e}    // SELECT
                ).ToListAsync();

            return View(artisteVM2);
        }

        // GET: Artistes
        public async Task<IActionResult> Query5()
        {
            IEnumerable<NbSpecialiteViewModel> vm = from a in _context.Artistes
                                                    group a.ArtisteId by a.Specialite into result
                                                    select new NbSpecialiteViewModel { Specialite = result.Key, Nb = result.Count() };

            IEnumerable<NbSpecialiteViewModel> vm2 = _context.Artistes.GroupBy( g => g.Specialite ).Select( s => new NbSpecialiteViewModel { Specialite = s.Key, Nb = s.Count() });

            return View(vm2);
        }

        // GET: Artistes
        public async Task<IActionResult> Query6()
        {
            //NOT GOOD: Les noms de specialités ayant le prlus grand nombre de lettre.
            IEnumerable<NbSpecialiteViewModel> vm = _context.Artistes.GroupBy(g => g.Specialite).Select(s => new NbSpecialiteViewModel { Specialite = s.Key, Nb = s.Key.Length }).OrderByDescending(o => o.Nb).Take(2);

            // Les deux spécialités dont la quantité moyenne de lettres dans le prénom des artistes est la plus grande ? 
            IEnumerable<NbSpecialiteViewModel> vm2 = (from a in _context.Artistes
                                                      join e in _context.Employes
                                                      on a.EmployeId equals e.EmployeId
                                                      group e by a.Specialite into result
                                                      orderby result.Average(r => r.Prenom.Length) descending
                                                      select new NbSpecialiteViewModel { Specialite = result.Key, Nb = (int)Math.Ceiling(result.Average(r => r.Prenom.Length)) }).Take(2);

            IEnumerable<NbSpecialiteViewModel> vm3 = await _context.Artistes
                .GroupBy(a => new { Specility = a.Specialite })
                .OrderByDescending(s => s.Average(n => n.Employe.Prenom.Length))
                .Select(g => new NbSpecialiteViewModel
                {
                    Specialite = g.Key.Specility,
                    Nb = (int) Math.Ceiling(g.Average(n => n.Employe.Prenom.Length))
                })
                .Take(2).ToListAsync();

            return View(vm3);
        }

        // GET: Artistes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Artistes == null)
            {
                return NotFound();
            }

            var artiste = await _context.Artistes
                .Include(a => a.Employe)
                .FirstOrDefaultAsync(m => m.ArtisteId == id);
            if (artiste == null)
            {
                return NotFound();
            }

            return View(artiste);
        }

        // GET: Artistes/Create
        //public IActionResult Create()
        //{
        //    //ViewData["EmployeId"] = new SelectList(_context.Employes, "EmployeId", "EmployeId");
        //    return View();
        //}
        public async Task<IActionResult> Create()
        {
            // Appel procédure stockée

            //await _context.SaveChangesAsync();
            await Task.CompletedTask;
            return View();
        }

        // POST: Artistes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Artiste, Employe")] ArtisteEmployeViewModel artisteEmploye)
        {
            //artisteEmploye.Artiste.Employe = artisteEmploye.Employe;
            //artisteEmploye
            //if( ModelState.IsValid)
            //{
            //    ModelState.AddModelError("Model vide", "Le model est vide");
            //    return View(artisteEmploye);
            //}

            artisteEmploye.Artiste.Employe = artisteEmploye.Employe;
            bool isValid = Validator.TryValidateObject(
                artisteEmploye,
                new ValidationContext(artisteEmploye),
                new List<ValidationResult>(), 
                true
                );

            if (!isValid)
            {
                ModelState.AddModelError("Error", "Une erreur s'est produite.");
                return View(artisteEmploye);
            }

            string query = "EXEC Employes.USP_AjouterArtiste @Prenom, @Nom, @NoTel, @Courriel, @Specialite";
            List<SqlParameter> param = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName = "@Prenom", Value = artisteEmploye.Employe.Prenom},
                    new SqlParameter{ParameterName = "@Nom", Value = artisteEmploye.Employe.Nom},
                    new SqlParameter{ParameterName = "@NoTel", Value = artisteEmploye.Employe.NoTel},
                    new SqlParameter{ParameterName = "@Courriel", Value = artisteEmploye.Employe.Courriel},
                    new SqlParameter{ParameterName = "@Specialite", Value = artisteEmploye.Artiste.Specialite}
                };


            await _context.Database.ExecuteSqlRawAsync(query, param.ToArray());

            await _context.SaveChangesAsync();
            return View(artisteEmploye);
        }

        // GET: Artistes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Artistes == null)
            {
                return NotFound();
            }

            Artiste artiste = await _context.Artistes.Where( a => a.ArtisteId == id ).Include( a => a.Employe ).FirstOrDefaultAsync();

            ArtisteEmployeViewModel vm = new ArtisteEmployeViewModel
            {
                Artiste = artiste,
                Employe = artiste.Employe
            };

            if (artiste == null)
            {
                return NotFound();
            }
            //ViewData["EmployeId"] = new SelectList(_context.Employes, "EmployeId", "EmployeId", artiste.EmployeId);
            return View(vm);
        }

        // POST: Artistes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Artiste, Employe")] ArtisteEmployeViewModel artisteEmploye)
        {
            if (id != artisteEmploye.Artiste.ArtisteId)
            {
                return NotFound();
            }


            artisteEmploye.Artiste.Employe = artisteEmploye.Employe;
            bool isValid = Validator.TryValidateObject(
                artisteEmploye,
                new ValidationContext(artisteEmploye),
                new List<ValidationResult>(),
                true
                );

            if (isValid)
            {
                try
                {
                    _context.Artistes.Update(artisteEmploye.Artiste);
                    _context.Employes.Update(artisteEmploye.Employe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArtisteExists(artisteEmploye.Artiste.ArtisteId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            //ViewData["EmployeId"] = new SelectList(_context.Employes, "EmployeId", "EmployeId", artiste.EmployeId);
            return View(artisteEmploye);
        }

        // GET: Artistes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Artistes == null)
            {
                return NotFound();
            }

            var artiste = await _context.Artistes
                .Include(a => a.Employe)
                .FirstOrDefaultAsync(m => m.ArtisteId == id);
            if (artiste == null)
            {
                return NotFound();
            }

            return View(artiste);
        }

        // POST: Artistes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Artistes == null)
            {
                return Problem("Entity set 'Lab09_EmployesContext.Artistes'  is null.");
            }
            var artiste = await _context.Artistes.FindAsync(id);
            if (artiste != null)
            {
                _context.Artistes.Remove(artiste);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArtisteExists(int id)
        {
          return (_context.Artistes?.Any(e => e.ArtisteId == id)).GetValueOrDefault();
        }
    }
}
