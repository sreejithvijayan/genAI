using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aidemo.Pages;

public class ResumeAnalyserModel : PageModel
{

    [BindProperty]
    public IFormFile ResumeFile { get; set; }
    [BindProperty]
    public IFormFile JobDescriptionFile { get; set; }

    public string Message { get; set; }
    public string ImageUrl { get; set; }


    public async Task<IActionResult> OnPostAsync()
    {
        if (ResumeFile == null)
        {
            Message = "Please upload a resume";
            return Page();
        }
        if (JobDescriptionFile == null)
        {
            Message = "Please upload a job description";
            return Page();
        }
        if(IsInValidFileExtensions())
        {
            Message = "Only .pdf, .doc and .docx files are allowed";
            return Page();
        }
        using (var memoryStream = new MemoryStream())
        {
            var resumeText = await DocumentAnalyser.ExtractTextFromDocumentFile(ResumeFile);
            var jobDescriptionText = await DocumentAnalyser.ExtractTextFromDocumentFile(JobDescriptionFile);

            var resumeKeyphrases = await DocumentAnalyser.ExtractKeyPhrases(resumeText);
            var jobDescriptionKeyphrases = await DocumentAnalyser.ExtractKeyPhrases(jobDescriptionText);

            var similarityScore =  SimilarityAnalyser.ComputeCosineSimilarity(resumeKeyphrases.Item2, jobDescriptionKeyphrases.Item2);
            var similarityPercentage = similarityScore.Item1 * 100;
            var matches = string.Join(",", similarityScore.Item2);
            

            // Set the message with the analysis results
            Message = $"Resume {ResumeFile.FileName} analyzed successfully."
                        + $"{Environment.NewLine}Similarity Score: {similarityPercentage}%"
                        + $"{Environment.NewLine}Matches: {matches}";

        }

        return Page();
    }

    private bool IsInValidFileExtensions()
    {
        var supportedTypes = new[] { ".pdf", ".doc", ".docx" };
        var resumeExt = Path.GetExtension(ResumeFile.FileName).ToLower();
        var jobDescriptionExt = Path.GetExtension(JobDescriptionFile.FileName).ToLower();

        return !supportedTypes.Contains(resumeExt) || !supportedTypes.Contains(jobDescriptionExt);
    }
}


