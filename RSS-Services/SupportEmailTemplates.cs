namespace RSS_Services
{
    public static class SupportEmailTemplates
    {
        public static (string Subject, string Text, string Html) BuildSupportRequest(
            string name, string email, string category, string message)
        {
            var subject = $"[Support/{category}] {name}";

            var text =
                $"New support request\n\n" +
                $"Category: {category}\n" +
                $"From: {name} <{email}>\n\n" +
                $"{message}";

            var html =
                $"<div style=\"font-family:Arial,Helvetica,sans-serif;max-width:600px;margin:0 auto;color:#222\">" +
                $"<h2 style=\"color:#c62828\">New support request</h2>" +
                $"<p><strong>Category:</strong> {Escape(category)}</p>" +
                $"<p><strong>From:</strong> {Escape(name)} &lt;{Escape(email)}&gt;</p>" +
                $"<p style=\"white-space:pre-wrap;border-top:1px solid #ddd;padding-top:12px;margin-top:12px\">{Escape(message)}</p>" +
                $"</div>";

            return (subject, text, html);
        }

        private static string Escape(string value) => System.Net.WebUtility.HtmlEncode(value);
    }
}
