using ASI.Basecode.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace ASI.Basecode.Tests.Dashboard
{
    public sealed class SqliteDb : IDisposable
    {
        private readonly SqliteConnection _connection;
        public SqliteConnection Connection => _connection;

        public SqliteDb()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            using var db = Create();
            db.Database.EnsureCreated();
        }

        public SqliteDashboardDbContext Create() => new(
            new DbContextOptionsBuilder<SqliteDashboardDbContext>()
                .UseSqlite(_connection)
                .Options);

        public void Dispose() => _connection.Dispose();
    }

    public sealed class SqliteDashboardDbContext : AsiBasecodeDBContext
    {
        public SqliteDashboardDbContext(
            DbContextOptions<SqliteDashboardDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Keep the production model and queries, but remove SQL Server-only
            // defaults and type declarations that SQLite cannot create verbatim.
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    if (property.GetDefaultValueSql() == "SYSUTCDATETIME()")
                    {
                        property.SetDefaultValueSql(null);
                    }

                    if (property.ClrType == typeof(DateTime) ||
                        Nullable.GetUnderlyingType(property.ClrType) == typeof(DateTime))
                    {
                        property.SetColumnType(null);
                    }

                    if (property.GetColumnType() == "nvarchar(max)")
                    {
                        property.SetColumnType(null);
                    }
                }
            }
        }
    }
}
