using System.Collections.Generic;

namespace ASI.Basecode.Services.ServiceModels.Dashboard
{
    public class CustodianDashboardModel
    {
        public int PendingCount { get; set; }
        public int PendingStartingSoonCount { get; set; }
        public int PickupsTodayCount { get; set; }
        public int ReturnsDueTodayCount { get; set; }
        public int OverdueCount { get; set; }
        public List<ReservationRow> PendingQueue { get; set; } = new();
        public List<ReservationRow> PickupsToday { get; set; } = new();
        public List<ReservationRow> ReturnsDueToday { get; set; } = new();
        public List<ReservationRow> Overdue { get; set; } = new();
        public List<DashboardTask> Desk { get; set; } = new();
        public List<ItemRow> NeedsAttention { get; set; } = new();
    }

    public class DashboardTask
    {
        public string Kind { get; set; }
        public ReservationRow Row { get; set; }
        public int SortKey { get; set; }
    }

    public class ItemRow
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemStatus { get; set; }
        public string ConditionStatus { get; set; }
        public string LocationName { get; set; }
    }
}
