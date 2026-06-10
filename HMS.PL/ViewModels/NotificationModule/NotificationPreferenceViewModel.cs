using HMS.BLL.Shared.Dtos.NotificationDtos.Results;

namespace HMS.PL.ViewModels.NotificationModule
{
    public class NotificationPreferenceViewModel
    {
        public IEnumerable<NotificationPreferenceResult> Preferences { get; init; } = [];
    }
}
