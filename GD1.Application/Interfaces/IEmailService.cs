using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);

        Task SendWithAttachmentAsync(string to, string subject, string body, byte[] attachment, string attachmentFileName);
    }
}
