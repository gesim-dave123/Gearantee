using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace ASI.Basecode.Tests.UiMigration
{
    public class UiAcceptanceTests
    {
        [Fact]
        public async Task All_role_screens_render_through_real_MVC_with_existing_permission_boundaries()
        {
            await using var site = await UiAcceptanceSite.CreateAsync();
            using var admin = site.NewClient();
            using var custodian = site.NewClient();
            using var borrower = site.NewClient();
            await site.LoginAsync(admin, "UI-ADMIN");
            await site.LoginAsync(custodian, "UI-CUST");
            await site.LoginAsync(borrower, "UI-BORROWER");
            var adminPaths = new[]
            {
                "/Dashboard/Administrator", "/Categories/Index", "/Categories/Index?tab=categories", "/Categories/Create",
                $"/Categories/Edit?id={site.CategoryId}", "/Categories/CreateEquipment",
                $"/Categories/EditEquipment?id={site.AvailableItemId}", $"/Categories/EditEquipment?id={site.LockedItemId}",
                "/BorrowerProfile/Index", $"/BorrowerProfile/Edit?id={site.BorrowerProfileId}",
                "/Users/Index", "/Users/Create", $"/Users/Edit?id={site.BorrowerId}", "/Users/Import",
                "/BorrowingHistory/Index", "/AvailabilityCalendar/Index"
            };
            var custodianPaths = new[]
            {
                "/Dashboard/Custodian", "/ReservationReview/Index", "/ReservationReview/Index?status=Pending", "/ReservationReview/Index?status=Approved",
                "/ReservationReview/Index?status=Rejected", "/ReservationReview/Index?status=All",
                "/ReservationReview/Index?status=Cancelled", "/ReservationReview/Index?status=Expired",
                $"/ReservationReview/Details?id={site.PendingId}", $"/ReservationReview/Details?id={site.PickupId}",
                $"/ReservationReview/Details?id={site.RejectedId}", "/ReturnRecord/Index", "/ReturnRecord/Index?tab=Overdue",
                "/ReturnRecord/Index?tab=Returned", "/ReturnRecord/Index?tab=Late",
                $"/ReturnRecord/Create?releaseRecordId={site.ReleaseId}", $"/ReturnRecord/Details?id={site.ReturnedId}",
                "/BorrowingHistory/Index", "/AvailabilityCalendar/Index"
            };
            foreach (var path in adminPaths) await Check(admin, path);
            foreach (var path in custodianPaths) await Check(custodian, path);
            foreach (var path in new[] { "/Dashboard/Borrower", "/Catalog/Index", $"/Catalog/Details?id={site.AvailableItemId}", "/Reservation/Index", $"/Reservation/Create?equipmentId={site.AvailableItemId}", "/AvailabilityCalendar/Index" })
                await Check(borrower, path);
            foreach (var client in new[] { custodian, borrower })
            {
                var denied = await client.GetAsync("/Users/Index");
                Assert.Equal(HttpStatusCode.Redirect, denied.StatusCode);
                Assert.Contains("AccessDenied", denied.Headers.Location.ToString());
            }
        }

        // Regression coverage for asynchronous disposal of the streamed CSV response.
        [Fact]
        public async Task Csv_export_completes_under_default_Kestrel_response_rules()
        {
            await using var site = await UiAcceptanceSite.CreateAsync();
            using var admin = site.NewClient();
            await site.LoginAsync(admin, "UI-ADMIN");
            System.Net.Http.HttpResponseMessage csv;
            try { csv = await admin.GetAsync("/BorrowingHistory/Export"); }
            catch (System.Net.Http.HttpRequestException error)
            {
                throw new InvalidOperationException("CSV server failure: " + site.LastRequestFailure, error);
            }
            Assert.Equal(HttpStatusCode.OK, csv.StatusCode);
            Assert.Contains("text/csv", csv.Content.Headers.ContentType.ToString());
            Assert.Equal("attachment", csv.Content.Headers.ContentDisposition.DispositionType);
            Assert.StartsWith("borrowing-history-", csv.Content.Headers.ContentDisposition.FileName.Trim('"'));
            var content = await csv.Content.ReadAsStringAsync();
            Assert.StartsWith(ASI.Basecode.Services.Utilities.BorrowingHistoryCsvFormatter.FormatHeader(), content);
            Assert.Contains("DEMO-LAP-001", content);
        }

        [Fact]
        public async Task Empty_filters_and_invalid_forms_keep_existing_validation_and_redisplay_context()
        {
            await using var site = await UiAcceptanceSite.CreateAsync();
            using var admin = site.NewClient();
            using var custodian = site.NewClient();
            await site.LoginAsync(admin, "UI-ADMIN");
            await site.LoginAsync(custodian, "UI-CUST");
            foreach (var entry in new[]
            {
                ("/BorrowerProfile/Index?search=NO_MATCH_UI_FIXTURE", "No borrower profiles match that search."),
                ("/Categories/Index?equipmentSearch=NO_MATCH_UI_FIXTURE", "No equipment items match these filters."),
                ("/Users/Index?search=NO_MATCH_UI_FIXTURE", "No accounts match these filters."),
                ("/BorrowingHistory/Index?Search=NO_MATCH_UI_FIXTURE", "No borrowing history found")
            }) Assert.Contains(entry.Item2, await Check(admin, entry.Item1));
            Assert.Contains("Nothing to review", await Check(custodian, "/ReservationReview/Index?search=NO_MATCH_UI_FIXTURE"));
            await Invalid(admin, "/Categories/Create", new() { ["CategoryCode"] = "", ["CategoryName"] = "" });
            await Invalid(admin, "/Users/Create", new() { ["RoleName"] = "Custodian", ["UserCode"] = "", ["FirstName"] = "", ["LastName"] = "", ["Email"] = "", ["Password"] = "", ["ConfirmPassword"] = "" });
            var profilePath = $"/BorrowerProfile/Edit?id={site.BorrowerProfileId}";
            var profileHtml = await admin.GetStringAsync(profilePath);
            var stamp = Regex.Match(profileHtml, "name=\"ConcurrencyStamp\"[^>]*value=\"([^\"]*)\"").Groups[1].Value;
            await Invalid(admin, profilePath, new() { ["id"] = site.BorrowerProfileId.ToString(), ["ConcurrencyStamp"] = WebUtility.HtmlDecode(stamp), ["Department"] = "" });
            await Invalid(custodian, $"/ReturnRecord/Create?releaseRecordId={site.ReleaseId}", new() { ["ReleaseRecordId"] = site.ReleaseId.ToString(), ["ActualReturnAt"] = "", ["ReturnedCondition"] = "", ["ResultingItemStatus"] = "" });
        }

        [InteractiveUiPreviewFact]
        public async Task Interactive_ui_preview_uses_only_disposable_fixture_data()
        {
            await using var site = await UiAcceptanceSite.CreateAsync();
            var stop = Path.Combine(site.Root, "artifacts/ui-acceptance-stop");
            var until = DateTime.UtcNow.AddMinutes(40);
            Console.WriteLine("UI acceptance preview: http://127.0.0.1:5192");
            while (!File.Exists(stop) && DateTime.UtcNow < until) await Task.Delay(1000);
        }

        private static async Task Invalid(HttpClient client, string path, Dictionary<string, string> fields)
        {
            var before = await client.GetStringAsync(path);
            fields["__RequestVerificationToken"] = UiAcceptanceSite.Token(before);
            var response = await client.PostAsync(path, new FormUrlEncodedContent(fields));
            var html = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Matches("class=\"[^\"]*field-validation-error[^\"]*\"[^>]*>\\s*[^<\\s]", html);
            Assert.NotEmpty(UiAcceptanceSite.Token(html));
        }

        private static async Task<string> Check(System.Net.Http.HttpClient client, string path)
        {
            var response = await client.GetAsync(path);
            var html = await response.Content.ReadAsStringAsync();
            var safeError = html.Split("HEADERS")[0];
            Assert.True(response.StatusCode == HttpStatusCode.OK, path + ": " + response.StatusCode + " " + safeError[..Math.Min(safeError.Length, 1800)]);
            Assert.Contains("main-content", html);
            Assert.DoesNotContain("Dashboard information could not be loaded", html);
            Assert.DoesNotContain("<title> · Gearantee</title>", html);
            return html;
        }
    }

    public sealed class InteractiveUiPreviewFactAttribute : FactAttribute
    {
        public InteractiveUiPreviewFactAttribute()
        {
            if (Environment.GetEnvironmentVariable("GEARANTEE_UI_PREVIEW") != "1")
                Skip = "Opt-in interactive browser fixture; set GEARANTEE_UI_PREVIEW=1 to run.";
        }
    }
}
