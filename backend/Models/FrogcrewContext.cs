using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

public partial class FrogcrewContext : DbContext
{
    public FrogcrewContext()
    {
    }

    public FrogcrewContext(DbContextOptions<FrogcrewContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Availability> Availabilities { get; set; }

    public virtual DbSet<CrewedUser> CrewedUsers { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserQualifiedPosition> UserQualifiedPositions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySQL("Server=localhost;Database=frogcrew;User=root;Password=password;Port=3306;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Availability>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.GameId }).HasName("PRIMARY");

            entity.ToTable("Availability");

            entity.HasIndex(e => e.GameId, "gameId");

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.GameId).HasColumnName("gameId");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .HasColumnName("comment");
            entity.Property(e => e.Open).HasColumnName("open");

            entity.HasOne(d => d.Game).WithMany(p => p.Availabilities)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("Availability_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.Availabilities)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Availability_ibfk_1");
        });

        modelBuilder.Entity<CrewedUser>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.GameId }).HasName("PRIMARY");

            entity.ToTable("CrewedUser");

            entity.HasIndex(e => e.GameId, "gameId");

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.GameId).HasColumnName("gameId");
            entity.Property(e => e.ArrivalTime)
                .HasColumnType("datetime")
                .HasColumnName("arrivalTime");
            entity.Property(e => e.CrewedPosition)
                .HasMaxLength(255)
                .HasColumnName("crewedPosition");

            entity.HasOne(d => d.Game).WithMany(p => p.CrewedUsers)
                .HasForeignKey(d => d.GameId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CrewedUser_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.CrewedUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CrewedUser_ibfk_1");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Game");

            entity.HasIndex(e => e.ScheduleId, "scheduleId");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GameDate)
                .HasColumnType("date")
                .HasColumnName("gameDate");
            entity.Property(e => e.GameStart)
                .HasColumnType("time")
                .HasColumnName("gameStart");
            entity.Property(e => e.IsFinalized).HasColumnName("isFinalized");
            entity.Property(e => e.Opponent)
                .HasMaxLength(255)
                .HasColumnName("opponent");
            entity.Property(e => e.ScheduleId).HasColumnName("scheduleId");
            entity.Property(e => e.Venue)
                .HasMaxLength(255)
                .HasColumnName("venue");

            entity.HasOne(d => d.Schedule).WithMany(p => p.Games)
                .HasForeignKey(d => d.ScheduleId)
                .HasConstraintName("Game_ibfk_1");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Notification");

            entity.HasIndex(e => e.UserId, "userId");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasMaxLength(255)
                .HasColumnName("content");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Notification_ibfk_1");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Schedule");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Season)
                .HasMaxLength(255)
                .HasColumnName("season");
            entity.Property(e => e.Sport)
                .HasMaxLength(255)
                .HasColumnName("sport");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("User");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .HasColumnName("firstName");
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .HasColumnName("lastName");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.PayRate)
                .HasMaxLength(100)
                .HasColumnName("payRate");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(10)
                .HasColumnName("phoneNumber");
            entity.Property(e => e.Role)
                .HasMaxLength(100)
                .HasColumnName("role");
        });

        modelBuilder.Entity<UserQualifiedPosition>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.Position }).HasName("PRIMARY");

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.Position).HasColumnName("position");

            entity.HasOne(d => d.User).WithMany(p => p.UserQualifiedPositions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserQualifiedPositions_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
