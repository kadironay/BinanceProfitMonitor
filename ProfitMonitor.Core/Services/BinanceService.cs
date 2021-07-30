using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot.SpotData;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;

namespace ProfitMonitor.Core
{
    public class BinanceService : IBinanceService
    {
        private IBinanceClient _client;
        private IBinanceSocketClient _socketClient;

        public BinanceService(IBinanceClient client, IBinanceSocketClient socketClient)
        {
            _client = client;
            _socketClient = socketClient;
        }

        public Task<CallResult<UpdateSubscription>> SubscribeTickerUpdates(Action<IEnumerable<IBinanceTick>> tickHandler)
        {
            return _socketClient.Spot.SubscribeToAllSymbolTickerUpdatesAsync(data => {
                tickHandler(data);
            });
        }

        public async Task Unsubscribe(UpdateSubscription subscription)
        {
            await _socketClient.Unsubscribe(subscription);
        }

        public WebCallResult<IEnumerable<IBinanceTick>> Get24HPrices()
        {
            return _client.Spot.Market.Get24HPrices();
        }

        public Task<WebCallResult<IEnumerable<IBinanceTick>>> Get24HPricesAsync()
        {
            return _client.Spot.Market.Get24HPricesAsync();
        }

        public WebCallResult<IEnumerable<BinanceTrade>> GetMyTrades(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null)
        {
            return _client.Spot.Order.GetMyTrades(symbol, startTime, endTime, limit);
        }
    }
}