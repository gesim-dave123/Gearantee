using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.ServiceModels.EquipmentItem;
using ASI.Basecode.Services.ServiceModels.Returns;
using ASI.Basecode.Services.ServiceModels.Reservations;
using ASI.Basecode.Services.Services;
using ASI.Basecode.Services.Utilities;
using ASI.Basecode.Tests.Dashboard;
using ASI.Basecode.Tests.UserAdministration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static ASI.Basecode.Tests.Dashboard.DashboardSeed;
using static ASI.Basecode.Tests.Reservations.ReservationTestHelpers;

namespace ASI.Basecode.Tests.Resilience
{
    /// <summary>
    /// Regression tests for the SQL Server retry execution strategy rollout (fix for the
    /// "physical connection is not usable" error). Every service method that opens a
    /// user-initiated transaction must wrap it in Database.CreateExecutionStrategy(), or
    /// EF Core throws InvalidOperationException the moment EnableRetryOnFailure is on.
    /// Tests 3-5 stay red (InvalidOperationException) until the corresponding transaction
    /// site is wrapped; tests 1-2 guard the reservation wraps. Opt-in via
    /// GEARANTEE_TEST_SQLSERVER=1 (Windows with LocalDB), like the rest of the suite.
    /// </summary>
    public sealed class RetryExecutionStrategyTests
    {
        private static readonly DateTime Now = Utc(2026, 10, 7, 0);

        [SqlServerFact]
        public async Task SqlServer_reservation_submit_wraps_transaction_in_retry_strategy()
        {
            await using var environment = await RetryEnvironment.CreateAsync();
            string borrowerId;
            long itemId;
            using (var db = environment.NewSeedingContext())
            {
                var user = User(db, "BRW-1");
                Profile(db, user);
                borrowerId = user.Id;
                itemId = Item(db, "ITEM-1").EquipmentId;
            }

            var request = new ReservationCreateViewModel
            {
                EquipmentId = itemId,
                ReservationStart = ManilaClock.ToLocal(Now.AddDays(1)),
                ReservationEnd = ManilaClock.ToLocal(Now.AddDays(1).AddHours(3)),
                Purpose = "Thesis defense"
            };

            var result = await environment.With<ReservationService, ReservationResult>(
                service => Task.Run(() => service.SubmitReservation(request, borrowerId)));

            Assert.True(result.Succeeded, result.Message);
        }

        [SqlServerFact]
        public async Task SqlServer_reservation_approve_wraps_transaction_in_retry_strategy()
        {
            await using var environment = await RetryEnvironment.CreateAsync();
            long reservationId;
            using (var db = environment.NewSeedingContext())
            {
                var user = User(db, "BRW-1");
                var profile = Profile(db, user);
                var item = Item(db, "ITEM-1");
                reservationId = Reservation(db, profile, item,
                    Now.AddDays(1), Now.AddDays(1).AddHours(3),
                    DomainValues.ReservationStatuses.Pending, Now.AddDays(-1)).ReservationId;
            }

            var actorId = environment.ActorId;
            var result = await environment.With<ReservationService, ReservationResult>(
                service => Task.Run(() => service.ApproveReservation(actorId, reservationId)));

            Assert.True(result.Succeeded, result.Message);
            using var check = environment.NewSeedingContext();
            Assert.Equal(
                DomainValues.ReservationStatuses.Approved,
                check.Reservations.Single().Status);
        }

        [SqlServerFact]
        public async Task SqlServer_return_record_wraps_transaction_in_retry_strategy()
        {
            await using var environment = await RetryEnvironment.CreateAsync();
            long releaseRecordId;
            using (var db = environment.NewSeedingContext())
            {
                var user = User(db, "BRW-1");
                var profile = Profile(db, user);
                var item = Item(db, "ITEM-1", DomainValues.EquipmentStatuses.Borrowed);
                var reservation = Reservation(db, profile, item,
                    Now.AddDays(-2), Now.AddDays(1),
                    DomainValues.ReservationStatuses.Approved, Now.AddDays(-2));
                releaseRecordId = Release(db, reservation, Now.AddHours(-1)).ReleaseRecordId;
            }

            var actorId = environment.ActorId;
            var model = new ReturnRecordViewModel
            {
                ReleaseRecordId = releaseRecordId,
                ReturnedCondition = DomainValues.ReturnConditions.Good,
                ResultingItemStatus = DomainValues.EquipmentStatuses.Available,
                ActualReturnAt = ManilaClock.ToLocal(Now),
                Notes = null
            };

            var result = await environment.With<ReturnRecordService, ReturnRecordResult>(
                service => Task.Run(() => service.AddReturnRecord(model, actorId)));

            Assert.True(result.Succeeded, result.Message);
            using var check = environment.NewSeedingContext();
            Assert.Single(check.ReturnRecords);
        }

        [SqlServerFact]
        public async Task SqlServer_equipment_item_archive_wraps_transaction_in_retry_strategy()
        {
            await using var environment = await RetryEnvironment.CreateAsync();
            long itemId;
            using (var db = environment.NewSeedingContext())
            {
                itemId = Item(db, "ITEM-1").EquipmentId;
            }

            var result = await environment.With<EquipmentItemService, EquipmentItemOperationResult>(
                service => service.SetArchivedAsync(environment.ActorId, itemId, isArchived: true));

            Assert.True(result.Succeeded, result.Message);
            using var check = environment.NewSeedingContext();
            Assert.True(check.EquipmentItems.Single().IsArchived);
        }

        [SqlServerFact]
        public async Task SqlServer_borrower_profile_eligibility_wraps_transaction_in_retry_strategy()
        {
            await using var environment = await RetryEnvironment.CreateAsync();
            long profileId;
            using (var db = environment.NewSeedingContext())
            {
                profileId = Profile(db, User(db, "BRW-1")).BorrowerProfileId;
            }

            var result = await environment.With<BorrowerProfileService, BorrowerProfileResult>(
                service => service.UpdateEligibilityAsync(environment.ActorId, profileId, isEligible: false));

            Assert.True(result.Succeeded, result.Message);
            using var check = environment.NewSeedingContext();
            Assert.False(check.BorrowerProfiles.Single().IsEligible);
        }

        // TODO: once UserAdministrationService.UpdateAsync/SetActiveAsync/CreateAsync/ImportAsync
        // are wrapped, extend RetryEnvironment with the identity builders they need and add a
        // matching test; AccountController.Register needs an MVC integration harness.
    }

    /// <summary>
    /// A disposable LocalDB database whose contexts are configured exactly like production WebApp
    /// (UseSqlServer + EnableRetryOnFailure), plus the role/permission seed the services' guards
    /// expect. The administrator actor also holds the Custodian role, so review/return guards pass.
    /// </summary>
    internal sealed class RetryEnvironment : IAsyncDisposable
    {
        private static readonly DateTime Now = Utc(2026, 10, 7, 0);

        private readonly string _connectionString;
        private ServiceProvider _provider;

        private RetryEnvironment()
        {
            _connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=Gearantee_Retry_Test_" +
                Guid.NewGuid().ToString("N") + ";Trusted_Connection=True;TrustServerCertificate=True";
        }

        public string ActorId { get; private set; }

        public static async Task<RetryEnvironment> CreateAsync()
        {
            var environment = new RetryEnvironment();
            var services = new ServiceCollection();
            environment.ConfigureServices(services);
            environment._provider = services.BuildServiceProvider();
            try
            {
                using var scope = environment._provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AsiBasecodeDBContext>();
                await db.Database.MigrateAsync();
                environment.Seed(db);
                return environment;
            }
            catch
            {
                await environment.DisposeAsync();
                throw;
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
            services.AddScoped<IAuditService, AuditService>();
            services.AddDbContext<AsiBasecodeDBContext>(options => options
                .UseSqlServer(_connectionString, sqlServerOptions => sqlServerOptions
                    .EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null)));
            services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AsiBasecodeDBContext>().AddDefaultTokenProviders();

            services.AddSingleton(TimeProvider);
            services.AddScoped<IUnitOfWork>(provider => new UnitOfWork(
                provider.GetRequiredService<AsiBasecodeDBContext>()));
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IEquipmentItemRepository, EquipmentItemRepository>();
            services.AddScoped<IBorrowerProfileRepository, BorrowerProfileRepository>();
            services.AddScoped<IReturnRecordRepository, ReturnRecordRepository>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ReservationService>();
            services.AddScoped<ReturnRecordService>();
            services.AddScoped<BorrowerProfileService>();
            services.AddScoped<EquipmentItemService>();
        }

        private TimeProvider TimeProvider =>
            new FixedTimeProvider(Now);

        private void Seed(AsiBasecodeDBContext db)
        {
            foreach (var roleName in new[] { DomainValues.Roles.Custodian, DomainValues.Roles.Administrator })
            {
                db.Roles.Add(new IdentityRole(roleName));
                db.SaveChanges();
            }

            foreach (var permissionName in new[]
            {
                DomainValues.Permissions.ReservationCreate,
                DomainValues.Permissions.ReservationReview,
                DomainValues.Permissions.TransactionReleaseReturn,
                DomainValues.Permissions.EquipmentManage,
                DomainValues.Permissions.BorrowerManage,
                DomainValues.Permissions.UserRoleManage
            })
            {
                db.Permissions.Add(new Permission
                {
                    PermissionName = permissionName,
                    Description = permissionName
                });
            }
            db.SaveChanges();

            foreach (var role in db.Roles.ToList())
            {
                foreach (var permission in db.Permissions.ToList())
                {
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.PermissionId
                    });
                }
            }
            db.SaveChanges();

            var actor = User(db, "ADMIN-1");
            var administratorId = db.Roles.Single(role => role.Name == DomainValues.Roles.Administrator).Id;
            var custodianId = db.Roles.Single(role => role.Name == DomainValues.Roles.Custodian).Id;
            db.UserRoles.Add(new IdentityUserRole<string> { UserId = actor.Id, RoleId = administratorId });
            db.UserRoles.Add(new IdentityUserRole<string> { UserId = actor.Id, RoleId = custodianId });
            db.SaveChanges();
            ActorId = actor.Id;
        }

        /// <summary>A plain context for seeding, exactly like the production settings.</summary>
        public AsiBasecodeDBContext NewSeedingContext() =>
            new(new DbContextOptionsBuilder<AsiBasecodeDBContext>()
                .UseSqlServer(_connectionString)
                .Options);

        public async Task<TResult> With<TService, TResult>(
            Func<TService, Task<TResult>> operation) where TService : notnull
        {
            using var scope = _provider.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            return await operation(service);
        }

        public async ValueTask DisposeAsync()
        {
            if (_provider == null)
            {
                return;
            }

            await using var scope = _provider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AsiBasecodeDBContext>();
            await db.Database.EnsureDeletedAsync();
            await _provider.DisposeAsync();
        }
    }
}
