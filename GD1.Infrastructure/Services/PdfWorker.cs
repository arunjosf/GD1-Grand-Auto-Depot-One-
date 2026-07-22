using Azure.Messaging.ServiceBus;
using GD1.Application.Interfaces.Services;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GD1.Infrastructure.Services
{
    public class PdfWorker : BackgroundService
    {
        private readonly ILogger<PdfWorker> _logger;
        private readonly string _connectionString;
        private readonly IServiceProvider _serviceProvider;
        private ServiceBusClient _client;
        private ServiceBusProcessor _processor;

        public PdfWorker(
            ILogger<PdfWorker> logger,
            IConfiguration config,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _connectionString = config["Azure:ServiceBusConnectionString"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                _logger.LogWarning("Service Bus connection string is missing. PdfWorker will not start.");
                return;
            }

            _client = new ServiceBusClient(_connectionString);
            _processor = _client.CreateProcessor("pdf-queue", new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1 
            });

            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;

            await _processor.StartProcessingAsync(stoppingToken);
            _logger.LogInformation("PdfWorker started listening to pdf-queue.");
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            _logger.LogInformation("Received message from queue: {Body}", body);

            try
            {
                var payload = JsonSerializer.Deserialize<PdfQueuePayload>(body);

                using var scope = _serviceProvider.CreateScope();
                var agreementRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<Agreement>>();
                var pdfService = scope.ServiceProvider.GetRequiredService<IPdfGeneratorService>();

                var agreement = await agreementRepo.GetByIdAsync(payload.AgreementId);

                if (agreement != null)
                {
                    var pdfBytes = pdfService.GenerateFromHtml(agreement.Content);
                    _logger.LogInformation("Successfully generated PDF for Agreement {Id}", agreement.Id);

                    var userRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<User>>();
                    var user = await userRepo.GetByIdAsync(agreement.UserId);

                    if (user != null)
                    {
                        var emailService = scope.ServiceProvider.GetRequiredService<GD1.Application.Interfaces.IEmailService>();
                        
                   

                        await emailService.SendWithAttachmentAsync(
                         user.Email,
                        "Your Official Parking Agreement",
                         $"Hello {user.FullName}, please find your signed agreement attached.",
                        pdfBytes,
                        $"Agreement_{agreement.Id}.pdf"
  );
                    }
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PDF message.");
                // If it fails, leave it in the queue to retry
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Service Bus Error in PdfWorker.");
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            if (_processor != null)
            {
                await _processor.StopProcessingAsync(stoppingToken);
                await _processor.DisposeAsync();
                await _client.DisposeAsync();
            }
            await base.StopAsync(stoppingToken);
        }
    }

    public class PdfQueuePayload
    {
        public long AgreementId { get; set; }
    }
}