﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaskVault.DataAccess.Context;

#nullable disable

namespace TaskVault.DataAccess.Migrations
{
    [DbContext(typeof(TaskVaultDevContext))]
    [Migration("20250317230545_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("FileOwner", b =>
                {
                    b.Property<Guid>("FileId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("TEXT");

                    b.HasKey("FileId", "OwnerId");

                    b.HasIndex("OwnerId");

                    b.ToTable("FileOwner");
                });

            modelBuilder.Entity("FileUser", b =>
                {
                    b.Property<Guid>("FilesId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("OwnersId")
                        .HasColumnType("TEXT");

                    b.HasKey("FilesId", "OwnersId");

                    b.HasIndex("OwnersId");

                    b.ToTable("FileUser");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.File", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("FileTypeId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<double>("Size")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("UploadedAt")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("UploaderId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FileTypeId");

                    b.HasIndex("UploaderId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.FileCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("FileCategories");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.FileType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Extension")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("FileTypes");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.Task", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeadlineAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("TEXT");

                    b.Property<int>("StatusId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.HasIndex("StatusId");

                    b.ToTable("Tasks");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.TaskItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<int>("FileCategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FileTypeId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FileCategoryId");

                    b.HasIndex("FileTypeId");

                    b.ToTable("TaskItems");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.TaskStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TaskStatuses");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.TaskSubmission", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Approved")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("SubmittedAt")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SubmittedById")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TaskId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("SubmittedById");

                    b.HasIndex("TaskId");

                    b.ToTable("TaskSubmissions");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.TaskSubmissionTaskItemFile", b =>
                {
                    b.Property<Guid>("TaskSubmissionId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TaskItemId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("FileId")
                        .HasColumnType("TEXT");

                    b.HasKey("TaskSubmissionId", "TaskItemId", "FileId");

                    b.HasIndex("FileId");

                    b.HasIndex("TaskItemId");

                    b.ToTable("TaskSubmissionTaskItemFiles");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("FileOwner", b =>
                {
                    b.HasOne("TaskVault.DataAccess.Entities.File", null)
                        .WithMany()
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("FileUser", b =>
                {
                    b.HasOne("TaskVault.DataAccess.Entities.File", null)
                        .WithMany()
                        .HasForeignKey("FilesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("OwnersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.File", b =>
                {
                    b.HasOne("TaskVault.DataAccess.Entities.FileType", "FileType")
                        .WithMany()
                        .HasForeignKey("FileTypeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.User", "Uploader")
                        .WithMany()
                        .HasForeignKey("UploaderId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("FileType");

                    b.Navigation("Uploader");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.Task", b =>
                {
                    b.HasOne("TaskVault.DataAccess.Entities.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.TaskStatus", "Status")
                        .WithMany()
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Owner");

                    b.Navigation("Status");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.TaskItem", b =>
                {
                    b.HasOne("TaskVault.DataAccess.Entities.FileCategory", "FileCategory")
                        .WithMany()
                        .HasForeignKey("FileCategoryId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.FileType", "FileType")
                        .WithMany()
                        .HasForeignKey("FileTypeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("FileCategory");

                    b.Navigation("FileType");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.TaskSubmission", b =>
                {
                    b.HasOne("TaskVault.DataAccess.Entities.User", "SubmittedByUser")
                        .WithMany()
                        .HasForeignKey("SubmittedById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.Task", "Task")
                        .WithMany()
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SubmittedByUser");

                    b.Navigation("Task");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.TaskSubmissionTaskItemFile", b =>
                {
                    b.HasOne("TaskVault.DataAccess.Entities.File", null)
                        .WithMany()
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.TaskItem", null)
                        .WithMany()
                        .HasForeignKey("TaskItemId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.TaskSubmission", null)
                        .WithMany()
                        .HasForeignKey("TaskSubmissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
