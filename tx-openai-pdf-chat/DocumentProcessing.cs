using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TXTextControl;

public static class StringExtensions
{
	// count the number of occurrences of a substring in a string
	public static int CountSubstring(this string text, string substring)
	{
		int count = 0;
		// find the first occurrence
		int index = text.IndexOf(substring, StringComparison.OrdinalIgnoreCase);
			
		// loop through the string to find the next occurrence
		while (index != -1)
		{
			count++;
			// find the next occurrence
			index = text.IndexOf(substring, index + 1, StringComparison.OrdinalIgnoreCase);
		}

		return count;
	}
}

public static class DocumentProcessing
{
	// find matches in a list of chunks
	public static Dictionary<int, double> FindMatches(List<string> chunks, List<string> keywords, int padding = 500)
	{
		// create a dictionary to store the document frequency of each keyword
		Dictionary<string, int> df = new Dictionary<string, int>();

		// create a dictionary to store the results
		Dictionary<int, double> results = new Dictionary<int, double>();

		// create a list to store the trimmed chunks
		List<string> trimmedChunks = new List<string>();

		// loop through the chunks
		for (int i = 0; i < chunks.Count; i++)
		{
			// remove the padding from the first and last chunk
			string chunk = i != 0 ? chunks[i].Substring(padding) : chunks[i];
			chunk = i != chunks.Count - 1 ? chunk.Substring(0, chunk.Length - padding) : chunk;
			trimmedChunks.Add(chunk.ToLower());
		}

		// loop through the trimmed chunks
		foreach (string chunk in trimmedChunks)
		{
			// loop through the keywords
			foreach (string keyword in keywords)
			{
				// count the occurrences of the keyword in the chunk
				int occurrences = chunk.CountSubstring(keyword);
				
				// add the keyword to the document frequency dictionary
				if (!df.ContainsKey(keyword))
				{
					df[keyword] = 0;
				}
				
				// increment the document frequency
				df[keyword] += occurrences;
			}
		}

		// loop through the trimmed chunks
		for (int chunkId = 0; chunkId < trimmedChunks.Count; chunkId++)
		{
			// initialize the points
			double points = 0;

			// loop through the keywords
			foreach (string keyword in keywords)
			{
				// count the occurrences of the keyword in the chunk
				int occurrences = trimmedChunks[chunkId].CountSubstring(keyword);
				
				// calculate the points
				if (df[keyword] > 0)
				{
					// add the points
					points += occurrences / (double)df[keyword];
				}
			}
			// add the points to the results
			results[chunkId] = points;
		}

		// return the results sorted by points
		return results.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
	}

	// split a PDF document into chunks
	public static List<string> Chunk(byte[] pdfDocument, int chunkSize, int overlap = 1)
	{
		// create a new ServerTextControl instance
		using (TXTextControl.ServerTextControl tx = new TXTextControl.ServerTextControl())
		{
			tx.Create();

			var loadSettings = new TXTextControl.LoadSettings
			{
				PDFImportSettings = TXTextControl.PDFImportSettings.GenerateParagraphs
			};

			// load the PDF document
			tx.Load(pdfDocument, TXTextControl.BinaryStreamType.AdobePDF, loadSettings);

			// remove line breaks
			string pdfText = tx.Text.Replace("\r\n", " ");

			List<string> chunks = new List<string>();

			// split the text into chunks
			while (pdfText.Length > chunkSize)
			{
				chunks.Add(pdfText.Substring(0, chunkSize));
				pdfText = pdfText.Substring(chunkSize - overlap);
			}

			// add the last chunk
			chunks.Add(pdfText);

			return chunks;
		}
	}

}
