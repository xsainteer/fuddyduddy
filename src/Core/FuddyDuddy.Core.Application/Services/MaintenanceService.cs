using FuddyDuddy.Core.Application.Repositories;
using FuddyDuddy.Core.Domain.Entities;
using System.Runtime.CompilerServices;

namespace FuddyDuddy.Core.Application.Services;

public interface IMaintenanceService
{
    IAsyncEnumerable<string> RevisitCategoriesAsync(DateTimeOffset since, CancellationToken cancellationToken = default);
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

    public async IAsyncEnumerable<string> RevisitCategoriesAsync(
        DateTimeOffset since, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var summaries = await _summaryRepository.GetByStateAsync([NewsSummaryState.Validated, NewsSummaryState.Digested], date: since, cancellationToken: cancellationToken);

        var categories = (await _categoryRepository.GetAllAsync(cancellationToken)).ToDictionary(c=>c.Local, c=>c);
        var categoryPrompt = string.Join("\n", categories.Select(c => $"{c.Key} ({c.Value.KeywordsLocal})"));

        var total = summaries.Count();
        var index = 0;
        foreach (var summary in summaries)
        {
            index++;
            var progress = $"PROGRESS: {index}/{total}";
            yield return progress;
            var response = await _summaryValidationService.ValidateSummaryAsync(summary, categoryPrompt, cancellationToken);
            if (response.IsValid 
                && categories.TryGetValue(response.Topic, out var newCategory)
                && newCategory.Id != summary.Category.Id)
            {
                var result = $"Category updated from {summary.Category.Local.ToUpper()} to {newCategory.Local.ToUpper()} for TITLE: {summary.Title}, ARTICLE: {summary.Article}";
                summary.UpdateCategory(newCategory.Id);
                await _summaryRepository.UpdateAsync(summary, cancellationToken);
                yield return result;
            }
            else
            {
                yield return $"Category NOT updated for TITLE: {summary.Title}, ARTICLE: {summary.Article}";
            }
        }
    }
}