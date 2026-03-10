using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class StaffController : Controller
    {
        private readonly AppDbContext _db;
        public StaffController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? search)
        {
            var q = _db.Staff.Include(s => s.Department).Where(s => s.IsActive).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(s => (s.FirstName + " " + s.LastName).Contains(search) || s.Role.Contains(search));

            ViewBag.Search = search;
            return View(await q.OrderBy(s => s.FirstName).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = await _db.Departments.Where(d => d.IsActive).ToListAsync();
            return View(new Staff());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Staff staff)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await _db.Departments.ToListAsync();
                return View(staff);
            }
            _db.Staff.Add(staff);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Staff member added.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var s = await _db.Staff.FindAsync(id);
            if (s == null) return NotFound();
            ViewBag.Departments = await _db.Departments.ToListAsync();
            return View(s);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Staff staff)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await _db.Departments.ToListAsync();
                return View(staff);
            }
            var existing = await _db.Staff.FindAsync(id);
            if (existing == null) return NotFound();
            existing.FirstName = staff.FirstName;
            existing.LastName = staff.LastName;
            existing.Role = staff.Role;
            existing.DepartmentId = staff.DepartmentId;
            existing.Phone = staff.Phone;
            existing.Email = staff.Email;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Staff updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var s = await _db.Staff.FindAsync(id);
            if (s != null) { s.IsActive = false; await _db.SaveChangesAsync(); }
            TempData["Info"] = "Staff member deactivated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
