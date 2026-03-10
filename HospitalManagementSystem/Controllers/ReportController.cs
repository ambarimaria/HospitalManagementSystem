using HospitalManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly AppDbContext _db;
        public ReportController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var yearStart = new DateTime(today.Year, 1, 1);

            var allBills = await _db.Bills.Include(b => b.Items).ToListAsync();
            var appointments = await _db.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d!.Department)
                .ToListAsync();

            // Monthly revenue for current year
            var monthlyRevenue = new Dictionary<string, decimal>();
            for (int m = 1; m <= 12; m++)
            {
                var label = new DateTime(today.Year, m, 1).ToString("MMM");
                monthlyRevenue[label] = allBills
                    .Where(b => b.BillDate.Year == today.Year && b.BillDate.Month == m)
                    .Sum(b => b.PaidAmount);
            }

            // Department-wise appointments
            var deptAppts = appointments
                .Where(a => a.Doctor?.Department != null)
                .GroupBy(a => a.Doctor!.Department!.Name)
                .ToDictionary(g => g.Key, g => g.Count());

            // Status breakdown
            var statusBreakdown = appointments
                .GroupBy(a => a.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.DeptAppointments = deptAppts;
            ViewBag.StatusBreakdown = statusBreakdown;
            ViewBag.TotalRevenue = allBills.Sum(b => b.PaidAmount);
            ViewBag.TotalBilled = allBills.Sum(b => b.SubTotal);
            ViewBag.TotalPatients = await _db.Patients.CountAsync(p => p.IsActive);
            ViewBag.TotalAppointments = appointments.Count;
            ViewBag.CompletedAppointments = appointments.Count(a => a.Status == Models.AppointmentStatus.Completed);
            ViewBag.TodayDate = today;

            return View();
        }
    }
}
