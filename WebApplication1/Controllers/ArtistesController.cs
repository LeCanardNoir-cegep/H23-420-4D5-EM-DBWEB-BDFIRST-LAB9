using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
            return View();
        }

        // POST: Artistes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Artiste, Employe")] ArtisteEmployeViewModel artisteEmploye)
        {
            if( artisteEmploye  == null)
            {
                ModelState.AddModelError("Model vide", "Le model est vide");
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

            var result = _context.Employes.FromSqlRaw(query, param.ToArray());

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

            var artiste = await _context.Artistes.FindAsync(id);
            if (artiste == null)
            {
                return NotFound();
            }
            ViewData["EmployeId"] = new SelectList(_context.Employes, "EmployeId", "EmployeId", artiste.EmployeId);
            return View(artiste);
        }

        // POST: Artistes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArtisteId,Specialite,EmployeId")] Artiste artiste)
        {
            if (id != artiste.ArtisteId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(artiste);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArtisteExists(artiste.ArtisteId))
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
            ViewData["EmployeId"] = new SelectList(_context.Employes, "EmployeId", "EmployeId", artiste.EmployeId);
            return View(artiste);
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
