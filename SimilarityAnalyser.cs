using System;
namespace aidemo
{
    public static class SimilarityAnalyser
    {
        // Compute Cosine Similarity between the key phrases of the resume and job description
        public static (double, List<string>) ComputeCosineSimilarity(List<string> resumePhrases, List<string> jobDescriptionPhrases)
        {
            var allPhrases = resumePhrases.Union(jobDescriptionPhrases).Distinct().ToList();
            var matches = resumePhrases.Intersect(jobDescriptionPhrases).Distinct().ToList();
            // Create vectors based on key phrase occurrence
            var resumeVector = CreateVector(resumePhrases, allPhrases);
            var jobDescriptionVector = CreateVector(jobDescriptionPhrases, allPhrases);

            return (CosineSimilarity(resumeVector, jobDescriptionVector), matches);
        }

        // Create vector representation based on the occurrence of phrases
        private static int[] CreateVector(List<string> phrases, List<string> allPhrases)
        {
            return allPhrases.Select(phrase => phrases.Contains(phrase) ? 1 : 0).ToArray();
        }

        // Calculate cosine similarity between two vectors
        private static double CosineSimilarity(int[] vectorA, int[] vectorB)
        {
            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                magnitudeA += Math.Pow(vectorA[i], 2);
                magnitudeB += Math.Pow(vectorB[i], 2);
            }

            if (magnitudeA == 0 || magnitudeB == 0)
            {
                return 0;
            }

            return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
        }
    }
}

