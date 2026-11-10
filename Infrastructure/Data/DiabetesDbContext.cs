using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class DiabetesDbContext(DbContextOptions<DiabetesDbContext> options) : IdentityDbContext<User, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorApprovalRequest> DoctorApprovalRequests => Set<DoctorApprovalRequest>();
    public DbSet<DoctorPatientRelationship> DoctorPatientRelationships => Set<DoctorPatientRelationship>();
    public DbSet<GlucoseReading> GlucoseReadings => Set<GlucoseReading>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<MedicationSchedule> MedicationSchedules => Set<MedicationSchedule>();
    public DbSet<MedicationLog> MedicationLogs => Set<MedicationLog>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<DoctorNote> DoctorNotes => Set<DoctorNote>();
    public DbSet<HealthReport> HealthReports => Set<HealthReport>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.UserName).HasMaxLength(256);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.AccountStatus).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Patient>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId).IsUnique();
            e.HasOne(x => x.User).WithOne(x => x.Patient).HasForeignKey<Patient>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Doctor>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId).IsUnique();
            e.HasIndex(x => x.LicenseNumber).IsUnique();
            e.Property(x => x.LicenseNumber).HasMaxLength(100).IsRequired();
            e.Property(x => x.Specialization).HasMaxLength(150);
            e.Property(x => x.ClinicName).HasMaxLength(200);
            e.HasOne(x => x.User).WithOne(x => x.Doctor).HasForeignKey<Doctor>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DoctorApprovalRequest>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Doctor).WithMany(x => x.ApprovalRequests).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ReviewedByAdmin).WithMany().HasForeignKey(x => x.ReviewedByAdminId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DoctorPatientRelationship>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.DoctorId, x.PatientId }).IsUnique();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Doctor).WithMany(x => x.PatientRelationships).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Patient).WithMany(x => x.DoctorRelationships).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GlucoseReading>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.GlucoseValue).HasPrecision(6, 2);
            e.Property(x => x.Unit).HasConversion<string>().HasMaxLength(10);
            e.Property(x => x.MeasurementContext).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasIndex(x => new { x.PatientId, x.MeasurementDateTime });
            e.HasOne(x => x.Patient).WithMany(x => x.GlucoseReadings).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Medication>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Dosage).HasMaxLength(100);
            e.Property(x => x.Instructions).HasMaxLength(500);
            e.HasOne(x => x.Patient).WithMany(x => x.Medications).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MedicationSchedule>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Frequency).HasMaxLength(50).IsRequired();
            e.HasOne(x => x.Medication).WithMany(x => x.Schedules).HasForeignKey(x => x.MedicationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MedicationLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Medication).WithMany(x => x.Logs).HasForeignKey(x => x.MedicationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Alert>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.AlertType).HasConversion<string>().HasMaxLength(40);
            e.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Message).HasMaxLength(500).IsRequired();
            e.HasOne(x => x.Patient).WithMany(x => x.Alerts).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.GlucoseReading).WithMany(x => x.Alerts).HasForeignKey(x => x.GlucoseReadingId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Body).HasMaxLength(500).IsRequired();
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
            e.HasOne(x => x.User).WithMany(x => x.Notifications).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Alert).WithMany(x => x.Notifications).HasForeignKey(x => x.AlertId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DoctorNote>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Note).HasMaxLength(2000).IsRequired();
            e.HasOne(x => x.Doctor).WithMany(x => x.Notes).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Patient).WithMany(x => x.DoctorNotes).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HealthReport>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.AverageGlucose).HasPrecision(6, 2);
            e.Property(x => x.MinimumGlucose).HasPrecision(6, 2);
            e.Property(x => x.MaximumGlucose).HasPrecision(6, 2);
            e.Property(x => x.InRangePercentage).HasPrecision(5, 2);
            e.Property(x => x.HighPercentage).HasPrecision(5, 2);
            e.Property(x => x.LowPercentage).HasPrecision(5, 2);
            e.Property(x => x.StabilityScore).HasPrecision(5, 2);
            e.Property(x => x.Trend).HasConversion<string>().HasMaxLength(30);
            e.HasOne(x => x.Patient).WithMany(x => x.HealthReports).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeviceToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Token).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.Token).IsUnique();
            e.HasOne(x => x.User).WithMany(x => x.DeviceTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Action).HasMaxLength(100).IsRequired();
            e.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.IpAddress).HasMaxLength(45);
            e.HasIndex(x => new { x.EntityType, x.EntityId });
            e.HasOne(x => x.User).WithMany(x => x.AuditLogs).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Token).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.Token).IsUnique();
            e.HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
