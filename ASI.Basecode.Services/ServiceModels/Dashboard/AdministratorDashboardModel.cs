using System.Collections.Generic;

namespace ASI.Basecode.Services.ServiceModels.Dashboard
{
    public class AdministratorDashboardModel
    {
        public int TotalItems { get; set; }
        public int ActiveCategories { get; set; }
        public int ActiveLoanCount { get; set; }
        public int UnderMaintenanceCount { get; set; }
        public int DamagedAwaitingReviewCount { get; set; }
        public int IneligibleBorrowerCount { get; set; }
        public List<CategoryInventoryRow> InventoryByCategory { get; set; } = new();
        public List<AttentionItem> NeedsAttention { get; set; } = new();
    }

    public class CategoryInventoryRow
    {
        public long CategoryId { get; set; }
        public string Name { get; set; }
        public int Available { get; set; }
        public int Reserved { get; set; }
        public int Borrowed { get; set; }
        public int Maintenance { get; set; }
        public int Unavailable { get; set; }
        public int Total => Available + Reserved + Borrowed + Maintenance + Unavailable;
        public int AvailableSpareCount => Available;
    }

    public class AttentionItem
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public string Href { get; set; }
        public bool IsAvailable { get; set; }
    }
}
