using System.Text.RegularExpressions;

namespace Tully
{
    internal static class NetworkPackageSniffer
    {
        private const string GetRequestPattern = "^GET";

        private const string ConnectionUpgradePattern = "Connection: Upgrade";

        private const string UpgradeWebSocketPattern = "Upgrade: websocket";

        private const string SwitichingProtocolsPattern = "Switching Protocols";

        private const string WebSocketKeyPattern = "Sec-WebSocket-Key: (.*)";

        private static readonly Regex GetRequestRegex = new Regex(
            GetRequestPattern,
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ConnectionUpgradeRegex = new Regex(
            ConnectionUpgradePattern,
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex UpgradeWebSocketRegex = new Regex(
            UpgradeWebSocketPattern,
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SwitchingProtocolsRegex = new Regex(
            SwitichingProtocolsPattern,
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex WebSocketKeyRegex = new Regex(
            WebSocketKeyPattern,
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static bool IsOpeningHandshake(string packet)
        {
            return GetRequestRegex.IsMatch(packet) && ConnectionUpgradeRegex.IsMatch(packet)
                   && UpgradeWebSocketRegex.IsMatch(packet);
        }

        internal static bool IsClosingHandshake(string packet)
        {
            return SwitchingProtocolsRegex.IsMatch(packet) && ConnectionUpgradeRegex.IsMatch(packet)
                   && UpgradeWebSocketRegex.IsMatch(packet);
        }

        internal static string GetWebSocketKey(string packet)
        {
            string key = string.Empty;
            Match match = WebSocketKeyRegex.Match(packet);

            if (match.Success)
            {
                key = match.Groups[1].Value.Trim();
            }

            return key;
        }
    }
}