using System.Text;

namespace RSS_Services
{
    public class RecapPeriodRow
    {
        public string PeriodLabel { get; set; } = string.Empty;
        public string WinnerName { get; set; } = string.Empty;
        public decimal Payout { get; set; }
    }

    public class RecapEmailModel
    {
        public string GameName { get; set; } = string.Empty;
        public string MatchupLine { get; set; } = string.Empty; // e.g. "Warriors @ Lakers — 98–110"
        public List<RecapPeriodRow> PeriodRows { get; set; } = new();
        public string RecipientName { get; set; } = string.Empty;
        public decimal CoinsWagered { get; set; }
        public decimal CoinsWon { get; set; }
        public decimal Net => CoinsWon - CoinsWagered;
    }

    public static class GameEmailTemplates
    {
        public static string GetPeriodLabel(int period, int periodCount)
        {
            if (periodCount == 2)
                return period == 1 ? "1st Half" : "2nd Half";
            return $"Q{period}";
        }

        // Amounts are coins, not currency — never format with a culture-dependent currency string
        public static string FormatCoins(decimal amount) => $"{amount:0.##} coins";

        public static (string Subject, string Text, string Html) BuildPeriodWin(
            string gameName, string periodLabel, decimal coinsWon)
        {
            var subject = $"You won {periodLabel} in {gameName}!";

            var text =
                $"Congratulations!\n\n" +
                $"Your square won {periodLabel} in \"{gameName}\".\n" +
                $"Winnings: {FormatCoins(coinsWon)}\n\n" +
                $"Good luck the rest of the game!";

            var html =
                $"<div style=\"font-family:Arial,Helvetica,sans-serif;max-width:600px;margin:0 auto;color:#222\">" +
                $"<h2 style=\"color:#c62828\">You won {Escape(periodLabel)}!</h2>" +
                $"<p>Your square won <strong>{Escape(periodLabel)}</strong> in <strong>{Escape(gameName)}</strong>.</p>" +
                $"<p style=\"font-size:18px\">Winnings: <strong>{FormatCoins(coinsWon)}</strong></p>" +
                $"<p style=\"color:#666\">Good luck the rest of the game!</p>" +
                $"</div>";

            return (subject, text, html);
        }

        public static (string Subject, string Text, string Html) BuildRecap(RecapEmailModel model)
        {
            var subject = $"Game recap: {model.GameName}";

            var text = new StringBuilder();
            text.AppendLine($"Game recap — {model.GameName}");
            text.AppendLine(model.MatchupLine);
            text.AppendLine();
            text.AppendLine("Period winners:");
            foreach (var row in model.PeriodRows)
                text.AppendLine($"  {row.PeriodLabel}: {row.WinnerName} — {FormatCoins(row.Payout)}");
            text.AppendLine();
            text.AppendLine($"Your results, {model.RecipientName}:");
            text.AppendLine($"  Coins wagered: {FormatCoins(model.CoinsWagered)}");
            text.AppendLine($"  Coins won: {FormatCoins(model.CoinsWon)}");
            text.AppendLine($"  Net: {FormatCoins(model.Net)}");
            text.AppendLine();
            text.AppendLine("Thanks for playing!");

            var rows = new StringBuilder();
            foreach (var row in model.PeriodRows)
            {
                rows.Append(
                    $"<tr>" +
                    $"<td style=\"padding:6px 12px;border-bottom:1px solid #ddd\">{Escape(row.PeriodLabel)}</td>" +
                    $"<td style=\"padding:6px 12px;border-bottom:1px solid #ddd\">{Escape(row.WinnerName)}</td>" +
                    $"<td style=\"padding:6px 12px;border-bottom:1px solid #ddd;text-align:right\">{FormatCoins(row.Payout)}</td>" +
                    $"</tr>");
            }

            var netColor = model.Net >= 0 ? "#2e7d32" : "#c62828";
            var html =
                $"<div style=\"font-family:Arial,Helvetica,sans-serif;max-width:600px;margin:0 auto;color:#222\">" +
                $"<h2 style=\"color:#c62828\">Game recap: {Escape(model.GameName)}</h2>" +
                $"<p style=\"font-size:16px\"><strong>{Escape(model.MatchupLine)}</strong></p>" +
                $"<table style=\"border-collapse:collapse;width:100%\">" +
                $"<tr>" +
                $"<th style=\"padding:6px 12px;border-bottom:2px solid #222;text-align:left\">Period</th>" +
                $"<th style=\"padding:6px 12px;border-bottom:2px solid #222;text-align:left\">Winner</th>" +
                $"<th style=\"padding:6px 12px;border-bottom:2px solid #222;text-align:right\">Payout</th>" +
                $"</tr>" +
                rows +
                $"</table>" +
                $"<h3 style=\"margin-top:24px\">Your results, {Escape(model.RecipientName)}</h3>" +
                $"<p style=\"margin:4px 0\">Coins wagered: {FormatCoins(model.CoinsWagered)}</p>" +
                $"<p style=\"margin:4px 0\">Coins won: {FormatCoins(model.CoinsWon)}</p>" +
                $"<p style=\"margin:4px 0\">Net: <strong style=\"color:{netColor}\">{FormatCoins(model.Net)}</strong></p>" +
                $"<p style=\"color:#666;margin-top:24px\">Thanks for playing!</p>" +
                $"</div>";

            return (subject, text.ToString(), html);
        }

        private static string Escape(string value) => System.Net.WebUtility.HtmlEncode(value);
    }
}
