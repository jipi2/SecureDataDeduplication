﻿// <auto-generated />
using System;
using FileStorageApp.Server.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FileStorageApp.Server.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("FileStorageApp.Server.Entity.FSRC", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Base64KFrag")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ReceiverId")
                        .HasColumnType("int");

                    b.Property<int>("SenderId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SenderId");

                    b.ToTable("FSRCs");
                });

            modelBuilder.Entity("FileStorageApp.Server.Entity.FileMetadata", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("BlobLink")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("Size")
                        .HasColumnType("float");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("isDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("isInCache")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("FilesMetadata");
                });

            modelBuilder.Entity("FileStorageApp.Server.Entity.FileTransfer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ReceiverId")
                        .HasColumnType("int");

                    b.Property<int>("SenderId")
                        .HasColumnType("int");

                    b.Property<string>("base64EncIv")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("base64EncKey")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("isDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("isInCache")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("SenderId");

                    b.ToTable("FileTransfers");
                });

            modelBuilder.Entity("FileStorageApp.Server.Entity.Label", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Labels");
                });

            modelBuilder.Entity("FileStorageApp.Server.Entity.Resp", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Answer")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("FileMetadataId")
                        .HasColumnType("int");

                    b.Property<int>("Position_1")
                        .HasColumnType("int");

                    b.Property<int>("Position_2")
                        .HasColumnType("int");

                    b.Property<int>("Position_3")
                        .HasColumnType("int");

                    b.Property<bool>("wasUsed")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("FileMetadataId");

                    b.ToTable("Resps");
                });

            modelBuilder.Entity("FileStorageApp.Server.Entity.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("FileStorageApp.Server.Entity.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Base64PublicKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Base64RSAEncPrivateKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Base64RSAPublicKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("G")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("P")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ServerDHPrivate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ServerDHPublic")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SymKey")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("isDeleted")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("FileStorageApp.Server.Entity.UserFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("FileId")
                        .HasColumnType("int");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Iv")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UploadDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("UserId")
                        .IsRequired()
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FileId");

                    b.HasIndex("UserId");

                    b.ToTable("UserFiles");
                });

            modelBuilder.Entity("LabelUserFile", b =>
                {
                    b.Property<int>("LabelsId")
                        .HasColumnType("int");

                    b.Property<int>("UserFilesId")
                        .HasColumnType("int");

                    b.HasKey("LabelsId", "UserFilesId");

                    b.HasIndex("UserFilesId");

                    b.ToTable("LabelUserFile");
                });

            modelBuilder.Entity("RoleUser", b =>
                {
                    b.Property<int>("RolesId")
                        .HasColumnType("int");

                    b.Property<int>("UsersId")
                        .HasColumnType("int");

                    b.HasKey("RolesId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("UserRoles", (string)null);
                });

            modelBuilder.Entity("FileStorageApp.Server.Entity.FSRC", b =>
                {
                    b.HasOne("FileStorageApp.Server.Entity.User", "Sender")
                        .WithMany()
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("FileStorageApp.Server.Entity.FileTransfer", b =>
                {
                    b.HasOne("FileStorageApp.Server.Entity.User", "Sender")
                        .WithMany()
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("FileStorageApp.Server.Entity.Resp", b =>
                {
                    b.HasOne("FileStorageApp.Server.Entity.FileMetadata", "FileMetadata")
                        .WithMany("Resps")
                        .HasForeignKey("FileMetadataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FileMetadata");
                });

            modelBuilder.Entity("FileStorageApp.Server.Entity.UserFile", b =>
                {
                    b.HasOne("FileStorageApp.Server.Entity.FileMetadata", "FileMetadata")
                        .WithMany()
                        .HasForeignKey("FileId");

                    b.HasOne("FileStorageApp.Server.Entity.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FileMetadata");

                    b.Navigation("User");
                });

            modelBuilder.Entity("LabelUserFile", b =>
                {
                    b.HasOne("FileStorageApp.Server.Entity.Label", null)
                        .WithMany()
                        .HasForeignKey("LabelsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FileStorageApp.Server.Entity.UserFile", null)
                        .WithMany()
                        .HasForeignKey("UserFilesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RoleUser", b =>
                {
                    b.HasOne("FileStorageApp.Server.Entity.Role", null)
                        .WithMany()
                        .HasForeignKey("RolesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FileStorageApp.Server.Entity.User", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("FileStorageApp.Server.Entity.FileMetadata", b =>
                {
                    b.Navigation("Resps");
                });
#pragma warning restore 612, 618
        }
    }
}
