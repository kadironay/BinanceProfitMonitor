using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Objects.Spot;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using ProfitMonitor.Core;

namespace ProfitMonitor.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            BinanceClient.SetDefaultOptions(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials("APIKEY", "APISECRET"),
                LogVerbosity = LogVerbosity.Error,
                LogWriters = new List<TextWriter> { System.Console.Out }
            });
            BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
            {
                ApiCredentials = new ApiCredentials("APIKEY", "APISECRET"),
                LogVerbosity = LogVerbosity.Error,
                LogWriters = new List<TextWriter> { System.Console.Out }
            });

            BinanceClient binanceClient = new BinanceClient();
            BinanceSocketClient binanceSocketClient = new BinanceSocketClient();

            BinanceService binanceService = new BinanceService(binanceClient, binanceSocketClient);
            BinanceTradeHistoryFetcher binanceTradeHistoryFetcher = new BinanceTradeHistoryFetcher(binanceService);

            var trades = await binanceTradeHistoryFetcher.Fetch(DateTime.ParseExact("2021-01-01 00:00", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            foreach (var trade in trades)
            {
                System.Console.WriteLine($"Symbol: {trade.Symbol}, Price: {trade.Price}, Quantity: {trade.Quantity}, Commission: {trade.Commission}, CommissionAsset: {trade.CommissionAsset} ");
            }
            var json = JsonSerializer.Serialize(trades);
            File.WriteAllText("Trades.json", json);

            System.Console.ReadLine();
        }
    }
}
