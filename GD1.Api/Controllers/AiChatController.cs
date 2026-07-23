using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using MediatR;
using GD1.Application.Features.Vehicle.Queries;

namespace GD1.Api.Controllers
{
    [Route("api/aichat")]
    [ApiController]
    public class AiChatController : ControllerBase
    {
        private readonly Kernel _kernel;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly StackExchange.Redis.IConnectionMultiplexer _redis;
        private readonly IMediator _mediator;

        public AiChatController(
            Kernel kernel, 
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration,
            StackExchange.Redis.IConnectionMultiplexer redis,
            IMediator mediator)
        {
            _kernel = kernel;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _redis = redis;
            _mediator = mediator;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskQuestion([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest(new { reply = "Message cannot be empty." });

            try
            {
                long userId = 0;
                var userIdClaim = User?.FindFirst("userId")?.Value 
                    ?? User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                    ?? User?.FindFirst("sub")?.Value;
                if (long.TryParse(userIdClaim, out var parsedId)) {
                    userId = parsedId;
                }

                // Retrieve message history from Redis using User ID (or IP address if guest)
                var db = _redis.GetDatabase();
                var redisKey = userId > 0 ? $"chat_history:{userId}" : $"chat_history:guest:{Request.HttpContext.Connection.RemoteIpAddress}";
                
                var historyJson = await db.StringGetAsync(redisKey);
                var messages = new List<RedisChatMessage>();
                if (!string.IsNullOrEmpty(historyJson))
                {
                    messages = JsonSerializer.Deserialize<List<RedisChatMessage>>(historyJson) ?? new List<RedisChatMessage>();
                }

                // If user requests a clean reset
                if (request.Message.Equals("/clear", StringComparison.OrdinalIgnoreCase))
                {
                    await db.KeyDeleteAsync(redisKey);
                    return Ok(new { reply = "Conversation history cleared." });
                }

                // Construct full context string (previous 3 turns) to accurately classify intent
                var contextBuilder = new StringBuilder();
                foreach (var msg in messages.TakeLast(6))
                {
                    contextBuilder.AppendLine($"{msg.Role}: {msg.Content}");
                }
                contextBuilder.AppendLine($"user: {request.Message}");

                var intent = await DetectIntentAsync(contextBuilder.ToString());

                if (intent == "SEARCH_LOTS")
                {
                    // Fetch user's registered vehicles directly to avoid tool-calling failure/leaks
                    var vehiclesList = new List<string>();
                    bool hasVehicles = false;
                    if (userId > 0)
                    {
                        var vehicleResponse = await _mediator.Send(new GetMyVehiclesQuery { OwnerId = userId });
                        if (vehicleResponse.IsSuccess && vehicleResponse.Data != null)
                        {
                            var vehicles = vehicleResponse.Data.ToList();
                            hasVehicles = vehicles.Any();
                            vehiclesList = vehicles.Select(v => $"{v.Brand} {v.Model} (ID: {v.Id})").ToList();
                        }
                    }

                    var vehiclesContext = hasVehicles 
                        ? string.Join(", ", vehiclesList) 
                        : "None (The user has no registered vehicles)";

                    var chatService = _kernel.GetRequiredService<IChatCompletionService>();
                    var chatHistory = new ChatHistory();
                    
                    chatHistory.AddSystemMessage(
                        $"You are Lara, GD1's warm and professional virtual assistant. Your job is to help users find suitable parking spaces.\n\n" +
                        $"USER VEHICLES CONTEXT:\n" +
                        $"The logged-in user owns the following vehicles: {vehiclesContext}.\n\n" +
                        $"RULES:\n" +
                        $"1. NEVER mention the word 'ID' (e.g., 'vehicle ID' or 'user ID') to the user. Talk like a real human.\n" +
                        $"2. If the user has no registered vehicles (context is 'None'), politely tell them you couldn't find any registered cars in their profile. Inform them they need to add a vehicle first, and ALWAYS format the link to add a vehicle as a Markdown link: **[Add a Vehicle](/add-vehicle)**.\n" +
                        $"3. If they do have registered vehicles, list them conversationally (e.g. 'I see you have a Porsche 911 registered. Would you like to park this car today?') and wait for their confirmation before calling the `search_lots` tool.\n" +
                        $"4. When presenting parking lot options:\n" +
                        $"   - If the user HAS NO registered vehicles, the property names MUST link to the add-vehicle page: '**[Property Name](/add-vehicle)**'.\n" +
                        $"   - If the user HAS registered vehicles, the property names MUST link to the detailed page: '**[Property Name](/property/{{id}})**'.\n" +
                        $"5. NEVER output any XML-like tags, '<details>' elements, or raw URL links. Present the search results strictly using the following bulleted format:\n" +
                        $"   - **[1. Safe Park Kakkanchery](/property/10)** (or `/add-vehicle` if no vehicles)\n" +
                        $"     - *Address:* Opposite Puthanathani Temple, Kakkanchery, Kerala\n" +
                        $"     - *Price:* ₹250.00/day\n" +
                        $"     - *Rating:* 4.50/5\n" +
                        $"     - *Available Slots:* A-201, B-202\n" +
                        $"6. Keep responses clean, warm, and professional. Avoid raw technical dumps."
                    );

                    // Add historical conversation flow to Semantic Kernel
                    foreach (var msg in messages)
                    {
                        if (msg.Role == "user") chatHistory.AddUserMessage(msg.Content);
                        else if (msg.Role == "assistant") chatHistory.AddAssistantMessage(msg.Content);
                    }

                    // Add current message
                    chatHistory.AddUserMessage(request.Message);

#pragma warning disable SKEXP0001
                    var settings = new OpenAIPromptExecutionSettings
                    {
                        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                    };
#pragma warning restore SKEXP0001

                    var result = await chatService.GetChatMessageContentAsync(
                        chatHistory,
                        executionSettings: settings,
                        kernel: _kernel
                    );

                    // Save updated context back to Redis
                    messages.Add(new RedisChatMessage { Role = "user", Content = request.Message });
                    messages.Add(new RedisChatMessage { Role = "assistant", Content = result.Content ?? string.Empty });
                    
                    // Keep history tidy (last 20 messages / 10 turns max)
                    if (messages.Count > 20)
                    {
                        messages = messages.Skip(messages.Count - 20).ToList();
                    }
                    await db.StringSetAsync(redisKey, JsonSerializer.Serialize(messages), TimeSpan.FromHours(1));

                    var actions = await BuildActionsFromSearchAsync(request.Message);

                    return Ok(new
                    {
                        reply = result.Content,
                        actions = actions
                    });
                }

                // Fall back to general RAG knowledge base for general questions
                var ragResult = await HandleRagAsync(request.Message);
                
                if (ragResult is OkObjectResult okResult)
                {
                    // Serialize RAG response to history too
                    using var doc = JsonDocument.Parse(JsonSerializer.Serialize(okResult.Value));
                    if (doc.RootElement.TryGetProperty("reply", out var replyProp))
                    {
                        messages.Add(new RedisChatMessage { Role = "user", Content = request.Message });
                        messages.Add(new RedisChatMessage { Role = "assistant", Content = replyProp.GetString() ?? string.Empty });
                        if (messages.Count > 20) messages = messages.Skip(messages.Count - 20).ToList();
                        await db.StringSetAsync(redisKey, JsonSerializer.Serialize(messages), TimeSpan.FromHours(1));
                    }
                }

                return ragResult;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { reply = "Something went wrong. Please try again." });
            }
        }

        private async Task<string> DetectIntentAsync(string message)
        {
            var client = _httpClientFactory.CreateClient();
            var groqKey = _configuration["AI:GroqApiKey"];
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", groqKey);

            var body = JsonSerializer.Serialize(new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "You are an intent classifier. Reply with ONLY one of these words — nothing else:\n" +
                                  "SEARCH_LOTS — if user wants to find, search, or browse parking/garage spaces\n" +
                                  "GENERAL — for everything else"
                    },
                    new { role = "user", content = message }
                },
                max_tokens = 10,
                temperature = 0
            });

            var response = await client.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                new StringContent(body, Encoding.UTF8, "application/json")
            );

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var intent = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()?.Trim().ToUpper() ?? "GENERAL";

            return intent.Contains("SEARCH_LOTS") ? "SEARCH_LOTS" : "GENERAL";
        }

        private async Task<List<object>> BuildActionsFromSearchAsync(string message)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var baseUrl = _configuration["App:BaseUrl"]
                    ?? "https://gd1-grand-auto-depot-one-9ms1.onrender.com";

                var keyword = message.Replace("find", "").Replace("search", "")
                    .Replace("parking", "").Replace("near", "").Replace("lot", "").Trim();

                var response = await client.GetAsync(
                    $"{baseUrl}/api/franchise/search?location={keyword}&pageSize=5");

                if (!response.IsSuccessStatusCode) return new List<object>();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var actions = new List<object>();
                if (doc.RootElement.TryGetProperty("data", out var data))
                {
                    foreach (var item in data.EnumerateArray())
                    {
                        var id = item.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0;
                        var name = item.TryGetProperty("name", out var nameProp)
                            ? nameProp.GetString() : "View Lot";

                        actions.Add(new { label = name, url = $"/property/{id}" });
                    }
                }
                return actions;
            }
            catch
            {
                return new List<object>();
            }
        }

        private async Task<IActionResult> HandleRagAsync(string message)
        {
            var jinaKey = _configuration["AI:JinaApiKey"];
            var pineconeKey = _configuration["AI:PineconeApiKey"];
            var pineconeHost = _configuration["AI:PineconeIndexHost"];
            var groqKey = _configuration["AI:GroqApiKey"];
            var client = _httpClientFactory.CreateClient();

            var questionEmbedding = await GetEmbeddingAsync(client, jinaKey, message);

            var queryBody = JsonSerializer.Serialize(new
            {
                vector = questionEmbedding,
                topK = 3,
                includeMetadata = true
            });

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Api-Key", pineconeKey);
            var queryResponse = await client.PostAsync(
                $"{pineconeHost}/query",
                new StringContent(queryBody, Encoding.UTF8, "application/json")
            );
            var queryResult = await queryResponse.Content.ReadAsStringAsync();

            using var queryDoc = JsonDocument.Parse(queryResult);
            var contextBuilder = new StringBuilder();
            foreach (var match in queryDoc.RootElement.GetProperty("matches").EnumerateArray())
            {
                if (match.TryGetProperty("metadata", out var meta) &&
                    meta.TryGetProperty("text", out var text))
                    contextBuilder.AppendLine(text.GetString());
            }

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", groqKey);

            var groqBody = JsonSerializer.Serialize(new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = $"You are Lara, GD1's assistant. Answer using ONLY this context:\n\n{contextBuilder}\n\n" +
                                  "If the answer is not in the context say: 'I don't have that information. Please contact support.'"
                    },
                    new { role = "user", content = message }
                },
                temperature = 0.5,
                max_tokens = 512
            });

            var groqResponse = await client.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                new StringContent(groqBody, Encoding.UTF8, "application/json")
            );

            var groqResult = await groqResponse.Content.ReadAsStringAsync();
            using var groqDoc = JsonDocument.Parse(groqResult);
            var replyText = groqDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return Ok(new { reply = replyText, actions = new List<object>() });
        }

        private async Task<float[]> GetEmbeddingAsync(HttpClient client, string jinaKey, string text)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jinaKey);

            var body = JsonSerializer.Serialize(new
            {
                model = "jina-embeddings-v2-base-en",
                input = new[] { text }
            });

            var response = await client.PostAsync(
                "https://api.jina.ai/v1/embeddings",
                new StringContent(body, Encoding.UTF8, "application/json")
            );

            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            var values = new List<float>();
            foreach (var val in doc.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding")
                .EnumerateArray())
                values.Add(val.GetSingle());

            return values.ToArray();
        }

        [HttpPost("seed")]
        public async Task<IActionResult> SeedKnowledgeBase()
        {
            var documents = new List<string>
            {
                "GD1 Grand Auto Depot One is a comprehensive vehicle storage, maintenance, and valet management platform.",
                "Pricing starts from Rs 2500 per month for standard outdoor spaces and Rs 6000 for premium indoor bays.",
                "To book, search for available lots, select a space, choose Self Drop-off or Valet Pickup, and pay.",
                "Valet Pickup means a GD1 agent comes to your location and drives your vehicle to the lot.",
                "Vehicle owners must upload a valid driving license and vehicle registration certificate for KYC.",
                "Lot owners can list properties by submitting a garage application verified within 48 hours.",
                "Maintenance services include car washing, detailing, battery checks, oil changes, and tire maintenance.",
                "Payments are processed via Razorpay. Advance payment is required to confirm a reservation.",
                "GD1 generates a legally binding parking agreement PDF for every booking automatically.",
                "Facilities have 24/7 HD surveillance cameras, secure access control, and dedicated security.",
                "Cancellations 24 hours before start date get a full refund. Within 24 hours get 50% or free reschedule.",
                "User roles include Vehicle Owner, Lot Owner, Lot Manager, Agent, Service Center, and Admin.",
                "Lot Managers handle vehicle arrivals, departures, and maintenance tasks at the facility.",
                "Agents perform valet pickup tasks, travelling to owner locations to transport vehicles safely.",
                "For support, use the in-app chat to communicate with your lot manager or contact Help Centre.",
                "GD1 backend uses .NET 8 with Clean Architecture, CQRS and MediatR. Frontend uses React and Vite.",
                "To partner as a lot owner or service center, fill the application form in the Partner With Us section.",
                "GD1 uses Photon Geocoding for real-time address auto-completion during valet pickup booking.",
                "Real-time notifications are sent for booking approvals, arrivals, payments, and maintenance updates."
            };

            var jinaKey = _configuration["AI:JinaApiKey"];
            var pineconeKey = _configuration["AI:PineconeApiKey"];
            var pineconeHost = _configuration["AI:PineconeIndexHost"];
            var client = _httpClientFactory.CreateClient();
            var upsertVectors = new List<object>();

            for (int i = 0; i < documents.Count; i++)
            {
                var embedding = await GetEmbeddingAsync(client, jinaKey, documents[i]);
                upsertVectors.Add(new
                {
                    id = $"doc-{i}",
                    values = embedding,
                    metadata = new { text = documents[i] }
                });
            }

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Api-Key", pineconeKey);
            var upsertBody = JsonSerializer.Serialize(new { vectors = upsertVectors });
            var upsertResponse = await client.PostAsync(
                $"{pineconeHost}/vectors/upsert",
                new StringContent(upsertBody, Encoding.UTF8, "application/json")
            );
            var upsertResult = await upsertResponse.Content.ReadAsStringAsync();

            return Ok(new { message = $"Seeded {documents.Count} documents.", result = upsertResult });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }

    public class RedisChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}