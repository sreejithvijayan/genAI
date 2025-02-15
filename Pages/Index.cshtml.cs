using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aidemo.Pages;

public class IndexModel : PageModel
{

    [BindProperty]
    public IFormFile Document { get; set; }

    public string Message { get; set; }
    public string Sentiment { get; set; }
    public string Language { get; set; }
    public string KeyPhrases { get; set; }
    public string PiiInfo { get; set; }
    public string AbstractSummary { get; set; }


    public async Task<IActionResult> OnPostAsync()
    {
        if (Document == null && Document.Length <= 0)
        {
            Message = "Please upload a valid document.";
            return Page();
        }
        if (IsInValidFileExtensions())
        {
            Message = "Only .pdf files are allowed";
            return Page();
        }
        using (var memoryStream = new MemoryStream())
        {
            // Copy the uploaded document into the memory stream
            await Document.CopyToAsync(memoryStream);

            // Reset the stream's position to the beginning
            memoryStream.Position = 0;
            
            // Extract text from document using Azure Doc Intelligence SDK
            var extractedText = await DocumentAnalyser.ExtractTextFromDocument(memoryStream);

            // Perform the various analysis tasks
            var sentiment = await DocumentAnalyser.AnalyzeSentiment(extractedText);
            var language = await DocumentAnalyser.DetectLanguage(extractedText);
            var keyphrases = await DocumentAnalyser.ExtractKeyPhrases(extractedText);
            var piiEntities = await DocumentAnalyser.ExtractPiiInformation(extractedText);
            var summary = await DocumentAnalyser.ExtractAbstractSummaryFromRawText(extractedText);

            // Set the message with the analysis results
            Message = $"Document {Document.FileName} analyzed successfully.";
            Sentiment = "Sentiment : "+ sentiment;
            Language = "Language : " + language.Item1 +"("+ language.Item2 + ")";
            KeyPhrases = "Key Phrases: "+ keyphrases.Item1;
            PiiInfo = "Pii Entities :" + piiEntities;
            AbstractSummary = "Abstract : "+ summary;
        }
        #region commented old code
        //var filePath = Path.Combine(Directory.GetCurrentDirectory(), Document.FileName);
        //if (filePath.Contains("jpeg") || filePath.Contains("jpg") || filePath.Contains("png"))
        //{
        //    var fileName = Path.GetFileName(Document.FileName);
        //    var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        //    Directory.CreateDirectory(uploadsDirectory);
        //    var filePat = Path.Combine(uploadsDirectory, fileName);
        //    using(var fileStream = new FileStream(filePat,FileMode.Create))
        //    {
        //        await Document.CopyToAsync(fileStream);
        //    }
        //    using (var stream = new MemoryStream())
        //    {
        //        await Document.CopyToAsync(stream);
        //        stream.Position = 0;
        //        var imageDataArray = stream.ToArray();
        //        imageData = BinaryData.FromBytes(imageDataArray);
        //        ImageUrl = $"/uploads/{fileName}";

        //    }
        //    string imageDetails = await AnalyzeImage(imageData);
        //    Message = $"Image #{Document.FileName}' analyzed successfully. ::::: Image : {imageDetails}";
        //}
        //else
        //{
        //    using (var stream = new MemoryStream())
        //    {
        //        await Document.CopyToAsync(stream);
        //        stream.Position = 0;
        //    }
        //     extractedText = await ExtractTextFromDocument(filePath);
        //     sentiment = await AnalyzeSentiment(extractedText);
        //     language = await DetectLanguage(extractedText);
        //     keyphrases = await ExtractKeyPhrases(extractedText);
        //     piiEntities = await ExtractPiiInformation(extractedText);
        //     summary = await ExtractSummaryFromRawText(extractedText);
        //     abstractSummary = await ExtractAbstractSummaryFromRawText(extractedText);
        //     Message = "Document " + Document.FileName + " analyzed successfully. " + Environment.NewLine + Environment.NewLine + " Sentiment : " +  sentiment
        //        + Environment.NewLine + Environment.NewLine + " Language  : " + language  + Environment.NewLine + Environment.NewLine +
        //         "Key Phrases : " + keyphrases + Environment.NewLine + Environment.NewLine +
        //         "Pii Entities : " + piiEntities +
        //         Environment.NewLine + Environment.NewLine +
        //         "Summary : " + summary +
        //         Environment.NewLine + Environment.NewLine +
        //         "Abstract : " + abstractSummary;
        //}
        #endregion
        return Page();
    }

    private bool IsInValidFileExtensions()
    {
        var supportedTypes = new[] { ".pdf", ".doc", ".docx"};
        var documentExt = Path.GetExtension(Document.FileName).ToLower();

        return !supportedTypes.Contains(documentExt);
    }
}


