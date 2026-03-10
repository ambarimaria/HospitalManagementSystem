using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly AppDbContext _db;
        public PatientController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var q = _db.Patients.Where(p => p.IsActive).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(p => (p.FirstName + " " + p.LastName).Contains(search) || p.Phone.Contains(search) || p.Email!.Contains(search));

            int pageSize = 10;
            var total = await q.CountAsync();
            var items = await q.OrderByDescending(p => p.RegisteredAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return View(new PagedResult<Patient> { Items = items, TotalCount = total, Page = page, PageSize = pageSize, Search = search });
        }

        public async Task<IActionResult> Details(int id)
        {
            var p = await _db.Patients
                .Include(x => x.Appointments).ThenInclude(a => a.Doctor)
                .Include(x => x.Bills).ThenInclude(b => b.Items)
                .Include(x => x.Admissions).ThenInclude(a => a.Bed).ThenInclude(b => b!.Ward)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            return View(p);
        }

        public IActionResult Create() => View(new PatientFormViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var p = Map(new Patient(), vm);
            _db.Patients.Add(p);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Patient {p.FirstName} {p.LastName} registered successfully.";
            return RedirectToAction(nameof(Details), new { id = p.Id });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var p = await _db.Patients.FindAsync(id);
            if (p == null) return NotFound();
            return View(MapVm(p));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PatientFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var p = await _db.Patients.FindAsync(id);
            if (p == null) return NotFound();
            Map(p, vm);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Patient updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.Patients.FindAsync(id);
            if (p != null) { p.IsActive = false; await _db.SaveChangesAsync(); }
            TempData["Success"] = "Patient deactivated.";
            return RedirectToAction(nameof(Index));
        }

        private static Patient Map(Patient p, PatientFormViewModel vm)
        {
            p.FirstName = vm.FirstName; p.LastName = vm.LastName;
            p.DateOfBirth = vm.DateOfBirth; p.Gender = vm.Gender;
            p.BloodGroup = vm.BloodGroup; p.Phone = vm.Phone;
            p.Email = vm.Email; p.Address = vm.Address;
            p.EmergencyContact = vm.EmergencyContact; p.EmergencyPhone = vm.EmergencyPhone;
            p.MedicalHistory = vm.MedicalHistory; p.Allergies = vm.Allergies;
            return p;
        }

        private static PatientFormViewModel MapVm(Patient p) => new()
        {
            Id = p.Id, FirstName = p.FirstName, LastName = p.LastName,
            DateOfBirth = p.DateOfBirth, Gender = p.Gender, BloodGroup = p.BloodGroup,
            Phone = p.Phone, Email = p.Email, Address = p.Address,
            EmergencyContact = p.EmergencyContact, EmergencyPhone = p.EmergencyPhone,
            MedicalHistory = p.MedicalHistory, Allergies = p.Allergies
        };
    }
}
