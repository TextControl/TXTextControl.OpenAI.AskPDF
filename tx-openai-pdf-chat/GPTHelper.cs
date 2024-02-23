using System.Net.Http.Headers;
using System.Text;
using TXTextControl.OpenAI;

namespace tx_openai_pdf_chat
{
	public static class GPTHelper
	{
		// create a static HttpClient
		private static readonly HttpClient _client = new HttpClient
		{
			DefaultRequestHeaders =
		{
			Authorization = new AuthenticationHeaderValue("Bearer", Constants.OPENAI_API_KEY)
		}
		};

		public static List<string> GetKeywords(string text, int numKeywords = 10)
		{
			// create a list to store the keywords
			List<string> keywords = new List<string>();

			string prompt = $"Create {numKeywords} keywords and synonyms from the following question that can be used to find information in a larger text. Create only 1 word per keyword. Return the keywords in lowercase only. Here is the question: {text}";

			// create a request object
			Request apiRequest = new Request
			{
				Messages = new[]
				{
					new RequestMessage
					{
						Role = "system",
						Content = $"Always provide {numKeywords} keywords that include relevant synonyms of words in the original question."
					},
					new RequestMessage
					{
						Role = "user",
						Content = prompt
					}
				},
				Functions = new[]
				{
					new Function
					{
						Name = "get_keywords",
						Description = "Use this function to give the user a list of keywords.",
						Parameters = new Parameters
						{
							Type = "object",
							Properties = new Properties
							{
								List = new ListProperty
								{
									Type = "array",
									Items = new Items
									{
										Type = "string",
										Description = "A keyword"
									},
									Description = "A list of keywords"

								}
							}
							
						},
						Required = new List<string> { "list" }
					}
				},
				FunctionCall = new FunctionCall
				{
					Name = "get_keywords",
					Arguments = "{'list'}"
				}
			};

			// get the response
			if (GetResponse(apiRequest) is Response response)
			{
				// return the keywords
				return System.Text.Json.JsonSerializer.Deserialize<ListReturnObject>(response.Choices[0].Message.FunctionCall.Arguments).List;
			}

			return null;
		}

		public static string GetAnswer(string chunk, string question)
		{
			// create a prompt
			string prompt = $"```{chunk}```Your source is the information above. What is the answer to the following question? ```{question}```";

			// create a request object
			Request apiRequest = new Request
			{
				Messages = new[]
				{
					new RequestMessage
					{
						 Role = "system",
						 Content = "You should help to find an answer to a question in a document."
					},
					new RequestMessage
					{
						 Role = "user",
						 Content = prompt
					}
				}
			};

			// get the response
			if (GetResponse(apiRequest) is Response response)
			{
				// return the answer
				return response.Choices[0].Message.Content;
			}

			// return null if the response is null
			return null;
		}

		// get the response from the OpenAI API
		private static Response GetResponse(Request apiRequest)
		{
			// create a StringContent object
			StringContent content = new StringContent(
				System.Text.Json.JsonSerializer.Serialize(apiRequest),
				Encoding.UTF8,
				"application/json"
			);

			// send the request
			HttpResponseMessage httpResponseMessage = _client.PostAsync(
				"https://api.openai.com/v1/chat/completions",
				content
			).Result;

			// check if the request was successful
			if (httpResponseMessage.IsSuccessStatusCode)
			{
				// return the response
				return System.Text.Json.JsonSerializer.Deserialize<Response>(
					httpResponseMessage.Content.ReadAsStringAsync().Result
				);
			}

			// return null if the request was not successful
			return default;
		}
	}

}
