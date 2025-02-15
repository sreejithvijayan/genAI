using System;
using System.Text.Json;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace aidemo
{
	public static class ImageAnalyser
	{
        public static string imageAnalyticsEndpoint = Endpoints.imageAnalyticsEndpoint;
        public static string imageAnalysisKey = Endpoints.imageAnalyticsKey;
        public static async Task<(string, string, string)> AnalyzeImage(BinaryData imageData)
        {
            var credential = new AzureKeyCredential(imageAnalysisKey);
            var client = new ImageAnalysisClient(new Uri(imageAnalyticsEndpoint), credential);

            VisualFeatures visualFeatures =
            VisualFeatures.Caption |
            VisualFeatures.DenseCaptions |
            VisualFeatures.Objects |
            VisualFeatures.Read |
            VisualFeatures.Tags |
            VisualFeatures.People |
            VisualFeatures.SmartCrops;
            ImageAnalysisOptions options = new ImageAnalysisOptions
            {
                GenderNeutralCaption = true,
                Language = "en",
                SmartCropsAspectRatios = new float[] { 0.9F, 1.33F }
            };
            // Start analysis process.
            ImageAnalysisResult result = await client.AnalyzeAsync(imageData, visualFeatures, options);
            //await operation.WaitForCompletionAsync();


            Console.WriteLine();
            // View operation results.

            string summarizedText = "The object in the uploaded image is " + result.Caption.Text + " with a confidence score of " +
                result.Caption.Confidence * 100 + "%";

            List<string> objectsArray = new List<string>();
            foreach(var obj in result.Objects.Values)
            {
                foreach(var tag in obj.Tags)
                {
                    objectsArray.Add(tag.Name);
                }
            }
            string objects = string.Join(",", objectsArray.Distinct());

            List<string> tagsArray = new List<string>();
            foreach (var obj in result.Tags.Values)
            {
                tagsArray.Add(obj.Name);
            }
            string tags = string.Join(",", tagsArray.Distinct());

            return (summarizedText, objects, tags);
        }

        public static async Task<string> ExtractTextFromImage(BinaryData imageData)
        {
            var extractedText = string.Empty;
            try
            {
                ComputerVisionClient client =
                  new ComputerVisionClient(new ApiKeyServiceClientCredentials(imageAnalysisKey))
                  { Endpoint = imageAnalyticsEndpoint };

                // Read text from URL
                var textHeaders = await client.ReadInStreamAsync(imageData.ToStream());
                // After the request, get the operation location (operation ID)
                string operationLocation = textHeaders.OperationLocation;
                Thread.Sleep(2000);

                // Retrieve the URI where the extracted text will be stored from the Operation-Location header.
                // We only need the ID and not the full URL
                const int numberOfCharsInOperationId = 36;
                string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

                // Extract the text
                ReadOperationResult results;
                do
                {
                    results = await client.GetReadResultAsync(Guid.Parse(operationId));
                }
                while ((results.Status == OperationStatusCodes.Running ||
                    results.Status == OperationStatusCodes.NotStarted));

                var textUrlFileResults = results.AnalyzeResult.ReadResults;
                foreach (Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models.ReadResult page in textUrlFileResults)
                {
                    foreach (Line line in page.Lines)
                    {
                        extractedText += line.Text + Environment.NewLine;
                    }
                }
            }
            catch(Exception ex)
            {
                if(ex.GetType() == typeof(ComputerVisionOcrErrorException))
                {
                    var exception = ex as ComputerVisionOcrErrorException;
                    throw exception;
                }
            }
            return extractedText;
        }
    }
}

