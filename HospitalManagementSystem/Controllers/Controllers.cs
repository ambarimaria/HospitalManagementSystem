using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class DoctorController : Controller
    {
        private readonly AppDbContext _db;
        public DoctorController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var q = _db.Doctors.Include(d => d.Department).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(d => (d.FirstName + " " + d.LastName).Contains(search) || d.Specialization.Contains(search));

            int ps = 10;
            var items = await q.OrderBy(d => d.FirstName).Skip((page - 1) * ps).Take(ps).ToListAsync();
            return View(new PagedResult<Doctor> { Items = items, TotalCount = await q.CountAsync(), Page = page, PageSize = ps, Search = search });
        }

        public async Task<IActionResult> Details(int id)
        {
            var d = await _db.Doctors.Include(x => x.Department).Include(x => x.Appointments).ThenInclude(a => a.Patient).Include(x => x.Schedules).FirstOrDefaultAsync(x => x.Id == id);
            if (d == null) return NotFound();
            return View(d);
        }

        public async Task<IActionResult> Create()
        {
            return View(new DoctorFormViewModel { Departments = await _db.Departments.Where(d => d.IsActive).ToListAsync() });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorFormViewModel vm)
        {
            if (!ModelState.IsValid) { vm.Departments = await _db.Departments.ToListAsync(); return View(vm); }
            var d = new Doctor { FirstName = vm.FirstName, LastName = vm.LastName, Specialization = vm.Specialization, DepartmentId = vm.DepartmentId, Phone = vm.Phone, Email = vm.Email, LicenseNumber = vm.LicenseNumber, Qualification = vm.Qualification, ExperienceYears = vm.ExperienceYears, ConsultationFee = vm.ConsultationFee, IsAvailable = vm.IsAvailable };
            _db.Doctors.Add(d);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Doctor added successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var d = await _db.Doctors.FindAsync(id);
            if (d == null) return NotFound();
            return View(new DoctorFormViewModel { Id = d.Id, FirstName = d.FirstName, LastName = d.LastName, Specialization = d.Specialization, DepartmentId = d.DepartmentId, Phone = d.Phone, Email = d.Email, LicenseNumber = d.LicenseNumber, Qualification = d.Qualification, ExperienceYears = d.ExperienceYears, ConsultationFee = d.ConsultationFee, IsAvailable = d.IsAvailable, Departments = await _db.Departments.ToListAsync() });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DoctorFormViewModel vm)
        {
            if (!ModelState.IsValid) { vm.Departments = await _db.Departments.ToListAsync(); return View(vm); }
            var d = await _db.Doctors.FindAsync(id);
            if (d == null) return NotFound();
            d.FirstName = vm.FirstName; d.LastName = vm.LastName; d.Specialization = vm.Specialization; d.DepartmentId = vm.DepartmentId; d.Phone = vm.Phone; d.Email = vm.Email; d.LicenseNumber = vm.LicenseNumber; d.Qualification = vm.Qualification; d.ExperienceYears = vm.ExperienceYears; d.ConsultationFee = vm.ConsultationFee; d.IsAvailable = vm.IsAvailable;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Doctor updated.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly AppDbContext _db;
        public AppointmentController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? search, string? status, DateTime? date, int page = 1)
        {
            var q = _db.Appointments.Include(a => a.Patient).Include(a => a.Doctor).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(a => (a.Patient!.FirstName + " " + a.Patient.LastName).Contains(search) || (a.Doctor!.FirstName + " " + a.Doctor.LastName).Contains(search));
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, out var s)) q = q.Where(a => a.Status == s);
            if (date.HasValue) q = q.Where(a => a.AppointmentDate.Date == date.Value.Date);

            int ps = 15;
            var totalCount = await q.CountAsync();

            // ✅ Fetch page in SQL ordered by date, then sort TimeSpan in C# — SQLite cannot ORDER BY TimeSpan
            var raw = await q.OrderByDescending(a => a.AppointmentDate)
                .Skip((page - 1) * ps).Take(ps).ToListAsync();
            var items = raw.OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime).ToList();

            ViewBag.Status = status; ViewBag.Date = date;
            return View(new PagedResult<Appointment> { Items = items, TotalCount = totalCount, Page = page, PageSize = ps, Search = search });
        }

        public async Task<IActionResult> Details(int id)
        {
            var a = await _db.Appointments.Include(x => x.Patient).Include(x => x.Doctor).ThenInclude(d => d!.Department).FirstOrDefaultAsync(x => x.Id == id);
            if (a == null) return NotFound();
            return View(a);
        }

        public async Task<IActionResult> Create()
        {
            return View(new AppointmentFormViewModel
            {
                Patients = await _db.Patients.Where(p => p.IsActive).OrderBy(p => p.FirstName).ToListAsync(),
                Doctors = await _db.Doctors.Where(d => d.IsAvailable).Include(d => d.Department).OrderBy(d => d.FirstName).ToListAsync()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Patients = await _db.Patients.Where(p => p.IsActive).ToListAsync();
                vm.Doctors = await _db.Doctors.Where(d => d.IsAvailable).Include(d => d.Department).ToListAsync();
                return View(vm);
            }
            var a = new Appointment { PatientId = vm.PatientId, DoctorId = vm.DoctorId, AppointmentDate = vm.AppointmentDate, AppointmentTime = TimeSpan.Parse(vm.AppointmentTime), Reason = vm.Reason, Status = vm.Status };
            _db.Appointments.Add(a);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Appointment scheduled.";
            return RedirectToAction(nameof(Details), new { id = a.Id });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var a = await _db.Appointments.FindAsync(id);
            if (a == null) return NotFound();
            return View(new AppointmentFormViewModel
            {
                Id = a.Id, PatientId = a.PatientId, DoctorId = a.DoctorId,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime.ToString(@"hh\:mm"),
                Reason = a.Reason, Status = a.Status, Notes = a.Notes,
                Diagnosis = a.Diagnosis, Prescription = a.Prescription,
                Patients = await _db.Patients.Where(p => p.IsActive).ToListAsync(),
                Doctors = await _db.Doctors.Where(d => d.IsAvailable).Include(d => d.Department).ToListAsync()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentFormViewModel vm)
        {
            if (!ModelState.IsValid) { vm.Patients = await _db.Patients.ToListAsync(); vm.Doctors = await _db.Doctors.Include(d => d.Department).ToListAsync(); return View(vm); }
            var a = await _db.Appointments.FindAsync(id);
            if (a == null) return NotFound();
            a.PatientId = vm.PatientId; a.DoctorId = vm.DoctorId; a.AppointmentDate = vm.AppointmentDate; a.AppointmentTime = TimeSpan.Parse(vm.AppointmentTime); a.Reason = vm.Reason; a.Status = vm.Status; a.Notes = vm.Notes; a.Diagnosis = vm.Diagnosis; a.Prescription = vm.Prescription;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Appointment updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var a = await _db.Appointments.FindAsync(id);
            if (a != null) { a.Status = AppointmentStatus.Cancelled; await _db.SaveChangesAsync(); }
            TempData["Info"] = "Appointment cancelled.";
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize]
    public class WardController : Controller
    {
        private readonly AppDbContext _db;
        public WardController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var wards = await _db.Wards.Include(w => w.Department).Include(w => w.Beds).Where(w => w.IsActive).ToListAsync();
            return View(wards);
        }

        public async Task<IActionResult> Details(int id)
        {
            var w = await _db.Wards.Include(x => x.Department).Include(x => x.Beds).ThenInclude(b => b.Admissions).ThenInclude(a => a.Patient).FirstOrDefaultAsync(x => x.Id == id);
            if (w == null) return NotFound();
            return View(w);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBedStatus(int bedId, BedStatus status)
        {
            var bed = await _db.Beds.FindAsync(bedId);
            if (bed != null) { bed.Status = status; await _db.SaveChangesAsync(); TempData["Success"] = "Bed status updated."; }
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize]
    public class AdmissionController : Controller
    {
        private readonly AppDbContext _db;
        public AdmissionController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var admissions = await _db.Admissions.Include(a => a.Patient).Include(a => a.Bed).ThenInclude(b => b!.Ward).Include(a => a.AdmittingDoctor).OrderByDescending(a => a.AdmissionDate).ToListAsync();
            return View(admissions);
        }

        public async Task<IActionResult> Create()
        {
            return View(new AdmissionFormViewModel
            {
                Patients = await _db.Patients.Where(p => p.IsActive).OrderBy(p => p.FirstName).ToListAsync(),
                Doctors = await _db.Doctors.Where(d => d.IsAvailable).ToListAsync(),
                Wards = await _db.Wards.Include(w => w.Beds).Where(w => w.IsActive).ToListAsync()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdmissionFormViewModel vm)
        {
            if (!ModelState.IsValid) { vm.Patients = await _db.Patients.ToListAsync(); vm.Doctors = await _db.Doctors.ToListAsync(); vm.Wards = await _db.Wards.Include(w => w.Beds).ToListAsync(); return View(vm); }
            var bed = await _db.Beds.FindAsync(vm.BedId);
            if (bed == null || bed.Status != BedStatus.Available) { ModelState.AddModelError("", "Selected bed is not available."); return View(vm); }
            bed.Status = BedStatus.Occupied;
            var adm = new Admission { PatientId = vm.PatientId, BedId = vm.BedId, AdmittingDoctorId = vm.AdmittingDoctorId, AdmissionDate = vm.AdmissionDate, AdmissionReason = vm.AdmissionReason };
            _db.Admissions.Add(adm);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Patient admitted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Discharge(int id, string? notes)
        {
            var adm = await _db.Admissions.Include(a => a.Bed).FirstOrDefaultAsync(a => a.Id == id);
            if (adm == null) return NotFound();
            adm.DischargeDate = DateTime.Now;
            adm.DischargeNotes = notes;
            if (adm.Bed != null) adm.Bed.Status = BedStatus.Available;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Patient discharged.";
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize]
    public class BillingController : Controller
    {
        private readonly AppDbContext _db;
        public BillingController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? status, int page = 1)
        {
            var q = _db.Bills.Include(b => b.Patient).Include(b => b.Items).AsQueryable();
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BillStatus>(status, out var s)) q = q.Where(b => b.Status == s);
            int ps = 10;
            var items = await q.OrderByDescending(b => b.BillDate).Skip((page - 1) * ps).Take(ps).ToListAsync();
            ViewBag.Status = status;
            return View(new PagedResult<Bill> { Items = items, TotalCount = await q.CountAsync(), Page = page, PageSize = ps });
        }

        public async Task<IActionResult> Details(int id)
        {
            var b = await _db.Bills.Include(x => x.Patient).Include(x => x.Items).Include(x => x.Payments).Include(x => x.Appointment).ThenInclude(a => a!.Doctor).FirstOrDefaultAsync(x => x.Id == id);
            if (b == null) return NotFound();
            return View(b);
        }

        public async Task<IActionResult> Create(int? patientId, int? appointmentId)
        {
            var vm = new BillFormViewModel
            {
                Patients = await _db.Patients.Where(p => p.IsActive).OrderBy(p => p.FirstName).ToListAsync(),
                DueDate = DateTime.Today.AddDays(15)
            };
            if (patientId.HasValue) vm.PatientId = patientId.Value;
            if (appointmentId.HasValue)
            {
                vm.AppointmentId = appointmentId;
                var appt = await _db.Appointments.Include(a => a.Doctor).FirstOrDefaultAsync(a => a.Id == appointmentId);
                if (appt?.Doctor != null)
                    vm.Items.Add(new BillItemViewModel { Description = $"Consultation - {appt.Doctor.FirstName} {appt.Doctor.LastName}", Category = "Consultation", Quantity = 1, UnitPrice = appt.Doctor.ConsultationFee });
            }
            if (!vm.Items.Any()) vm.Items.Add(new BillItemViewModel { Category = "Consultation", Quantity = 1 });
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BillFormViewModel vm)
        {
            var validItems = vm.Items.Where(i => !string.IsNullOrWhiteSpace(i.Description) && i.UnitPrice > 0).ToList();
            if (!validItems.Any()) ModelState.AddModelError("", "At least one item is required.");
            if (!ModelState.IsValid) { vm.Patients = await _db.Patients.ToListAsync(); return View(vm); }

            var subTotal = validItems.Sum(i => i.Quantity * i.UnitPrice);
            var bill = new Bill
            {
                PatientId = vm.PatientId, AppointmentId = vm.AppointmentId, AdmissionId = vm.AdmissionId,
                DueDate = vm.DueDate, Discount = vm.Discount, TaxPercent = vm.TaxPercent,
                SubTotal = subTotal, Notes = vm.Notes,
                Items = validItems.Select(i => new BillItem { Description = i.Description, Category = i.Category, Quantity = i.Quantity, UnitPrice = i.UnitPrice }).ToList()
            };
            _db.Bills.Add(bill);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Bill created.";
            return RedirectToAction(nameof(Details), new { id = bill.Id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(int billId, decimal amount, string method, string? transactionRef)
        {
            var bill = await _db.Bills.Include(b => b.Payments).FirstOrDefaultAsync(b => b.Id == billId);
            if (bill == null) return NotFound();
            bill.Payments.Add(new Payment { Amount = amount, PaymentMethod = method, TransactionRef = transactionRef });
            bill.PaidAmount += amount;
            var tax = (bill.SubTotal - bill.Discount) * bill.TaxPercent / 100;
            var total = bill.SubTotal - bill.Discount + tax;
            bill.Status = bill.PaidAmount >= total ? BillStatus.Paid : bill.PaidAmount > 0 ? BillStatus.Partial : BillStatus.Pending;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Payment of ${amount:F2} recorded.";
            return RedirectToAction(nameof(Details), new { id = billId });
        }
    }

    [Authorize]
    public class InventoryController : Controller
    {
        private readonly AppDbContext _db;
        public InventoryController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? category, bool lowStock = false)
        {
            var q = _db.InventoryItems.Where(i => i.IsActive).AsQueryable();
            if (!string.IsNullOrWhiteSpace(category)) q = q.Where(i => i.Category == category);
            if (lowStock) q = q.Where(i => i.CurrentStock <= i.MinimumStock);
            var items = await q.OrderBy(i => i.Name).ToListAsync();
            ViewBag.Category = category; ViewBag.LowStock = lowStock;
            ViewBag.Categories = await _db.InventoryItems.Select(i => i.Category).Distinct().ToListAsync();
            return View(items);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int id, int change, string reason)
        {
            var item = await _db.InventoryItems.FindAsync(id);
            if (item != null) { item.CurrentStock = Math.Max(0, item.CurrentStock + change); await _db.SaveChangesAsync(); TempData["Success"] = $"Stock updated for {item.Name}."; }
            return RedirectToAction(nameof(Index));
        }
    }
}
