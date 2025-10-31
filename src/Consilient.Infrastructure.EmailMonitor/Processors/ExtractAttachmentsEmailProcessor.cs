using MailKit;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Consilient.Infrastructure.EmailMonitor.Processors
{
    internal class ExtractAttachmentsEmailProcessor : EmailProcessorBase
    {
        private readonly ExtractAttachmentsEmailProcessorConfiguration _configuration;
        private readonly ILogger _logger;

        public ExtractAttachmentsEmailProcessor(
            ExtractAttachmentsEmailProcessorConfiguration configuration,
            ILogger<ExtractAttachmentsEmailProcessor> logger
        )
        {
            _configuration = configuration;
            _logger = logger;

            // Ensure the save path is not null or empty to prevent runtime errors.
            if (string.IsNullOrWhiteSpace(_configuration.SavePath))
            {
                throw new ArgumentException("SavePath cannot be null or empty.", nameof(configuration));
            }
        }

        protected override bool MustProcessEmail(IMessageSummary message)
        {
            return message.Attachments.Any(a => a is not null && FilterBasedOnConfiguration(a, _configuration));
        }

        protected override async Task ProcessEmail(IMessageSummary message)
        {
            var docAttachments = message.Attachments
                .Where(a => FilterBasedOnConfiguration(a, _configuration));

            var folder = message.Folder;
            var attachmentSavePath = _configuration.SavePath;

            Directory.CreateDirectory(attachmentSavePath);

            foreach (var attachment in docAttachments)
            {
                if (await folder.GetBodyPartAsync(message.UniqueId, attachment) is not MimePart part)
                {
                    _logger.LogWarning("Could not retrieve body part for attachment on message {MessageId}.", message.UniqueId);
                    continue;
                }

                var fileName = attachment.FileName ?? $"attachment_{Guid.NewGuid()}.dat";
                var sanitizedFileName = $"{message.UniqueId}_{Path.GetFileName(fileName)}";
                var fullPath = Path.Combine(attachmentSavePath, sanitizedFileName);

                _logger.LogInformation("Saving attachment to {Path}", fullPath);

                await using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                await part.Content.DecodeToAsync(stream);
            }
        }

        private static bool FilterBasedOnConfiguration(BodyPartBasic attachment, ExtractAttachmentsEmailProcessorConfiguration configuration)
        {
            // Check if the attachment's content type matches any in the filter.
            if (configuration.ContentTypeFilter.Any(ct => attachment.ContentType.MimeType.Equals(ct, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Check if the attachment's file extension matches any in the filter.
            return attachment.FileName != null && configuration.ExtensionFilter.Any(ext => attachment.FileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }
    }
}
