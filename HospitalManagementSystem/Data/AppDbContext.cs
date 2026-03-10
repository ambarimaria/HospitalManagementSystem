using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Schedule> Schedules => Set<Schedule>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Ward> Wards => Set<Ward>();
        public DbSet<Bed> Beds => Set<Bed>();
        public DbSet<Admission> Admissions => Set<Admission>();
        public DbSet<Staff> Staff => Set<Staff>();
        public DbSet<Bill> Bills => Set<Bill>();
        public DbSet<BillItem> BillItems => Set<BillItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Precision for decimals
            builder.Entity<Doctor>().Property(d => d.ConsultationFee).HasPrecision(18, 2);
            builder.Entity<Bed>().Property(b => b.DailyRate).HasPrecision(18, 2);
            builder.Entity<Bill>().Property(b => b.SubTotal).HasPrecision(18, 2);
            builder.Entity<Bill>().Property(b => b.Discount).HasPrecision(18, 2);
            builder.Entity<Bill>().Property(b => b.TaxPercent).HasPrecision(18, 2);
            builder.Entity<Bill>().Property(b => b.PaidAmount).HasPrecision(18, 2);
            builder.Entity<BillItem>().Property(b => b.UnitPrice).HasPrecision(18, 2);
            builder.Entity<Payment>().Property(p => p.Amount).HasPrecision(18, 2);
            builder.Entity<InventoryItem>().Property(i => i.UnitCost).HasPrecision(18, 2);
            builder.Entity<InventoryItem>().Property(i => i.SellingPrice).HasPrecision(18, 2);

            // Ignore computed properties
            builder.Entity<Bill>().Ignore(b => b.TaxAmount).Ignore(b => b.TotalAmount).Ignore(b => b.BalanceAmount);
            builder.Entity<Ward>().Ignore(w => w.AvailableBeds);
            builder.Entity<Patient>().Ignore(p => p.Age).Ignore(p => p.FullName).Ignore(p => p.PatientCode);
            builder.Entity<Doctor>().Ignore(d => d.FullName).Ignore(d => d.DoctorCode);
            builder.Entity<Admission>().Ignore(a => a.IsActive).Ignore(a => a.DaysAdmitted).Ignore(a => a.AdmissionCode);
            builder.Entity<Bill>().Ignore(b => b.BillCode);
            builder.Entity<BillItem>().Ignore(b => b.Total);
            builder.Entity<Staff>().Ignore(s => s.FullName).Ignore(s => s.StaffCode);
        }
    }
}
