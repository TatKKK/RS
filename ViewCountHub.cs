using Microsoft.AspNetCore.SignalR;
using RS.Packages;

namespace RS
{
    public class ViewCountHub:Hub
    {
        private readonly IPKG_DOCTOR _package;
        private readonly ILogger<ViewCountHub> _logger;

        public ViewCountHub(IPKG_DOCTOR package, ILogger<ViewCountHub> logger)
        {
            _package = package;
            _logger = logger;
        }

        public async Task UpdateViewCount(int doctorId)
        {
            try
            {
                int updatedViewCount = _package.UpdateViewCount(doctorId);

                await Clients.All.SendAsync("ReceiveViewCountUpdate", doctorId, updatedViewCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the view count for doctor with ID {DoctorId}", doctorId);
              
                await Clients.Caller.SendAsync("Error", "An error occurred while updating the view count.");
            }
        }
    }
}
