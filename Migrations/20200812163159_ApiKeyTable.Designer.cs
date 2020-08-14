﻿// <auto-generated />
using System;
using IdleBusiness.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IdleBusiness.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20200812163159_ApiKeyTable")]
    partial class ApiKeyTable
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("IdleBusiness.Models.ApiKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<Guid>("Key")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.ToTable("ApiKeys");
                });

            modelBuilder.Entity("IdleBusiness.Models.Business", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AmountEmployed")
                        .HasColumnType("int");

                    b.Property<int>("AmountOwnedItems")
                        .HasColumnType("int");

                    b.Property<double>("Cash")
                        .HasColumnType("double");

                    b.Property<double>("CashPerSecond")
                        .HasColumnType("double");

                    b.Property<double>("EspionageChance")
                        .HasColumnType("double");

                    b.Property<double>("EspionageDefense")
                        .HasColumnType("double");

                    b.Property<DateTime>("LastCheckIn")
                        .HasColumnType("datetime(6)");

                    b.Property<double>("LifeTimeEarnings")
                        .HasColumnType("double");

                    b.Property<int>("MaxEmployeeAmount")
                        .HasColumnType("int");

                    b.Property<int>("MaxItemAmount")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Product")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime?>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp(6)");

                    b.Property<int?>("SectorId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SectorId");

                    b.ToTable("Business");
                });

            modelBuilder.Entity("IdleBusiness.Models.BusinessInvestment", b =>
                {
                    b.Property<int>("BusinessId")
                        .HasColumnType("int");

                    b.Property<int>("InvestmentId")
                        .HasColumnType("int");

                    b.Property<int>("InvestmentDirection")
                        .HasColumnType("int");

                    b.Property<int>("InvestmentType")
                        .HasColumnType("int");

                    b.HasKey("BusinessId", "InvestmentId");

                    b.HasIndex("InvestmentId");

                    b.ToTable("BusinessInvestments");
                });

            modelBuilder.Entity("IdleBusiness.Models.BusinessPurchase", b =>
                {
                    b.Property<int>("BusinessId")
                        .HasColumnType("int");

                    b.Property<int>("PurchaseId")
                        .HasColumnType("int");

                    b.Property<int>("AmountOfPurchases")
                        .HasColumnType("int");

                    b.HasKey("BusinessId", "PurchaseId");

                    b.HasIndex("PurchaseId");

                    b.ToTable("BusinessPurchases");
                });

            modelBuilder.Entity("IdleBusiness.Models.Investment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int?>("BusinessId")
                        .HasColumnType("int");

                    b.Property<double>("InvestedBusinessCashAtInvestment")
                        .HasColumnType("double");

                    b.Property<double>("InvestedBusinessCashPerSecondAtInvestment")
                        .HasColumnType("double");

                    b.Property<double>("InvestmentAmount")
                        .HasColumnType("double");

                    b.Property<DateTime>("InvestmentExpiration")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("InvestmentType")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BusinessId");

                    b.ToTable("Investments");
                });

            modelBuilder.Entity("IdleBusiness.Models.Log", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Callsite")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Exception")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Level")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime>("Logged")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Logger")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("MachineName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Message")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("IdleBusiness.Models.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("DateReceived")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("MessageBody")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("ReadByBusiness")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("ReceivingBusinessId")
                        .HasColumnType("int");

                    b.Property<int>("SendingBusinessId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ReceivingBusinessId");

                    b.HasIndex("SendingBusinessId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("IdleBusiness.Models.Purchasable", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<double>("CashModifier")
                        .HasColumnType("double");

                    b.Property<double>("Cost")
                        .HasColumnType("double");

                    b.Property<string>("Description")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<double>("EspionageDefenseModifier")
                        .HasColumnType("double");

                    b.Property<double>("EspionageModifier")
                        .HasColumnType("double");

                    b.Property<bool>("IsGlobalPurchase")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsSinglePurchase")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsUpgrade")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("MaxEmployeeModifier")
                        .HasColumnType("int");

                    b.Property<int>("MaxItemAmountModifier")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<double>("PerOwnedModifier")
                        .HasColumnType("double");

                    b.Property<int>("PurchasableTypeId")
                        .HasColumnType("int");

                    b.Property<int>("PurchasableUpgradeId")
                        .HasColumnType("int");

                    b.Property<double>("UnlocksAtTotalEarnings")
                        .HasColumnType("double");

                    b.HasKey("Id");

                    b.HasIndex("PurchasableTypeId");

                    b.HasIndex("PurchasableUpgradeId")
                        .IsUnique();

                    b.ToTable("Purchasables");
                });

            modelBuilder.Entity("IdleBusiness.Models.PurchasableType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.ToTable("PurchasableTypes");
                });

            modelBuilder.Entity("IdleBusiness.Models.Sector", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Name")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.ToTable("Sectors");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(256) CHARACTER SET utf8mb4")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasColumnType("varchar(256) CHARACTER SET utf8mb4")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Email")
                        .HasColumnType("varchar(256) CHARACTER SET utf8mb4")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("NormalizedEmail")
                        .HasColumnType("varchar(256) CHARACTER SET utf8mb4")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("varchar(256) CHARACTER SET utf8mb4")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserName")
                        .HasColumnType("varchar(256) CHARACTER SET utf8mb4")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");

                    b.HasDiscriminator<string>("Discriminator").HasValue("IdentityUser");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(128) CHARACTER SET utf8mb4")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderKey")
                        .HasColumnType("varchar(128) CHARACTER SET utf8mb4")
                        .HasMaxLength(128);

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("RoleId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(128) CHARACTER SET utf8mb4")
                        .HasMaxLength(128);

                    b.Property<string>("Name")
                        .HasColumnType("varchar(128) CHARACTER SET utf8mb4")
                        .HasMaxLength(128);

                    b.Property<string>("Value")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("IdleBusiness.Models.Entrepreneur", b =>
                {
                    b.HasBaseType("Microsoft.AspNetCore.Identity.IdentityUser");

                    b.Property<int>("BusinessId")
                        .HasColumnType("int");

                    b.Property<int>("Score")
                        .HasColumnType("int");

                    b.HasIndex("BusinessId")
                        .IsUnique();

                    b.HasDiscriminator().HasValue("Entrepreneur");
                });

            modelBuilder.Entity("IdleBusiness.Models.Business", b =>
                {
                    b.HasOne("IdleBusiness.Models.Sector", "Sector")
                        .WithMany("Businesses")
                        .HasForeignKey("SectorId");
                });

            modelBuilder.Entity("IdleBusiness.Models.BusinessInvestment", b =>
                {
                    b.HasOne("IdleBusiness.Models.Business", "Business")
                        .WithMany("BusinessInvestments")
                        .HasForeignKey("BusinessId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("IdleBusiness.Models.Investment", "Investment")
                        .WithMany("BusinessInvestments")
                        .HasForeignKey("InvestmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("IdleBusiness.Models.BusinessPurchase", b =>
                {
                    b.HasOne("IdleBusiness.Models.Business", "Business")
                        .WithMany("BusinessPurchases")
                        .HasForeignKey("BusinessId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("IdleBusiness.Models.Purchasable", "Purchase")
                        .WithMany("BusinessPurchases")
                        .HasForeignKey("PurchaseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("IdleBusiness.Models.Investment", b =>
                {
                    b.HasOne("IdleBusiness.Models.Business", null)
                        .WithMany("GroupInvestments")
                        .HasForeignKey("BusinessId");
                });

            modelBuilder.Entity("IdleBusiness.Models.Message", b =>
                {
                    b.HasOne("IdleBusiness.Models.Business", "ReceivingBusiness")
                        .WithMany("ReceivedMessages")
                        .HasForeignKey("ReceivingBusinessId");

                    b.HasOne("IdleBusiness.Models.Business", "SendingBusiness")
                        .WithMany("SentMessages")
                        .HasForeignKey("SendingBusinessId");
                });

            modelBuilder.Entity("IdleBusiness.Models.Purchasable", b =>
                {
                    b.HasOne("IdleBusiness.Models.PurchasableType", "Type")
                        .WithMany("Purchasables")
                        .HasForeignKey("PurchasableTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("IdleBusiness.Models.Purchasable", "PurchasableUpgrade")
                        .WithOne()
                        .HasForeignKey("IdleBusiness.Models.Purchasable", "PurchasableUpgradeId");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("IdleBusiness.Models.Entrepreneur", b =>
                {
                    b.HasOne("IdleBusiness.Models.Business", "Business")
                        .WithOne("Owner")
                        .HasForeignKey("IdleBusiness.Models.Entrepreneur", "BusinessId");
                });
#pragma warning restore 612, 618
        }
    }
}
