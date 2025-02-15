using System.Net.NetworkInformation;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Rest;

namespace aidemo.Pages;

public class ImageAnalyzerModel : PageModel
{

    [BindProperty]
    public IFormFile Document { get; set; }

    public string Message { get; set; }
    public string ImageUrl { get; set; }
    public string ImageDetails { get; set; }
    public string Objects { get; set; }
    public string Tags { get; set; }
    public string Text { get; set; }
    public string ErrorMessage { get; set; }


    public async Task<IActionResult> OnPostAsync()
    {
        if (Document == null && Document.Length <= 0)
        {
            Message = "Please upload a valid document.";
            return Page();
        }
        if (IsInValidFileExtensions())
        {
            Message = "Only .jpeg,.jpg,.png files are allowed";
            return Page();
        }
        using (var memoryStream = new MemoryStream())
        {
            // Copy the uploaded document into the memory stream
            await Document.CopyToAsync(memoryStream);

            // Reset the stream's position to the beginning
            memoryStream.Position = 0;

            // Check if it's an image or a document based on the file extension or content type
            if (IsImage(Document))
            {
                // Process the image data in-memory
                var imageDataArray = memoryStream.ToArray();
                var imageData = BinaryData.FromBytes(imageDataArray);

                var base64Image = Convert.ToBase64String(imageDataArray);
                ImageUrl = $"data:image/jpeg;base64,{base64Image}";

                try
                {
                    // Optionally analyze the image without saving it
                    var imageDetails = await ImageAnalyser.AnalyzeImage(imageData);
                    var extractedTextFromImage = await ImageAnalyser.ExtractTextFromImage(imageData);
                    Message = $"Image {Document.FileName} analyzed successfully.";
                    ImageDetails = imageDetails.Item1;
                    Objects = "Objects : " + imageDetails.Item2;
                    Tags = "Tags : " + imageDetails.Item3;
                    Text = "OCR Text : " + extractedTextFromImage;
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(ComputerVisionOcrErrorException))
                    {
                        var exception = ex as ComputerVisionOcrErrorException;
                        ErrorObj? errorObj =
                            JsonSerializer.Deserialize<ErrorObj>(exception?.Response.Content);
                        ErrorMessage = errorObj?.error.message;
                    }
                }
            }
            else
            {
                Message = "Not a valid image file";
                return Page();
            }
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

    bool IsImage(IFormFile file)
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return imageExtensions.Contains(extension);
    }

    private bool IsInValidFileExtensions()
    {
        var supportedTypes = new[] { ".jpeg", ".jpg", ".png " };
        var documentExt = Path.GetExtension(Document.FileName).ToLower();

        return !supportedTypes.Contains(documentExt);
    }
}


