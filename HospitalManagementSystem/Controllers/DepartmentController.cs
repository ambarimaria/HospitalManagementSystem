using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class DepartmentController : Controller
    {
        private readonly AppDbContext _db;
        public DepartmentController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var depts = await _db.Departments
                .Include(d => d.Doctors)
                .Include(d => d.Wards)
                .Where(d => d.IsActive)
                .ToListAsync();
            return View(depts);
        }

        public IActionResult Create() => View(new Department());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department dept)
        {
            if (!ModelState.IsValid) return View(dept);
            _db.Departments.Add(dept);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Department created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var d = await _db.Departments.FindAsync(id);
            if (d == null) return NotFound();
            return View(d);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department dept)
        {
            if (!ModelState.IsValid) return View(dept);
            var existing = await _db.Departments.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Name = dept.Name;
            existing.Description = dept.Description;
            existing.Icon = dept.Icon;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Department updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
