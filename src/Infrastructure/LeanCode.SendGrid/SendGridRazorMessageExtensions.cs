using System;
using System.IO;
using System.Threading.Tasks;
using SendGrid.Helpers.Mail;

namespace LeanCode.SendGrid
{
    public static class SendGridRazorMessageExtensions
    {
        public static SendGridRazorMessage WithSender(this SendGridRazorMessage message, EmailAddress emailAddress)
        {
            message.SetFrom(emailAddress);

            return message;
        }

        public static SendGridRazorMessage WithRecipient(this SendGridRazorMessage message, EmailAddress emailAddress)
        {
            message.AddTo(emailAddress);

            return message;
        }

        public static SendGridRazorMessage WithSubject(this SendGridRazorMessage message, string subject)
        {
            if (message is SendGridLocalizedRazorMessage localized)
            {
                localized.SetGlobalSubject(subject, Array.Empty<object>());
            }
            else
            {
                message.SetGlobalSubject(subject);
            }

            return message;
        }

        public static SendGridRazorMessage WithSubject(
            this SendGridRazorMessage message,
            string subject,
            params object[] formatArgs)
        {
            if (message is SendGridLocalizedRazorMessage localized)
            {
                localized.SetGlobalSubject(subject, formatArgs);
            }
            else
            {
                message.SetGlobalSubject(string.Format(subject, formatArgs));
            }

            return message;
        }

        public static SendGridRazorMessage WithPlainTextContent(this SendGridRazorMessage message, object model)
        {
            message.PlainTextContentModel = model;

            return message;
        }

        public static SendGridRazorMessage WithHtmlContent(this SendGridRazorMessage message, object model)
        {
            message.HtmlContentModel = model;

            return message;
        }

        public static SendGridRazorMessage WithAttachment(this SendGridRazorMessage message, Attachment attachment)
        {
            message.AddAttachment(attachment);

            return message;
        }

        public static SendGridRazorMessage WithAttachment(
            this SendGridRazorMessage message, string base64content, string fileName, string? mimeType)
        {
            message.AddAttachment(fileName, base64content, mimeType);

            return message;
        }

        public static async Task<SendGridRazorMessage> WithAttachmentAsync(
            this SendGridRazorMessage message, Stream content, string fileName, string? mimeType)
        {
            await message.AddAttachmentAsync(fileName, content, mimeType);

            return message;
        }

        public static async Task<SendGridRazorMessage> WithAttachmentAsync(
            this Task<SendGridRazorMessage> message, Stream content, string fileName, string? mimeType)
        {
            return await WithAttachmentAsync(await message, content, fileName, mimeType);
        }
    }
}
