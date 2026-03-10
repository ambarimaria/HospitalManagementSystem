using HospitalManagementSystem.Data;
using HospitalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        public HomeController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var bills = await _db.Bills.Include(b => b.Items).ToListAsync();
            var todayBills = bills.Where(b => b.BillDate.Date == today);
            var monthBills = bills.Where(b => b.BillDate >= monthStart);

            // ✅ Pull statuses into memory first, then group in C# — fixes SQLite ToString() error
            var appointmentStatuses = await _db.Appointments
                .Select(a => a.Status)
                .ToListAsync();

            var vm = new DashboardViewModel
            {
                TotalPatients = await _db.Patients.CountAsync(p => p.IsActive),
                TodayAppointments = await _db.Appointments.CountAsync(a => a.AppointmentDate.Date == today),
                AvailableBeds = await _db.Beds.CountAsync(b => b.Status == Models.BedStatus.Available),
                TotalDoctors = await _db.Doctors.CountAsync(d => d.IsAvailable),
                TodayRevenue = todayBills.Sum(b => b.PaidAmount),
                MonthlyRevenue = monthBills.Sum(b => b.PaidAmount),
                PendingBills = await _db.Bills.CountAsync(b => b.Status == Models.BillStatus.Pending),
                LowStockItems = await _db.InventoryItems.CountAsync(i => i.CurrentStock <= i.MinimumStock),

                RecentAppointments = await _db.Appointments
                    .Include(a => a.Patient).Include(a => a.Doctor)
                    .OrderByDescending(a => a.AppointmentDate)
                    .Take(5).ToListAsync(),

                RecentPatients = await _db.Patients
                    .OrderByDescending(p => p.RegisteredAt)
                    .Take(5).ToListAsync(),

                ActiveAdmissions = await _db.Admissions
                    .Include(a => a.Patient).Include(a => a.Bed).ThenInclude(b => b!.Ward)
                    .Where(a => a.DischargeDate == null)
                    .Take(5).ToListAsync(),

                // ✅ GroupBy runs in C# memory — NOT sent to SQL
                AppointmentsByStatus = appointmentStatuses
                    .GroupBy(s => s.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
            };

            // Revenue last 6 months
            for (int i = 5; i >= 0; i--)
            {
                var m = today.AddMonths(-i);
                var key = m.ToString("MMM yyyy");
                var rev = bills.Where(b => b.BillDate.Year == m.Year && b.BillDate.Month == m.Month).Sum(b => b.PaidAmount);
                vm.RevenueByMonth[key] = rev;
            }

            return View(vm);
        }
    }
}
