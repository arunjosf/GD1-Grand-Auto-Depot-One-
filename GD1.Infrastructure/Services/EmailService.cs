using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Application.Interfaces;

namespace GD1.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config) => _config = config;

        public async Task SendAsync(string to, string subject, string body)
        {
            Console.WriteLine(
                $"[EMAIL DEV]\nTo: {to}\nSubject: {subject}\nBody: {body}\n");
            await Task.CompletedTask;
        }
    }
}
