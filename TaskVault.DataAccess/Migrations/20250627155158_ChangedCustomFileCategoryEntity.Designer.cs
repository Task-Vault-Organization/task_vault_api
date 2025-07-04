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
    [Migration("20250627155158_ChangedCustomFileCategoryEntity")]
    partial class ChangedCustomFileCategoryEntity
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

            modelBuilder.Entity("TaskUsers", b =>
                {
                    b.Property<Guid>("TaskId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("TaskId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("TaskUsers");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.CustomFileCategory", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("CustomFileCategories");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.DirectoryEntry", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("DirectoryId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("FileId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Index")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserId", "DirectoryId", "FileId");

                    b.HasIndex("DirectoryId");

                    b.HasIndex("FileId");

                    b.ToTable("DirectoryEntries");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.EmailConfirmationRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("CodeToVerify")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<bool>("Confirmed")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("EmailConfirmationRequests");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.File", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("FileTypeId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("HistoryJson")
                        .HasMaxLength(100000)
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDirectory")
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

            modelBuilder.Entity("TaskVault.DataAccess.Entities.FileShareRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("FileId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("FromId")
                        .HasColumnType("TEXT");

                    b.Property<int>("StatusId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("ToId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FileId");

                    b.HasIndex("FromId");

                    b.HasIndex("StatusId");

                    b.HasIndex("ToId");

                    b.ToTable("FileShareRequests");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.FileShareRequestStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("FileShareRequestStatuses");
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

            modelBuilder.Entity("TaskVault.DataAccess.Entities.Notification", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ContentJson")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("NotificationStatusId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("NotificationTypeId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("ToId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NotificationStatusId");

                    b.HasIndex("NotificationTypeId");

                    b.HasIndex("ToId");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.NotificationStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("NotificationStatuses");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.NotificationType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("NotificationTypes");
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

                    b.Property<Guid>("TaskId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FileCategoryId");

                    b.HasIndex("FileTypeId");

                    b.HasIndex("TaskId");

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

                    b.Property<bool?>("Approved")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("SubmittedAt")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SubmittedById")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TaskId")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("TaskId1")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("SubmittedById");

                    b.HasIndex("TaskId");

                    b.HasIndex("TaskId1");

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

                    b.Property<bool?>("AiApproved")
                        .HasColumnType("INTEGER");

                    b.HasKey("TaskSubmissionId", "TaskItemId", "FileId");

                    b.HasIndex("FileId");

                    b.HasIndex("TaskItemId");

                    b.ToTable("TaskSubmissionTaskItemFiles");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.TaskSubmissionTaskItemFileComment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("CommentHtml")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("TEXT");

                    b.Property<Guid>("FromUserId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TaskSubmissionId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("TaskSubmissionTaskItemFileId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FromUserId");

                    b.HasIndex("TaskSubmissionId", "TaskSubmissionTaskItemFileId", "FromUserId");

                    b.ToTable("TaskSubmissionTaskItemFileComments");
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

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("GoogleId")
                        .HasColumnType("TEXT");

                    b.Property<string>("GoogleProfilePhotoUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("ProfilePhotoId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("RootDirectoryId")
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

            modelBuilder.Entity("TaskUsers", b =>
                {
                    b.HasOne("TaskVault.DataAccess.Entities.Task", null)
                        .WithMany()
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.CustomFileCategory", b =>
                {
                    b.HasOne("TaskVault.DataAccess.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.DirectoryEntry", b =>
                {
                    b.HasOne("TaskVault.DataAccess.Entities.File", "Directory")
                        .WithMany("AsDirectoryEntries")
                        .HasForeignKey("DirectoryId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.File", "File")
                        .WithMany("AsFileEntries")
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.User", "User")
                        .WithMany("DirectoryEntries")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Directory");

                    b.Navigation("File");

                    b.Navigation("User");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.EmailConfirmationRequest", b =>
                {
                    b.HasOne("TaskVault.DataAccess.Entities.User", "User")
                        .WithMany("EmailConfirmationRequests")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
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

            modelBuilder.Entity("TaskVault.DataAccess.Entities.FileShareRequest", b =>
                {
                    b.HasOne("TaskVault.DataAccess.Entities.File", "File")
                        .WithMany()
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.User", "From")
                        .WithMany()
                        .HasForeignKey("FromId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.FileShareRequestStatus", "Status")
                        .WithMany()
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.User", "To")
                        .WithMany()
                        .HasForeignKey("ToId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("File");

                    b.Navigation("From");

                    b.Navigation("Status");

                    b.Navigation("To");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.Notification", b =>
                {
                    b.HasOne("TaskVault.DataAccess.Entities.NotificationStatus", "NotificationStatus")
                        .WithMany()
                        .HasForeignKey("NotificationStatusId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.NotificationType", "NotificationType")
                        .WithMany()
                        .HasForeignKey("NotificationTypeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.User", "ToUser")
                        .WithMany()
                        .HasForeignKey("ToId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("NotificationStatus");

                    b.Navigation("NotificationType");

                    b.Navigation("ToUser");
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

                    b.HasOne("TaskVault.DataAccess.Entities.Task", "Task")
                        .WithMany("TaskItems")
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("FileCategory");

                    b.Navigation("FileType");

                    b.Navigation("Task");
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

                    b.HasOne("TaskVault.DataAccess.Entities.Task", null)
                        .WithMany("TaskSubmissions")
                        .HasForeignKey("TaskId1");

                    b.Navigation("SubmittedByUser");

                    b.Navigation("Task");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.TaskSubmissionTaskItemFile", b =>
                {
                    b.HasOne("TaskVault.DataAccess.Entities.File", "File")
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

                    b.Navigation("File");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.TaskSubmissionTaskItemFileComment", b =>
                {
                    b.HasOne("TaskVault.DataAccess.Entities.User", "FromUser")
                        .WithMany()
                        .HasForeignKey("FromUserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.TaskSubmission", "TaskSubmission")
                        .WithMany()
                        .HasForeignKey("TaskSubmissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaskVault.DataAccess.Entities.TaskSubmissionTaskItemFile", "TaskSubmissionTaskItemFile")
                        .WithMany("Comments")
                        .HasForeignKey("TaskSubmissionId", "TaskSubmissionTaskItemFileId", "FromUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("FromUser");

                    b.Navigation("TaskSubmission");

                    b.Navigation("TaskSubmissionTaskItemFile");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.File", b =>
                {
                    b.Navigation("AsDirectoryEntries");

                    b.Navigation("AsFileEntries");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.Task", b =>
                {
                    b.Navigation("TaskItems");

                    b.Navigation("TaskSubmissions");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.TaskSubmissionTaskItemFile", b =>
                {
                    b.Navigation("Comments");
                });

            modelBuilder.Entity("TaskVault.DataAccess.Entities.User", b =>
                {
                    b.Navigation("DirectoryEntries");

                    b.Navigation("EmailConfirmationRequests");
                });
#pragma warning restore 612, 618
        }
    }
}
