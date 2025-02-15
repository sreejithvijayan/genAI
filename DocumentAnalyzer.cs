using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.AI.TextAnalytics;

namespace aidemo
{
    public static class DocumentAnalyser
    {
        public static string formRecognizerEndpoint = Endpoints.formRecognizerEndpoint;
        public static string formRecognizerKey = Endpoints.formRecognizerKey;
        public static string textAnalyticsEndpoint = Endpoints.textAnalyticsEndpointEastUs;
        public static string textAnalyticsKey = Endpoints.textAnalyticsKey;
        public static async Task<string> ExtractTextFromDocument(MemoryStream stream)
        {
            var credential = new AzureKeyCredential(formRecognizerKey);
            var client = new DocumentAnalysisClient(new Uri(formRecognizerEndpoint), credential);
            //using var stream = new FileStream(documentPath, FileMode.Open);
            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", stream);
            AnalyzeResult result = operation.Value;
            string extractedText = "";
            foreach (var page in result.Pages)
            {
                foreach (var line in page.Lines)
                {
                    extractedText += line.Content + "\n";
                }
            }
            return extractedText;
        }

        public static async Task<string> ExtractTextFromDocumentFile(IFormFile file)
        {
            var credential = new AzureKeyCredential(formRecognizerKey);
            var client = new DocumentAnalysisClient(new Uri(formRecognizerEndpoint), credential);
            using (var memoryStream = new MemoryStream())
            {
                // Copy the uploaded document into the memory stream
                await file.CopyToAsync(memoryStream);

                // Reset the stream's position to the beginning
                memoryStream.Position = 0;
                AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", memoryStream);
                AnalyzeResult result = operation.Value;
                string extractedText = "";
                foreach (var page in result.Pages)
                {
                    foreach (var line in page.Lines)
                    {
                        extractedText += line.Content + "\n";
                    }
                }

                return extractedText;
            }
        }


        public static async Task<string> ExtractSummaryFromRawText(string documentPath)
        {
            var credential = new AzureKeyCredential(textAnalyticsKey);
            var client = new TextAnalyticsClient(new Uri(textAnalyticsEndpoint), credential);
            var batchInput = new List<string>
            {
                documentPath
            };

            TextAnalyticsActions actions = new TextAnalyticsActions()
            {
                ExtractiveSummarizeActions = new List<ExtractiveSummarizeAction>() { new ExtractiveSummarizeAction() }
            };
            AnalyzeActionsOperation operation = await client.AnalyzeActionsAsync(WaitUntil.Completed, batchInput, actions);
            var result = operation.Value;
            string extractedText = "";
            await foreach (AnalyzeActionsResult documentsInPage in result)
            {
                IReadOnlyCollection<ExtractiveSummarizeActionResult> summaryResults = documentsInPage.ExtractiveSummarizeResults;

                foreach (ExtractiveSummarizeActionResult summaryActionResults in summaryResults)
                {
                    foreach (ExtractiveSummarizeResult documentResults in summaryActionResults.DocumentsResults)
                    {
                        foreach (ExtractiveSummarySentence sentence in documentResults.Sentences)
                        {
                            extractedText += sentence.Text;
                        }
                    }
                }
            }
            return extractedText;
        }

        public static async Task<string> ExtractAbstractSummaryFromRawText(string documentPath)
        {
            var credential = new AzureKeyCredential(textAnalyticsKey);
            var client = new TextAnalyticsClient(new Uri(textAnalyticsEndpoint), credential);
            //string document = @"The extractive summarization feature uses natural language processing techniques to locate key sentences in an unstructured text document. 
            //        These sentences collectively convey the main idea of the document. This feature is provided as an API for developers. 
            //        They can use it to build intelligent solutions based on the relevant information extracted to support various use cases. 
            //        Extractive summarization supports several languages. It is based on pretrained multilingual transformer models, part of our quest for holistic representations. 
            //        It draws its strength from transfer learning across monolingual and harness the shared nature of languages to produce models of improved quality and efficiency.";
            var batchInput = new List<string>
        {
            documentPath
        };

            TextAnalyticsActions actions = new TextAnalyticsActions()
            {
                AbstractiveSummarizeActions = new List<AbstractiveSummarizeAction>() { new AbstractiveSummarizeAction() }
            };
            AnalyzeActionsOperation operation = await client.AnalyzeActionsAsync(WaitUntil.Completed, batchInput, actions);
            var result = operation.Value;
            string extractedText = "";
            await foreach (AnalyzeActionsResult documentsInPage in result)
            {
                IReadOnlyCollection<AbstractiveSummarizeActionResult> abstractiveummaryResults = documentsInPage.AbstractiveSummarizeResults;

                foreach (AbstractiveSummarizeActionResult summaryActionResults in abstractiveummaryResults)
                {
                    foreach (AbstractiveSummarizeResult documentResults in summaryActionResults.DocumentsResults)
                    {
                        foreach (AbstractiveSummary sentence in documentResults.Summaries)
                        {
                            extractedText += sentence.Text;
                        }
                    }
                }
            }
            return extractedText;
        }

        public static async Task<string> AnalyzeSentiment(string extractedText)
        {
            var credential = new AzureKeyCredential(textAnalyticsKey);
            var client = new TextAnalyticsClient(new Uri(textAnalyticsEndpoint), credential);

            //var documents = new List<TextDocumentInput>() { new TextDocumentInput("1", document) };
            var batchInput = new List<string>
            {
                extractedText
            };
            //var options = new ExtractiveSummarizeOptions { MaxSentenceCount = 3 };

            TextAnalyticsActions actions = new TextAnalyticsActions()
            {
                ExtractiveSummarizeActions = new List<ExtractiveSummarizeAction>() { new ExtractiveSummarizeAction() }
            };

            string summarizedText = "";
            // Start analysis process.
            DocumentSentiment documentSentiment = await client.AnalyzeSentimentAsync(extractedText);
            //await operation.WaitForCompletionAsync();


            Console.WriteLine();
            // View operation results.

            summarizedText +=
                documentSentiment.Sentiment;

            return summarizedText;
        }

        public static async Task<(string, string)> DetectLanguage(string extractedText)
        {
            var credential = new AzureKeyCredential(textAnalyticsKey);
            var client = new TextAnalyticsClient(new Uri(textAnalyticsEndpoint), credential);

            TextAnalyticsActions actions = new TextAnalyticsActions()
            {
                ExtractiveSummarizeActions = new List<ExtractiveSummarizeAction>() { new ExtractiveSummarizeAction() }
            };

            // Start analysis process.
            DetectedLanguage detectedLanguage = await client.DetectLanguageAsync(extractedText);

            Console.WriteLine();
            // View operation results.

            return (detectedLanguage.Name, detectedLanguage.Iso6391Name);
        }

        

        public static async Task<(string, List<string>)> ExtractKeyPhrases(string extractedText)
        {
            var credential = new AzureKeyCredential(textAnalyticsKey);
            var client = new TextAnalyticsClient(new Uri(textAnalyticsEndpoint), credential);

            string keyphrases = "";
            Response<KeyPhraseCollection> response = await client.ExtractKeyPhrasesAsync(extractedText);
            KeyPhraseCollection keyPhrases = response.Value;
            HashSet<string> distinctKeyPhrases = new HashSet<string>();
            Console.WriteLine($"Extracted {keyPhrases.Count} key phrases:");
            foreach (string keyPhrase in keyPhrases)
            {
                string keyphraseText = keyPhrase;
                distinctKeyPhrases.Add(keyphraseText);
            }
            keyphrases = string.Join(",", distinctKeyPhrases);
            keyphrases = keyphrases.TrimEnd(',');
            return (keyphrases, keyPhrases.ToList());
        }

        public static async Task<string> ExtractPiiInformation(string extractedText)
        {
            var credential = new AzureKeyCredential(textAnalyticsKey);
            var client = new TextAnalyticsClient(new Uri(textAnalyticsEndpoint), credential);

            string piiEntitiesText = "";
            Response<PiiEntityCollection> response = await client.RecognizePiiEntitiesAsync(extractedText);
            PiiEntityCollection piiEntities = response.Value;
            HashSet<string> distinctPiiEntities = new HashSet<string>();

            Console.WriteLine($"Extracted {piiEntities.Count} pii entities:");
            foreach (PiiEntity piiEntity in piiEntities)
            {
                string entityText = piiEntity.Category + " : " + piiEntity.Text;
                distinctPiiEntities.Add(entityText);
            }
            piiEntitiesText = string.Join(",", distinctPiiEntities);
            return piiEntitiesText.TrimEnd(',');
        }
    }
}