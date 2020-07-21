using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using IdleBusiness.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdleBusiness.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public virtual DbSet<Business> Business { get; set; }
        public virtual DbSet<Entrepreneur> Entrepreneurs { get; set; }
        public virtual DbSet<Purchasable> Purchasables { get; set; }
        public virtual DbSet<BusinessPurchase> BusinessPurchases { get; set; }
        public virtual DbSet<PurchasableType> PurchasableTypes { get; set; }
        public virtual DbSet<Investment> Investments { get; set; }
        public virtual DbSet<Sector> Sectors { get; set; }
        public virtual DbSet<Log> Logs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entrepreneur>()
                .HasOne(s => s.Business)
                .WithOne(s => s.Owner)
                .HasForeignKey<Entrepreneur>(s => s.BusinessId)
                .IsRequired(false);

            modelBuilder.Entity<BusinessPurchase>()
                .HasKey(s => new { s.BusinessId, s.PurchaseId });

            modelBuilder.Entity<BusinessPurchase>()
                .HasOne(s => s.Business)
                .WithMany(s => s.BusinessPurchases)
                .HasForeignKey(s => s.BusinessId);

            modelBuilder.Entity<BusinessPurchase>()
                .HasOne(s => s.Purchase)
                .WithMany(s => s.BusinessPurchases)
                .HasForeignKey(s => s.PurchaseId);

            modelBuilder.Entity<Investment>()
                .HasOne(s => s.BusinessToInvest)
                .WithMany(s => s.Investments)
                .HasForeignKey(s => s.BusinessToInvestId);

            modelBuilder.Entity<Business>()
                .HasMany(s => s.Investments)
                .WithOne(s => s.BusinessToInvest)
                .HasForeignKey(s => s.BusinessToInvestId);

            modelBuilder.Entity<Business>()
                .HasOne(s => s.Sector)
                .WithMany(s => s.Businesses)
                .IsRequired(false);

            modelBuilder.Entity<Purchasable>()
                .HasOne(s => s.Type)
                .WithMany(s => s.Purchasables)
                .HasForeignKey(s => s.PurchasableTypeId);

            modelBuilder.Entity<Business>()
                .Property(s => s.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<Purchasable>()
                .HasOne(s => s.PurchasableUpgrade)
                .WithOne()
                .IsRequired(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
