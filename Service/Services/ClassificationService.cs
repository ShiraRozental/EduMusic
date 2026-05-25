using Repository.Entities;
using Service.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Service.Services
{
    /// <summary>
    /// Service responsible for classifying songs into categories 
    /// using a Multinomial Naive Bayes algorithm.
    /// 
    /// The algorithm answers: "Given these tags, which category is most likely?"
    /// Formula per category:
    ///   score = log P(Category) + SUM[ count(tag) * log P(tag | Category) ]
    /// </summary>
    public class ClassificationService(IClassificationDataCache cache) : IClassificationService
    {
        private readonly IClassificationDataCache _cache = cache;

        /// <summary>
        /// Predicts the most likely category for a given list of tags.
        /// </summary>
        /// <param name="songTags">
        ///   Dictionary where Key = Tag entity and Value = how many times
        ///   that tag appears in the current song.
        /// </param>
        /// <returns>The predicted Category, or null if no tags were supplied.</returns>
        public Category? PredictCategory(Dictionary<Tag, int> songTags, int adminId)
        {
            // Validation: If no tags are provided, we cannot classify
            if (songTags == null || !songTags.Any())
                return null;

            var relevantCategories = _cache.LeafCategories
                .Where(c => c.AdminID == null || c.AdminID == adminId)
                .ToList();

            var categoryScores = new Dictionary<Category, double>();

            // Calculate the probability for each category in the system
            foreach (var category in relevantCategories)
            {
                // Step 1:log P(Category)
                double score = CalculatePriorLogProbability(category, relevantCategories.Count);

                // Step 2: SUM[ count(tag) * log P(tag | Category) ]
                score += CalculateLikelihoodLogProbability(category, songTags);

                categoryScores[category] = score;
            }

            // Step 3: Return the category with the highest probability score
            return GetBestMatch(categoryScores);
        }


        // ── PRIVATE HELPERS ──────────────────────────────────────────────────
        /// <summary>
        /// Computes log P(Category) using Laplace smoothing.
        /// Formula: log( (songs_in_category + 1) / (total_songs + num_relevant_categories) )
        /// </summary>
        private double CalculatePriorLogProbability(Category category, int relevantCategoriesCount)
        {
            if (_cache.TotalSongs == 0) return 0;

            _cache.SongsPerCategory.TryGetValue(category.CategoryID, out int songsInCategory);

            //Laplace smoothing to avoid prior = 0 for empty categories
            double prior = (songsInCategory + 1.0) / (_cache.TotalSongs + relevantCategoriesCount);

            return Math.Log(prior);
        }

        /// <summary>
        /// Computes SUM[ count(t) * log P(t | Category) ] over all tags in the song.
        ///
        ///                  freq(t, Category) + 1
        ///   P(t | C)  =   ─────────────────────────────────────
        ///                  totalWords(Category) + VocabularySize
        /// </summary>
        private double CalculateLikelihoodLogProbability(Category category, Dictionary<Tag, int> tagsWithCountsFromSong)
        {
            double likelihoodSum = 0;
            // tagsInCat: TagID -> total frequency across all songs in this category
            _cache.CategoryTagCounts.TryGetValue(category.CategoryID, out var tagsInCat);

            // Total number of tag occurrences in this category
            int totalWordsInCat = tagsInCat?.Values.Sum() ?? 0;
            // Denominator shared by all tags in this category.
            double denominator = totalWordsInCat + _cache.VocabularySize;

            foreach (var kvp in tagsWithCountsFromSong)
            {
                Tag tag = kvp.Key;
                int countInCurrentSong = kvp.Value;

                int frequencyInDb = 0;
                //tagsInCat - TagID -> Frequency
                tagsInCat?.TryGetValue(tag.TagID, out frequencyInDb);

                // Calculate standard log probability for a single word occurrence
                //                  freq(t, Category) + 1
                //   P(t | C)  =   ───────────────────────────────────
                //                  totalWords(Category) + VocabularySize
                double wordLogProbability = Math.Log((frequencyInDb + 1.0) / denominator);

                // Weight the probability by multiplying it by how many times the word appeared in this song
                // ── count(t) * log P(t | Category)  ──────────────────────────────
                likelihoodSum += (wordLogProbability * countInCurrentSong);
            }

            return likelihoodSum;
        }


        /// <summary>
        /// Returns the category with the highest log-probability score.
        /// </summary>
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
