using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await db.Database.MigrateAsync();

            // Roles
            string[] roles = { "Admin", "Doctor", "Nurse", "Staff", "Receptionist" };
            foreach (var r in roles)
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));

            // Admin user
            if (await userMgr.FindByEmailAsync("admin@hospital.com") == null)
            {
                var admin = new ApplicationUser { UserName = "admin@hospital.com", Email = "admin@hospital.com", FullName = "System Administrator", Role = "Admin", EmailConfirmed = true };
                await userMgr.CreateAsync(admin, "Admin@123!");
                await userMgr.AddToRoleAsync(admin, "Admin");
            }

            if (await db.Departments.AnyAsync()) return;

            // Departments
            var depts = new[]
            {
                new Department { Name = "Cardiology", Description = "Heart & Cardiovascular Care", Icon = "❤️" },
                new Department { Name = "Neurology", Description = "Brain & Nervous System", Icon = "🧠" },
                new Department { Name = "Orthopedics", Description = "Bones, Joints & Muscles", Icon = "🦴" },
                new Department { Name = "Pediatrics", Description = "Child Healthcare", Icon = "👶" },
                new Department { Name = "Oncology", Description = "Cancer Treatment", Icon = "🎗️" },
                new Department { Name = "Emergency", Description = "Emergency & Trauma Care", Icon = "🚨" },
                new Department { Name = "Radiology", Description = "Imaging & Diagnostics", Icon = "🔬" },
                new Department { Name = "Gynecology", Description = "Women's Health", Icon = "🌸" },
            };
            db.Departments.AddRange(depts);
            await db.SaveChangesAsync();

            // Doctors
            var doctors = new[]
            {
                new Doctor { FirstName = "Sarah", LastName = "Mitchell", Specialization = "Cardiologist", DepartmentId = depts[0].Id, Phone = "555-0101", Email = "s.mitchell@hospital.com", LicenseNumber = "LIC001", Qualification = "MD, FACC", ExperienceYears = 15, ConsultationFee = 250 },
                new Doctor { FirstName = "James", LastName = "Rivera", Specialization = "Neurologist", DepartmentId = depts[1].Id, Phone = "555-0102", Email = "j.rivera@hospital.com", LicenseNumber = "LIC002", Qualification = "MD, PhD", ExperienceYears = 12, ConsultationFee = 300 },
                new Doctor { FirstName = "Emily", LastName = "Chen", Specialization = "Orthopedic Surgeon", DepartmentId = depts[2].Id, Phone = "555-0103", Email = "e.chen@hospital.com", LicenseNumber = "LIC003", Qualification = "MD, MS Ortho", ExperienceYears = 10, ConsultationFee = 280 },
                new Doctor { FirstName = "Michael", LastName = "Thompson", Specialization = "Pediatrician", DepartmentId = depts[3].Id, Phone = "555-0104", Email = "m.thompson@hospital.com", LicenseNumber = "LIC004", Qualification = "MD, DCH", ExperienceYears = 8, ConsultationFee = 200 },
                new Doctor { FirstName = "Lisa", LastName = "Patel", Specialization = "Oncologist", DepartmentId = depts[4].Id, Phone = "555-0105", Email = "l.patel@hospital.com", LicenseNumber = "LIC005", Qualification = "MD, DM Onco", ExperienceYears = 14, ConsultationFee = 350 },
                new Doctor { FirstName = "Robert", LastName = "Kim", Specialization = "Emergency Physician", DepartmentId = depts[5].Id, Phone = "555-0106", Email = "r.kim@hospital.com", LicenseNumber = "LIC006", Qualification = "MD, FACEP", ExperienceYears = 9, ConsultationFee = 200 },
            };
            db.Doctors.AddRange(doctors);
            await db.SaveChangesAsync();

            // Wards & Beds
            var wardNames = new[] { ("General Ward A", "G-A", "General", depts[5].Id, 10), ("ICU", "ICU", "Intensive", depts[5].Id, 6), ("Cardiology Ward", "C-1", "Specialty", depts[0].Id, 8), ("Pediatric Ward", "P-1", "Pediatric", depts[3].Id, 10) };
            foreach (var (name, num, type, deptId, bedCount) in wardNames)
            {
                var ward = new Ward { Name = name, WardNumber = num, DepartmentId = deptId, WardType = type, TotalBeds = bedCount };
                db.Wards.Add(ward);
                await db.SaveChangesAsync();
                for (int i = 1; i <= bedCount; i++)
                {
                    db.Beds.Add(new Bed { BedNumber = $"{num}-{i:D2}", WardId = ward.Id, Status = i <= 2 ? BedStatus.Occupied : BedStatus.Available, DailyRate = type == "Intensive" ? 800 : type == "Specialty" ? 500 : 300 });
                }
            }
            await db.SaveChangesAsync();

            // Patients
            var patients = new[]
            {
                new Patient { FirstName = "John", LastName = "Anderson", DateOfBirth = new DateTime(1975, 3, 15), Gender = "Male", BloodGroup = "A+", Phone = "555-1001", Email = "j.anderson@email.com", Address = "123 Main St, Springfield" },
                new Patient { FirstName = "Maria", LastName = "Garcia", DateOfBirth = new DateTime(1988, 7, 22), Gender = "Female", BloodGroup = "O+", Phone = "555-1002", Email = "m.garcia@email.com", Address = "456 Oak Ave, Riverside" },
                new Patient { FirstName = "David", LastName = "Wilson", DateOfBirth = new DateTime(1962, 11, 8), Gender = "Male", BloodGroup = "B-", Phone = "555-1003", MedicalHistory = "Hypertension, Diabetes Type 2" },
                new Patient { FirstName = "Jennifer", LastName = "Brown", DateOfBirth = new DateTime(1995, 5, 30), Gender = "Female", BloodGroup = "AB+", Phone = "555-1004", Allergies = "Penicillin" },
                new Patient { FirstName = "William", LastName = "Taylor", DateOfBirth = new DateTime(1958, 9, 12), Gender = "Male", BloodGroup = "O-", Phone = "555-1005", MedicalHistory = "Coronary Artery Disease" },
                new Patient { FirstName = "Susan", LastName = "Martinez", DateOfBirth = new DateTime(1980, 1, 25), Gender = "Female", BloodGroup = "A-", Phone = "555-1006" },
            };
            db.Patients.AddRange(patients);
            await db.SaveChangesAsync();

            // Appointments
            var today = DateTime.Today;
            var appointments = new[]
            {
                new Appointment { PatientId = patients[0].Id, DoctorId = doctors[0].Id, AppointmentDate = today, AppointmentTime = new TimeSpan(9, 0, 0), Reason = "Chest pain follow-up", Status = AppointmentStatus.Confirmed },
                new Appointment { PatientId = patients[1].Id, DoctorId = doctors[1].Id, AppointmentDate = today, AppointmentTime = new TimeSpan(10, 30, 0), Reason = "Migraine treatment", Status = AppointmentStatus.Scheduled },
                new Appointment { PatientId = patients[2].Id, DoctorId = doctors[0].Id, AppointmentDate = today.AddDays(1), AppointmentTime = new TimeSpan(11, 0, 0), Reason = "Hypertension checkup", Status = AppointmentStatus.Scheduled },
                new Appointment { PatientId = patients[3].Id, DoctorId = doctors[2].Id, AppointmentDate = today.AddDays(-2), AppointmentTime = new TimeSpan(14, 0, 0), Reason = "Knee pain", Status = AppointmentStatus.Completed, Diagnosis = "Mild osteoarthritis", Prescription = "Ibuprofen 400mg TID, Physiotherapy" },
                new Appointment { PatientId = patients[4].Id, DoctorId = doctors[0].Id, AppointmentDate = today.AddDays(-1), AppointmentTime = new TimeSpan(9, 30, 0), Reason = "Cardiac review", Status = AppointmentStatus.Completed, Diagnosis = "Stable CAD" },
            };
            db.Appointments.AddRange(appointments);
            await db.SaveChangesAsync();

            // Inventory
            var items = new[]
            {
                new InventoryItem { Name = "Paracetamol 500mg", Category = "Medicine", Unit = "Tablets", CurrentStock = 500, MinimumStock = 100, UnitCost = 0.1m, SellingPrice = 0.25m },
                new InventoryItem { Name = "Amoxicillin 250mg", Category = "Medicine", Unit = "Capsules", CurrentStock = 200, MinimumStock = 50, UnitCost = 0.5m, SellingPrice = 1.2m },
                new InventoryItem { Name = "Surgical Gloves (L)", Category = "Supplies", Unit = "Pairs", CurrentStock = 300, MinimumStock = 100, UnitCost = 0.8m, SellingPrice = 2.0m },
                new InventoryItem { Name = "Syringe 5ml", Category = "Supplies", Unit = "Pieces", CurrentStock = 15, MinimumStock = 50, UnitCost = 0.3m, SellingPrice = 0.8m },
                new InventoryItem { Name = "Blood Pressure Monitor", Category = "Equipment", Unit = "Units", CurrentStock = 8, MinimumStock = 3, UnitCost = 120m, SellingPrice = 180m },
                new InventoryItem { Name = "Bandage Roll", Category = "Supplies", Unit = "Rolls", CurrentStock = 150, MinimumStock = 40, UnitCost = 1.5m, SellingPrice = 4.0m },
            };
            db.InventoryItems.AddRange(items);

            // Bills
            var bill1 = new Bill
            {
                PatientId = patients[3].Id, AppointmentId = appointments[3].Id,
                BillDate = today.AddDays(-2), DueDate = today.AddDays(13),
                Status = BillStatus.Paid, SubTotal = 450, Discount = 0, PaidAmount = 472.5m,
                Items = new List<BillItem>
                {
                    new BillItem { Description = "Consultation Fee - Dr. Chen", Category = "Consultation", Quantity = 1, UnitPrice = 280 },
                    new BillItem { Description = "X-Ray Knee", Category = "Radiology", Quantity = 1, UnitPrice = 120 },
                    new BillItem { Description = "Ibuprofen 400mg (30 tablets)", Category = "Medicine", Quantity = 1, UnitPrice = 50 },
                }
            };
            db.Bills.Add(bill1);
            await db.SaveChangesAsync();

            db.Payments.Add(new Payment { BillId = bill1.Id, Amount = 472.5m, PaymentMethod = "Card", PaymentDate = today.AddDays(-2), TransactionRef = "TXN-001234" });
            await db.SaveChangesAsync();

            // Staff
            var staff = new[]
            {
                new Staff { FirstName = "Alice", LastName = "Johnson", Role = "Head Nurse", DepartmentId = depts[5].Id, Phone = "555-2001" },
                new Staff { FirstName = "Bob", LastName = "Smith", Role = "Lab Technician", DepartmentId = depts[6].Id, Phone = "555-2002" },
                new Staff { FirstName = "Carol", LastName = "White", Role = "Receptionist", Phone = "555-2003" },
                new Staff { FirstName = "David", LastName = "Lee", Role = "Pharmacist", Phone = "555-2004" },
            };
            db.Staff.AddRange(staff);
            await db.SaveChangesAsync();
        }
    }
}
