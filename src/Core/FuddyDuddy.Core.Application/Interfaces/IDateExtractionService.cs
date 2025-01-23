using System.Threading;
using System.Threading.Tasks;
using FuddyDuddy.Core.Application.Models.AI;

namespace FuddyDuddy.Core.Application.Interfaces;

public interface IDateExtractionService
{
    Task<DateRangeResponse> ExtractDateRangeAsync(string text, CancellationToken cancellationToken = default);
} 