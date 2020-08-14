using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using IdleBusiness.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<BusinessInvestment> BusinessInvestments { get; set; }
        public virtual DbSet<ApiKey> ApiKeys { get; set; }

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

            modelBuilder.Entity<BusinessInvestment>()
                .HasKey(s => new { s.BusinessId, s.InvestmentId });

            modelBuilder.Entity<BusinessInvestment>()
                .HasOne(s => s.Business)
                .WithMany(s => s.BusinessInvestments)
                .HasForeignKey(s => s.BusinessId);

            modelBuilder.Entity<BusinessInvestment>()
                .HasOne(s => s.Investment)
                .WithMany(s => s.BusinessInvestments)
                .HasForeignKey(s => s.InvestmentId);

            modelBuilder.Entity<Business>()
                .HasOne(s => s.Sector)
                .WithMany(s => s.Businesses)
                .IsRequired(false);

            modelBuilder.Entity<Business>()
                .Property(s => s.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<Purchasable>()
                .HasOne(s => s.PurchasableUpgrade)
                .WithOne()
                .IsRequired(false);

            modelBuilder.Entity<Message>()
                .HasOne(s => s.ReceivingBusiness)
                .WithMany(s => s.ReceivedMessages)
                .HasForeignKey(s => s.ReceivingBusinessId)
                .IsRequired(false);

            modelBuilder.Entity<Message>()
                .HasOne(s => s.SendingBusiness)
                .WithMany(s => s.SentMessages)
                .HasForeignKey(s => s.SendingBusinessId)
                .IsRequired(false);

            modelBuilder.Entity<ApiKey>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<ApiKey>()
                .Property(s => s.Key)
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }

    public static class ApplicationDbContextFactory
    {
        public static ApplicationDbContext CreateDbContext(IConfiguration config, DbContextOptions<ApplicationDbContext> options = null)
        {
            ApplicationDbContext context;
            if (options != null) context = new ApplicationDbContext(options);
            else context = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseMySql(config.GetConnectionString("DefaultConnection"))
                    .Options);

            return context;
        }
    }
}
