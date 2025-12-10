using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using AppCaravana.Models;     

namespace AppCaravana.Data     
{
    public class AppDbContext : DbContext
    {
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Caravana> Caravanas { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaCaravana> VentaCaravanas { get; set; }
        public DbSet<Stock> Stock { get; set; }
        public DbSet<AutorizacionSENASA> Autorizaciones { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Obtener la carpeta del proyecto (raíz, no bin)
            string projectRoot = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            // Subir dos niveles: bin\Release\net10.0-windows -> carpeta raíz del proyecto
            projectRoot = Directory.GetParent(projectRoot).Parent.Parent.FullName;

            if (!Directory.Exists(projectRoot))
                Directory.CreateDirectory(projectRoot);

            string dbPath = Path.Combine(projectRoot, "caravanas.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --------- RELACIONES ---------

            // Cliente —— (1:N) —— Ventas
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Venta —— (1:N) —— VentaCaravana —— (N:1) —— Caravana
            modelBuilder.Entity<VentaCaravana>()
                .HasOne(vc => vc.Venta)
                .WithMany(v => v.VentaCaravanas)
                .HasForeignKey(vc => vc.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VentaCaravana>()
                .HasOne(vc => vc.Caravana)
                .WithMany()
                .HasForeignKey(vc => vc.CaravanaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Caravana —— (1:N) —— Autorizaciones SENASA
            modelBuilder.Entity<AutorizacionSENASA>()
                .HasOne(a => a.Caravana)
                .WithMany()
                .HasForeignKey(a => a.CaravanaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Caravana —— (1:1) —— Stock
            //modelBuilder.Entity<Stock>()
            //    .HasOne(s => s.Caravana)
            //    .WithMany()
            //    .HasForeignKey(s => s.CaravanaId)
            //    .OnDelete(DeleteBehavior.Cascade);

            // --------- ÍNDICES ---------

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.DNI)
                .IsUnique();

            modelBuilder.Entity<Caravana>()
                .HasIndex(c => c.Matricula)
                .IsUnique();

            modelBuilder.Entity<Caravana>()
                .HasIndex(c => c.NumeroSenasa)
                .IsUnique();

            modelBuilder.Entity<Caravana>()
                .HasIndex(c => c.Serie)
                .IsUnique();
        }
    }
}
