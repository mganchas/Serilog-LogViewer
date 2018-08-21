using LiveViewer.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace LiveViewer.Services
{
    public class DatabaseContext : DbContext
    {
        private static string DbPath => @".\LiveViewer.db";
        private static string DbPathTemp => $"{DbPath}-journal";
        private static bool IsCreated = false;

        public DatabaseContext()
        {
            IsCreated = File.Exists(DbPath);
            if (!IsCreated)
            {
                var file = File.Create(DbPath);
                file.Close();

                Database.EnsureCreated();
            }
        }

        public static void KillDatabase()
        {
            try
            {
                File.Delete(DbPath);
                File.Delete(DbPathTemp);
            }
            catch (Exception) {

            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder)
        {
            optionBuilder.UseSqlite($@"Data Source={DbPath}");
        }

        public DbSet<Entry> Entries { get; set; }
    }
}
