using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZlEmailProvider;

namespace RSS_Services
{
    public class SupportService
    {
        private static readonly HashSet<string> ValidCategories = new(StringComparer.OrdinalIgnoreCase)
        {
            "Bug", "Account", "Other"
        };

        private readonly IEmailService _emailService;
        private readonly SupportOptions _options;
        private readonly ILogger<SupportService> _logger;

        public SupportService(IEmailService emailService, IOptions<SupportOptions> options, ILogger<SupportService> logger)
        {
            _emailService = emailService;
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendSupportRequestAsync(string name, string email, string category, string message)
        {
            name = (name ?? string.Empty).Trim();
            email = (email ?? string.Empty).Trim();
            message = (message ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
                throw new ArgumentException("Please enter a valid name.");
            if (string.IsNullOrWhiteSpace(email) || email.Length > 256 || !email.Contains('@'))
                throw new ArgumentException("Please enter a valid email.");
            if (string.IsNullOrWhiteSpace(category) || !ValidCategories.Contains(category))
                throw new ArgumentException("Please select a valid category.");
            if (message.Length < 10 || message.Length > 5000)
                throw new ArgumentException("Message must be between 10 and 5000 characters.");

            if (string.IsNullOrWhiteSpace(_options.ToAddress))
            {
                _logger.LogWarning("Support:ToAddress is not configured; dropping support request from {Email}", email);
                throw new InvalidOperationException("Support inbox is not configured.");
            }

            var (subject, text, html) = SupportEmailTemplates.BuildSupportRequest(name, email, category, message);
            await _emailService.SendAsync(_options.ToAddress, subject, text, html);
            _logger.LogInformation("Support request sent from {Email}, category {Category}", email, category);
        }
    }
}
