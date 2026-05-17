using Repository.Entities;
using Service.Interfaces;

namespace Service.Services
{
    /// <summary>
    /// Service responsible for classifying songs into categories 
    /// using a Multinomial Naive Bayes algorithm.
    /// </summary>
    public class ClassificationService(IClassificationDataCache cache) : IClassificationService
    {
        private readonly IClassificationDataCache _cache = cache;

        /// <summary>
        /// Predicts the most likely category for a given list of tags.
        /// </summary>
        public Category PredictCategory(Dictionary<Tag, int> songTags)
        {
            // Validation: If no tags are provided, we cannot classify
            if (songTags == null || !songTags.Any())
                return null;

            var categoryScores = new Dictionary<Category, double>();

            // Calculate the probability for each category in the system
            foreach (var category in _cache.AllCategories)
            {
                // Step 1: Start with Log(Prior) -> Log(P(Category))
                double score = CalculatePriorLogProbability(category);

                // Step 2: Add Log(Likelihood) -> Sum of Log(P(Tag | Category))
                score += CalculateLikelihoodLogProbability(category, songTags);

                categoryScores[category] = score;
            }

            // Step 3: Return the category with the highest probability score
            return GetBestMatch(categoryScores);
        }

        private double CalculatePriorLogProbability(Category category)
        {
            if (_cache.TotalSongs == 0) return 0;
            double prior = (double)category.Songs.Count / _cache.TotalSongs;
            return Math.Log(prior);
        }


        private double CalculateLikelihoodLogProbability(Category category, Dictionary<Tag, int> tagsWithCounts)
        {
            double likelihoodSum = 0;
            _cache.CategoryTagCounts.TryGetValue(category.CategoryID, out var tagsInCat);

            int totalWordsInCat = tagsInCat?.Values.Sum() ?? 0;
            double denominator = totalWordsInCat + _cache.VocabularySize;

            foreach (var kvp in tagsWithCounts)
            {
                Tag tag = kvp.Key;
                int countInCurrentSong = kvp.Value;

                int frequencyInDb = 0;
                tagsInCat?.TryGetValue(tag.TagID, out frequencyInDb);

                // Calculate standard log probability for a single word occurrence
                double wordLogProbability = Math.Log((frequencyInDb + 1.0) / denominator);

                // Weight the probability by multiplying it by how many times the word appeared in this song
                likelihoodSum += (wordLogProbability * countInCurrentSong);
            }

            return likelihoodSum;
        }

        private Category? GetBestMatch(Dictionary<Category, double> scores)
        {
            if (scores == null || !scores.Any())
                return null;

            // Select the key (Category) associated with the maximum value (Score)
            return scores.OrderByDescending(kvp => kvp.Value)
                         .Select(kvp => kvp.Key)
                         .FirstOrDefault();
        }
    }
}
