
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot.SpotData;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;

namespace ProfitMonitor.Core
{
    public interface IBinanceService
    {
        public Task<CallResult<UpdateSubscription>> SubscribeTickerUpdates(Action<IEnumerable<IBinanceTick>> tickHandler);
        public Task Unsubscribe(UpdateSubscription subscription);
        public WebCallResult<IEnumerable<IBinanceTick>> Get24HPrices();
        public Task<WebCallResult<IEnumerable<IBinanceTick>>> Get24HPricesAsync();
        public WebCallResult<IEnumerable<BinanceTrade>> GetMyTrades(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null);

    }

}