using church_api.Models;

namespace church_api.Repositories.Abstractions
{
    public interface IPrayerCardRepository
    {
        Task<IList<PrayerCard>> GetPrayCardsAsync(Guid userId, CancellationToken cancellationToken);
        Task<PrayerCard?> AddPrayerCardAsync(PrayerCard prayerCard, CancellationToken cancellationToken);
    }
}
