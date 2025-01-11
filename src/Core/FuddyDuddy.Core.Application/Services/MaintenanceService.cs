using System.Text;
using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Domain.Entities;

namespace FuddyDuddy.Core.Application.Services;

public interface IMaintenanceService
{
    Task<string> RevisitCategoriesAsync(DateTimeOffset since, CancellationToken cancellationToken = default);
}

internal class MaintenanceService : IMaintenanceService
{
    private readonly ISummaryValidationService _summaryValidationService;
    private readonly INewsSummaryRepository _summaryRepository;
    private readonly ICategoryRepository _categoryRepository;
    public MaintenanceService(
        ISummaryValidationService summaryValidationService,
        INewsSummaryRepository summaryRepository,
        ICategoryRepository categoryRepository)
    {
        _summaryValidationService = summaryValidationService;
        _summaryRepository = summaryRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<string> RevisitCategoriesAsync(DateTimeOffset since, CancellationToken cancellationToken = default)
    {
        var summaries = await _summaryRepository.GetByStateAsync([NewsSummaryState.Validated, NewsSummaryState.Digested], since, cancellationToken);

        var categories = (await _categoryRepository.GetAllAsync(cancellationToken)).ToDictionary(c=>c.Local, c=>c);
        var categoryPrompt = string.Join("\n", categories.Select(c => $"{c.Key} ({c.Value.KeywordsLocal})"));

        var result = new StringBuilder();
        foreach (var summary in summaries)
        {
            var response = await _summaryValidationService.ValidateSummaryAsync(summary, categoryPrompt, cancellationToken);
            if (response.IsValid 
                && categories.TryGetValue(response.Topic, out var newCategory)
                && newCategory.Id != summary.Category.Id)
            {
                result.AppendLine($"Title: {summary.Title}, Article: {summary.Article}, Category updated from {summary.Category.Local} to {newCategory.Local}");
                summary.UpdateCategory(newCategory.Id);
                await _summaryRepository.UpdateAsync(summary, cancellationToken);
            }
        }
        return result.ToString();
    }
}