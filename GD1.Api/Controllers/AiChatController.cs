using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GD1.Api.Controllers
{
    [Route("api/aichat")]
    [ApiController]
    public class AiChatController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AiChatController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // ─────────────────────────────────────────────
        // STEP 1: SEED - Upload GD1 Knowledge to Pinecone
        // Call this ONCE from Swagger/Postman: POST /api/aichat/seed
        // ─────────────────────────────────────────────
        [HttpPost("seed")]
        public async Task<IActionResult> SeedKnowledgeBase()
        {
            // GD1 Knowledge Base - Add all your business info here
            var documents = new List<string>
            {
                "GD1 Grand Auto Depot One is a comprehensive vehicle storage, maintenance, and valet management platform. It connects vehicle owners who need secure parking with property owners who have available parking spaces.",
                "Parking pricing at GD1 starts from Rs 2500 per month for standard outdoor spaces. Premium indoor climate-controlled bays are available from Rs 6000 per month. Daily and weekly plans are also available.",
                "To book a parking space on GD1, search for available lots by entering your location and preferred dates. Browse available lots, select your preferred space, choose between Self Drop-off or Valet Pickup, and complete the payment.",
                "GD1 offers two vehicle arrival options. Self Drop-off means you drive your vehicle to the parking lot yourself. Valet Pickup means a GD1 agent comes to your location and drives your vehicle to the lot on your behalf.",
                "Vehicle owners must complete KYC verification before booking. This includes uploading a valid driving license and vehicle registration certificate. Documents are verified using AI-powered OCR technology.",
                "Lot owners can list their properties on GD1 by submitting a garage application. The GD1 admin team will verify the property and onboard the lot within 48 hours.",
                "GD1 supports vehicle maintenance services while your car is in storage. Services include car washing, detailing, battery checks, oil changes, and tire maintenance. You can request these from your dashboard.",
                "Payments on GD1 are processed securely through Razorpay. Advance booking payments are required to confirm a reservation. Full settlement is done at the end of the storage period.",
                "GD1 provides a legally binding parking agreement generated automatically as a PDF for every booking. This agreement outlines the storage terms, pricing, duration, and responsibilities of both parties.",
                "GD1 facilities provide 24/7 security including HD surveillance cameras, secure access control, and dedicated security personnel. Vehicle owners can track their vehicle status in real time from their dashboard.",
                "Booking cancellations must be made at least 24 hours before the booking start date for a full refund. Cancellations within 24 hours are eligible for a 50% refund or a free reschedule.",
                "GD1 supports multiple user roles including Vehicle Owner, Lot Owner, Lot Manager, Agent, Service Center, and Admin. Each role has a dedicated dashboard with specific features.",
                "Lot Managers are assigned by Lot Owners to manage day-to-day operations at the parking facility. They handle vehicle arrivals, departures, and maintenance task updates.",
                "Agents are assigned by the GD1 admin team to perform valet pickup tasks. They travel to the vehicle owner's location and safely transport the vehicle to the assigned parking lot.",
                "For support, vehicle owners can use the GD1 in-app chat to communicate with their lot manager. They can also contact GD1 customer support through the Help Centre.",
                "GD1 is hosted on AWS using Docker containers. The backend is built on .NET 8 with C# using CQRS architecture with MediatR. The frontend is built with React and Vite.",
                "To partner with GD1 as a lot owner or service center, visit the Partner With Us section on the homepage and fill out the garage or service center application form.",
                "GD1 uses Photon Geocoding API powered by OpenStreetMap for real-time address auto-completion and spatial coordinate resolution during valet pickup booking.",
                "GD1 sends real-time notifications to users and lot owners for booking approvals, vehicle arrivals, payment confirmations, and maintenance updates."
            };

            var jinaKey = _configuration["AI:JinaApiKey"];
            var pineconeKey = _configuration["AI:PineconeApiKey"];
            var pineconeHost = _configuration["AI:PineconeIndexHost"];

            var client = _httpClientFactory.CreateClient();
            var upsertVectors = new List<object>();

            for (int i = 0; i < documents.Count; i++)
            {
                // Generate embedding for each document using Jina AI
                var embedding = await GetEmbeddingAsync(client, jinaKey, documents[i]);
                upsertVectors.Add(new
                {
                    id = $"doc-{i}",
                    values = embedding,
                    metadata = new { text = documents[i] }
                });
            }

            // Upsert all vectors into Pinecone
            var upsertBody = JsonSerializer.Serialize(new { vectors = upsertVectors });
            var upsertContent = new StringContent(upsertBody, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Api-Key", pineconeKey);

            var upsertResponse = await client.PostAsync($"{pineconeHost}/vectors/upsert", upsertContent);
            var upsertResult = await upsertResponse.Content.ReadAsStringAsync();

            return Ok(new { message = $"Successfully seeded {documents.Count} documents into Pinecone.", result = upsertResult });
        }

        // ─────────────────────────────────────────────
        // STEP 2: ASK - Full RAG Pipeline
        // Called by React frontend: POST /api/aichat/ask
        // ─────────────────────────────────────────────
        [HttpPost("ask")]
        public async Task<IActionResult> AskQuestion([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest(new { reply = "Message cannot be empty." });

            var jinaKey = _configuration["AI:JinaApiKey"];
            var pineconeKey = _configuration["AI:PineconeApiKey"];
            var pineconeHost = _configuration["AI:PineconeIndexHost"];
            var groqKey = _configuration["AI:GroqApiKey"];

            var client = _httpClientFactory.CreateClient();

            // ── R: RETRIEVAL ──────────────────────────
            // 1. Convert the user's question into a vector using Jina AI
            var questionEmbedding = await GetEmbeddingAsync(client, jinaKey, request.Message);

            // 2. Search Pinecone for the top 3 most relevant GD1 documents
            var queryBody = JsonSerializer.Serialize(new
            {
                vector = questionEmbedding,
                topK = 3,
                includeMetadata = true
            });

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Api-Key", pineconeKey);
            var queryContent = new StringContent(queryBody, Encoding.UTF8, "application/json");
            var queryResponse = await client.PostAsync($"{pineconeHost}/query", queryContent);
            var queryResult = await queryResponse.Content.ReadAsStringAsync();

            // 3. Extract retrieved text chunks from Pinecone response
            using var queryDoc = JsonDocument.Parse(queryResult);
            var contextBuilder = new StringBuilder();
            var matches = queryDoc.RootElement.GetProperty("matches");
            foreach (var match in matches.EnumerateArray())
            {
                if (match.TryGetProperty("metadata", out var metadata) &&
                    metadata.TryGetProperty("text", out var text))
                {
                    contextBuilder.AppendLine(text.GetString());
                }
            }
            var context = contextBuilder.ToString();

            // ── A: AUGMENTATION + G: GENERATION ──────
            // 4. Send the question + retrieved GD1 context to Groq
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", groqKey);

            var groqBody = JsonSerializer.Serialize(new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = $"You are the official customer support assistant for GD1 (Grand Auto Depot One). " +
                                  $"Answer the user's question using ONLY the context provided below. " +
                                  $"If the answer is not in the context, say: 'I don't have that information right now. Please contact our support team.' " +
                                  $"Be polite, concise, and professional.\n\n" +
                                  $"CONTEXT:\n{context}"
                    },
                    new
                    {
                        role = "user",
                        content = request.Message
                    }
                },
                temperature = 0.5,
                max_tokens = 512
            });

            var groqContent = new StringContent(groqBody, Encoding.UTF8, "application/json");
            var groqResponse = await client.PostAsync("https://api.groq.com/openai/v1/chat/completions", groqContent);

            if (!groqResponse.IsSuccessStatusCode)
            {
                var errorBody = await groqResponse.Content.ReadAsStringAsync();
                return StatusCode((int)groqResponse.StatusCode, new { reply = "AI service error.", detail = errorBody });
            }

            var groqResult = await groqResponse.Content.ReadAsStringAsync();
            using var groqDoc = JsonDocument.Parse(groqResult);
            var replyText = groqDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return Ok(new { reply = replyText });
        }

        // ─────────────────────────────────────────────
        // HELPER: Get Embedding from Jina AI
        // ─────────────────────────────────────────────
        private async Task<float[]> GetEmbeddingAsync(HttpClient client, string jinaKey, string text)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jinaKey);

            var body = JsonSerializer.Serialize(new
            {
                model = "jina-embeddings-v2-base-en",
                input = new[] { text }
            });

            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://api.jina.ai/v1/embeddings", content);
            var result = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(result);
            var embeddingArray = doc.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding");

            var values = new List<float>();
            foreach (var val in embeddingArray.EnumerateArray())
                values.Add(val.GetSingle());

            return values.ToArray();
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}