using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    // ─── Identity ───────────────────────────────────────────────────────────────
    public class ApplicationUser : IdentityUser
    {
        [Required, MaxLength(100)] public string FullName { get; set; } = "";
        public string Role { get; set; } = "Staff";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ─── Patient ────────────────────────────────────────────────────────────────
    public class Patient
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string FirstName { get; set; } = "";
        [Required, MaxLength(100)] public string LastName { get; set; } = "";
        public string FullName => $"{FirstName} {LastName}";
        [Required] public DateTime DateOfBirth { get; set; }
        public int Age => (int)((DateTime.Today - DateOfBirth).TotalDays / 365.25);
        [Required, MaxLength(10)] public string Gender { get; set; } = "";
        [MaxLength(20)] public string? BloodGroup { get; set; }
        [Required, Phone] public string Phone { get; set; } = "";
        [EmailAddress] public string? Email { get; set; }
        [MaxLength(300)] public string? Address { get; set; }
        [MaxLength(100)] public string? EmergencyContact { get; set; }
        [MaxLength(20)] public string? EmergencyPhone { get; set; }
        public string? MedicalHistory { get; set; }
        public string? Allergies { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string PatientCode => $"P{Id:D5}";

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Bill> Bills { get; set; } = new List<Bill>();
        public ICollection<Admission> Admissions { get; set; } = new List<Admission>();
    }

    // ─── Department ─────────────────────────────────────────────────────────────
    public class Department
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string Name { get; set; } = "";
        [MaxLength(300)] public string? Description { get; set; }
        [MaxLength(10)] public string? Icon { get; set; } = "🏥";
        public bool IsActive { get; set; } = true;

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        public ICollection<Ward> Wards { get; set; } = new List<Ward>();
    }

    // ─── Doctor ─────────────────────────────────────────────────────────────────
    public class Doctor
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string FirstName { get; set; } = "";
        [Required, MaxLength(100)] public string LastName { get; set; } = "";
        public string FullName => $"Dr. {FirstName} {LastName}";
        [Required, MaxLength(100)] public string Specialization { get; set; } = "";
        [Required] public int DepartmentId { get; set; }
        public Department? Department { get; set; }
        [Required, Phone] public string Phone { get; set; } = "";
        [EmailAddress] public string? Email { get; set; }
        [MaxLength(20)] public string? LicenseNumber { get; set; }
        [MaxLength(100)] public string? Qualification { get; set; }
        public int ExperienceYears { get; set; }
        public decimal ConsultationFee { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public string DoctorCode => $"D{Id:D4}";

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }

    // ─── Schedule ────────────────────────────────────────────────────────────────
    public class Schedule
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        [Required] public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxAppointments { get; set; } = 20;
        public bool IsActive { get; set; } = true;
    }

    // ─── Appointment ─────────────────────────────────────────────────────────────
    public enum AppointmentStatus { Scheduled, Confirmed, Completed, Cancelled, NoShow }
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        [Required] public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        [MaxLength(300)] public string? Reason { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
        public string? Notes { get; set; }
        public string? Diagnosis { get; set; }
        public string? Prescription { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string AppointmentCode => $"A{Id:D6}";
    }

    // ─── Ward & Bed ──────────────────────────────────────────────────────────────
    public enum BedStatus { Available, Occupied, Maintenance, Reserved }
    public class Ward
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string Name { get; set; } = "";
        [MaxLength(10)] public string? WardNumber { get; set; }
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }
        [MaxLength(50)] public string WardType { get; set; } = "General";
        public int TotalBeds { get; set; }
        public int AvailableBeds => Beds.Count(b => b.Status == BedStatus.Available);
        public bool IsActive { get; set; } = true;

        public ICollection<Bed> Beds { get; set; } = new List<Bed>();
    }

    public class Bed
    {
        public int Id { get; set; }
        [Required, MaxLength(20)] public string BedNumber { get; set; } = "";
        public int WardId { get; set; }
        public Ward? Ward { get; set; }
        public BedStatus Status { get; set; } = BedStatus.Available;
        public decimal DailyRate { get; set; }

        public ICollection<Admission> Admissions { get; set; } = new List<Admission>();
    }

    // ─── Admission ──────────────────────────────────────────────────────────────
    public class Admission
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        public int BedId { get; set; }
        public Bed? Bed { get; set; }
        public int AdmittingDoctorId { get; set; }
        public Doctor? AdmittingDoctor { get; set; }
        [Required] public DateTime AdmissionDate { get; set; }
        public DateTime? DischargeDate { get; set; }
        [MaxLength(300)] public string? AdmissionReason { get; set; }
        public string? DischargeNotes { get; set; }
        public bool IsActive => DischargeDate == null;
        public int DaysAdmitted => (int)((DischargeDate ?? DateTime.Today) - AdmissionDate).TotalDays;
        public string AdmissionCode => $"ADM{Id:D5}";
    }

    // ─── Staff ──────────────────────────────────────────────────────────────────
    public class Staff
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string FirstName { get; set; } = "";
        [Required, MaxLength(100)] public string LastName { get; set; } = "";
        public string FullName => $"{FirstName} {LastName}";
        [Required, MaxLength(100)] public string Role { get; set; } = "";
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        [Required, Phone] public string Phone { get; set; } = "";
        [EmailAddress] public string? Email { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string StaffCode => $"S{Id:D4}";
    }

    // ─── Billing ────────────────────────────────────────────────────────────────
    public enum BillStatus { Pending, Partial, Paid, Cancelled }
    public class Bill
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }
        public int? AdmissionId { get; set; }
        public Admission? Admission { get; set; }
        public DateTime BillDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public BillStatus Status { get; set; } = BillStatus.Pending;
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxPercent { get; set; } = 5;
        public decimal TaxAmount => (SubTotal - Discount) * TaxPercent / 100;
        public decimal TotalAmount => SubTotal - Discount + TaxAmount;
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount => TotalAmount - PaidAmount;
        public string? Notes { get; set; }
        public string BillCode => $"B{Id:D6}";

        public ICollection<BillItem> Items { get; set; } = new List<BillItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public class BillItem
    {
        public int Id { get; set; }
        public int BillId { get; set; }
        public Bill? Bill { get; set; }
        [Required, MaxLength(200)] public string Description { get; set; } = "";
        [MaxLength(50)] public string Category { get; set; } = "General";
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
    }

    public class Payment
    {
        public int Id { get; set; }
        public int BillId { get; set; }
        public Bill? Bill { get; set; }
        public decimal Amount { get; set; }
        [MaxLength(50)] public string PaymentMethod { get; set; } = "Cash";
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        [MaxLength(100)] public string? TransactionRef { get; set; }
        public string? Notes { get; set; }
    }

    // ─── Inventory ──────────────────────────────────────────────────────────────
    public class InventoryItem
    {
        public int Id { get; set; }
        [Required, MaxLength(200)] public string Name { get; set; } = "";
        [MaxLength(50)] public string Category { get; set; } = "Medicine";
        [MaxLength(50)] public string? Unit { get; set; }
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; } = 10;
        public decimal UnitCost { get; set; }
        public decimal SellingPrice { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsLowStock => CurrentStock <= MinimumStock;
        public bool IsActive { get; set; } = true;
    }
}
