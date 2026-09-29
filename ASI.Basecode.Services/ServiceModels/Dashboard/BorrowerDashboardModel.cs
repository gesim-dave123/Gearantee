using System.Collections.Generic;

namespace ASI.Basecode.Services.ServiceModels.Dashboard
{
    public class BorrowerDashboardModel
    {
        public string DisplayName { get; set; }
        public bool HasProfile { get; set; }
        public bool IsEligible { get; set; }
        public int LateReturnCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedAwaitingPickupCount { get; set; }
        public int ActiveLoanCount { get; set; }
        public int OverdueCount { get; set; }
        public int AvailableItemCount { get; set; }
        public int AvailableCategoryCount { get; set; }
        public List<ReservationRow> CurrentLoans { get; set; } = new();
        public List<ReservationRow> UpcomingPickups { get; set; } = new();
        public List<ReservationRow> RecentRequests { get; set; } = new();
    }
}
