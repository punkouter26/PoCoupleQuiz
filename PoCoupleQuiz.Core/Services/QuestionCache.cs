using PoCoupleQuiz.Core.Models;

namespace PoCoupleQuiz.Core.Services;

/// <summary>
/// Service for caching generated questions.
/// </summary>
public interface IQuestionCache
{
    /// <summary>
    /// Gets a cached question if available.
    /// </summary>
    Question? GetCachedQuestion(string cacheKey);

    /// <summary>
    /// Caches a question for future use.
    /// </summary>
    void CacheQuestion(string cacheKey, Question question);

    /// <summary>
    /// Builds a cache key for a question.
    /// </summary>
    string BuildCacheKey(string difficulty, string? lastQuestion = null);
}

public class QuestionCache : IQuestionCache
{
    private readonly Dictionary<string, (Question Question, DateTime Timestamp)> _cache = new();
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

    public Question? GetCachedQuestion(string cacheKey)
    {
        if (_cache.TryGetValue(cacheKey, out var cachedItem))
        {
            // Check if cache entry is still valid
            if (DateTime.UtcNow - cachedItem.Timestamp < _cacheExpiration)
            {
                return cachedItem.Question;
            }

            // Remove expired entry
            _cache.Remove(cacheKey);
        }

        return null;
    }

    public void CacheQuestion(string cacheKey, Question question)
    {
        _cache[cacheKey] = (question, DateTime.UtcNow);

        // Clean up old entries (simple cleanup strategy)
        if (_cache.Count > 100)
        {
            var oldEntries = _cache
                .Where(kvp => DateTime.UtcNow - kvp.Value.Timestamp >= _cacheExpiration)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in oldEntries)
            {
                _cache.Remove(key);
            }
        }
    }

    public string BuildCacheKey(string difficulty, string? lastQuestion = null)
    {
        return $"question_{difficulty}_{lastQuestion ?? "none"}";
    }
}
