using System.ComponentModel.DataAnnotations;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int TodayAppointments { get; set; }
        public int AvailableBeds { get; set; }
        public int TotalDoctors { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int PendingBills { get; set; }
        public int LowStockItems { get; set; }
        public List<Appointment> RecentAppointments { get; set; } = new();
        public List<Patient> RecentPatients { get; set; } = new();
        public List<Admission> ActiveAdmissions { get; set; } = new();
        public Dictionary<string, int> AppointmentsByStatus { get; set; } = new();
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
    }

    public class LoginViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
        public bool RememberMe { get; set; }
    }

    public class PatientFormViewModel
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string FirstName { get; set; } = "";
        [Required, MaxLength(100)] public string LastName { get; set; } = "";
        [Required, Display(Name = "Date of Birth")] public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-30);
        [Required] public string Gender { get; set; } = "";
        [Display(Name = "Blood Group")] public string? BloodGroup { get; set; }
        [Required, Phone] public string Phone { get; set; } = "";
        [EmailAddress] public string? Email { get; set; }
        public string? Address { get; set; }
        [Display(Name = "Emergency Contact")] public string? EmergencyContact { get; set; }
        [Display(Name = "Emergency Phone")] public string? EmergencyPhone { get; set; }
        [Display(Name = "Medical History")] public string? MedicalHistory { get; set; }
        public string? Allergies { get; set; }
    }

    public class DoctorFormViewModel
    {
        public int Id { get; set; }
        [Required] public string FirstName { get; set; } = "";
        [Required] public string LastName { get; set; } = "";
        [Required] public string Specialization { get; set; } = "";
        [Required, Display(Name = "Department")] public int DepartmentId { get; set; }
        [Required, Phone] public string Phone { get; set; } = "";
        [EmailAddress] public string? Email { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Qualification { get; set; }
        [Range(0, 50)] public int ExperienceYears { get; set; }
        [Range(0, 10000)] public decimal ConsultationFee { get; set; }
        public bool IsAvailable { get; set; } = true;
        public List<Department> Departments { get; set; } = new();
    }

    public class AppointmentFormViewModel
    {
        public int Id { get; set; }
        [Required, Display(Name = "Patient")] public int PatientId { get; set; }
        [Required, Display(Name = "Doctor")] public int DoctorId { get; set; }
        [Required, Display(Name = "Date")] public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);
        [Required, Display(Name = "Time")] public string AppointmentTime { get; set; } = "09:00";
        [MaxLength(300)] public string? Reason { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
        public string? Notes { get; set; }
        public string? Diagnosis { get; set; }
        public string? Prescription { get; set; }
        public List<Patient> Patients { get; set; } = new();
        public List<Doctor> Doctors { get; set; } = new();
    }

    public class BillFormViewModel
    {
        public int Id { get; set; }
        [Required, Display(Name = "Patient")] public int PatientId { get; set; }
        public int? AppointmentId { get; set; }
        public int? AdmissionId { get; set; }
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(15);
        public decimal Discount { get; set; }
        public decimal TaxPercent { get; set; } = 5;
        public string? Notes { get; set; }
        public List<BillItemViewModel> Items { get; set; } = new();
        public List<Patient> Patients { get; set; } = new();
    }

    public class BillItemViewModel
    {
        public string Description { get; set; } = "";
        public string Category { get; set; } = "General";
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
    }

    public class AdmissionFormViewModel
    {
        public int Id { get; set; }
        [Required] public int PatientId { get; set; }
        [Required] public int BedId { get; set; }
        [Required] public int AdmittingDoctorId { get; set; }
        [Required] public DateTime AdmissionDate { get; set; } = DateTime.Today;
        public string? AdmissionReason { get; set; }
        public List<Patient> Patients { get; set; } = new();
        public List<Doctor> Doctors { get; set; } = new();
        public List<Ward> Wards { get; set; } = new();
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrev => Page > 1;
        public bool HasNext => Page < TotalPages;
        public string? Search { get; set; }
    }
}
