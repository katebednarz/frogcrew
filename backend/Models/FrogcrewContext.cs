﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace backend.Models;

public partial class FrogcrewContext : IdentityDbContext<ApplicationUser,ApplicationRole,int>
{
    public FrogcrewContext()
    {
    }

    public FrogcrewContext(DbContextOptions<FrogcrewContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Availability> Availabilities { get; set; } = null!;

    public virtual DbSet<CrewedUser> CrewedUsers { get; set; } = null!;

    public virtual DbSet<Game> Games { get; set; } = null!;

    public virtual DbSet<Notification> Notifications { get; set; } = null!;

    public virtual DbSet<Schedule> Schedules { get; set; } = null!;

    public virtual DbSet<Invitation> Invitations { get; set; } = null!;
    
    public virtual DbSet<Position> Positions { get; set; }
    
    public virtual DbSet<TradeBoard> TradeBoards { get; set; }
    
    public virtual DbSet<UserQualifiedPosition> UserQualifiedPositions { get; set; } = null!;

    //public virtual DbSet<User> Users { get; set; } = null!;
    
    //public virtual DbSet<ApplicationUser> Users { get; set; } = null!;


    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<ApplicationUser>()
            .ToTable("User");
        
        modelBuilder.Entity<Availability>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.GameId }).HasName("PRIMARY");

            entity.ToTable("Availability");

            entity.HasIndex(e => e.GameId, "AvailabilityIndex");

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.GameId).HasColumnName("gameId");
            entity.Property(e => e.Comments)
                .HasMaxLength(255)
                .HasColumnName("comment");
            entity.Property(e => e.Available).HasColumnName("open");

            entity.HasOne(d => d.Game).WithMany(p => p.Availabilities)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("Availability_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.Availabilities)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Availability_ibfk_1");
        });

        modelBuilder.Entity<CrewedUser>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.GameId, e.PositionId }).HasName("PRIMARY");

            entity.ToTable("CrewedUser");

            entity.HasIndex(e => e.GameId, "CrewedUserIndex");

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.GameId).HasColumnName("gameId");
            entity.Property(e => e.PositionId).HasColumnName("positionId");
            entity.Property(e => e.ArrivalTime)
                .HasColumnType("time")
                .HasColumnName("arrivalTime")
                .HasConversion(new TimeOnlyConverter());
            
            entity.HasOne(d => d.CrewedPositionNavigation).WithMany(p => p.CrewedUsers)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CrewedUser_ibfk_3");

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

            entity.HasIndex(e => e.ScheduleId, "GameIndex");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GameDate)
                .HasColumnType("date")
                .HasColumnName("gameDate")
                .HasConversion(new DateOnlyConverter());
            entity.Property(e => e.GameStart)
                .HasColumnType("time")
                .HasColumnName("gameStart")
                .HasConversion(new TimeOnlyConverter());
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

        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasKey(e => e.Token);
            entity.Property(e => e.Token)
                .HasColumnName("Token")
                .HasMaxLength(450)
                .IsRequired();
        });
        
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Notification");

            entity.HasIndex(e => e.UserId, "NotificationIndex");

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

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.PositionId).HasName("PRIMARY");

            entity.ToTable("Position");

            entity.Property(e => e.PositionName)
                .HasMaxLength(255)
                .HasColumnName("PositionName");
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

        modelBuilder.Entity<TradeBoard>(entity =>
        {
            entity.HasKey(e => new { e.DropperId, e.GameId });

            entity.ToTable("TradeBoard");

            entity.Property(e => e.DropperId).HasColumnName("DropperID");
            entity.Property(e => e.Position).HasColumnName("PositionId");
            entity.Property(e => e.ReceiverId).HasColumnName("ReceiverID");
            entity.Property(e => e.Status).HasMaxLength(255);
        });

        modelBuilder.Entity<UserQualifiedPosition>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.PositionId }).HasName("PRIMARY");

            entity.ToTable("UserQualifiedPositions");

            entity.HasIndex(e => e.PositionId, "UserQualifiedPositionIndex");

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.PositionId).HasColumnName("positionId");

            entity.HasOne(d => d.Position).WithMany(p => p.UserQualifiedPositions)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserQualifiedPosition_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.UserQualifiedPositions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserQualifiedPosition_ibfk_1");
        });
        
        
        
        

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
