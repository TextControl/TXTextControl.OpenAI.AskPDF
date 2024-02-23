using tx_openai_pdf_chat;

string question = "Is contracting with other partners an option?";
//string question = "How will disputes be dealt with?";
//string question = "Can the agreement be changed or modified?";

string pdfPath = "Sample PDFs/SampleContract-Shuttle.pdf";

// load the PDF file
byte[] pdfDocument = File.ReadAllBytes(pdfPath);

// split the PDF document into chunks
var chunks = DocumentProcessing.Chunk(pdfDocument, 2500, 50);

Console.WriteLine($"{chunks.Count.ToString()} chunks generated from: {pdfPath}");

// get the keywords
List<string> generatedKeywords = GPTHelper.GetKeywords(question, 20);

// find the matches
var matches = DocumentProcessing.FindMatches(chunks, generatedKeywords).ToList().First();

// print the matches
Console.WriteLine($"The question: \"{question}\" was found in chunk {matches.Key}.");

// print the answer
Console.WriteLine("\r\n********\r\n" + GPTHelper.GetAnswer(chunks[matches.Key], question));