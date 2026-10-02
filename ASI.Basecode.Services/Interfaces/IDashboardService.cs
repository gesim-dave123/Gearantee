using ASI.Basecode.Services.ServiceModels.Dashboard;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<BorrowerDashboardModel> GetBorrowerDashboardAsync(string userId);
        Task<CustodianDashboardModel> GetCustodianDashboardAsync();
        Task<AdministratorDashboardModel> GetAdministratorDashboardAsync();
    }
}
