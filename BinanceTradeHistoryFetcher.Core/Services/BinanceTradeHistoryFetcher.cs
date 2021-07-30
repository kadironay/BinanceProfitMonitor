

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot.SpotData;

namespace BinanceTradeHistoryFetcher.Core
{
    public class BinanceTradeHistoryFetcher
    {
        private IBinanceService _binanceService;
        private List<IBinanceTick> _ticks;
        public BinanceTradeHistoryFetcher(IBinanceService binanceService)
        {
            _binanceService = binanceService;
        }

        public Task<IEnumerable<BinanceTrade>> Fetch(DateTime? startTime)
        {
            var result = _binanceService.Get24HPrices();
            if (result.Success)
            {
                _ticks = new List<IBinanceTick>(result.Data);
            }

            return FetchTradeHistories(startTime);
        }

        private Task<IEnumerable<BinanceTrade>> FetchTradeHistories(DateTime? startTime)
        {
            return Task.Run(() =>
            {
                int count = 0;
                List<BinanceTrade> trades = new List<BinanceTrade>();
                BinanceRequestWatcher watcher = new BinanceRequestWatcher(1200, TimeSpan.FromMinutes(1));
                watcher.Start();
                foreach (var tick in _ticks)
                {
                    count++;
                    var trade = GetTradeInfo(watcher, tick.Symbol, startTime);
                    if (trade != null)
                    {
                        trades.AddRange(trade);
                    }
                    int percentage = (int)Math.Round((double)(100 * count) / _ticks.Count);
                    if ((percentage % 2) == 0)
                    {
                        System.Console.WriteLine($"{percentage}% done!");
                    }
                }
                IEnumerable<BinanceTrade> tempTrades = trades;
                return tempTrades;
            });
        }

        private IEnumerable<BinanceTrade> GetTradeInfo(BinanceRequestWatcher watcher, string symbol, DateTime? startTime)
        {
            bool success = false;
            IEnumerable<BinanceTrade> trades = null;
            do
            {
                var result = _binanceService.GetMyTrades(symbol, startTime);
                watcher.AddWeight(10);
                success = result.Success;
                if (success)
                {
                    trades = result.Data;
                    var temp = new List<BinanceTrade>(trades);
                    if (temp.Count > 0)
                    {
                        foreach (var item in temp)
                        {
                            System.Console.WriteLine($"New trade symbol: {item.Symbol}");
                        }
                    }
                }
                else
                {
                    var remainingTime = watcher.PeriodRemaining();
                    System.Console.WriteLine($"Waiting for {remainingTime.TotalSeconds} seconds for the new request!");
                    Thread.Sleep(remainingTime);
                }
            } while (!success);

            return trades;
        }
    }

    public class BinanceRequestWatcher
    {
        private int _maxWeight;
        private TimeSpan _period;
        private Timer _timer = null;
        private Stopwatch _stopWatch = new Stopwatch();
        private int _currentWeight = 0;

        public BinanceRequestWatcher(int maxWeight, TimeSpan period)
        {
            _maxWeight = maxWeight;
            _period = period;
        }

        public void Start()
        {
            Stop();
            _timer = new Timer(OnTimerTimeout, null, _period, _period);
            _stopWatch.Start();
        }

        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
                _stopWatch.Reset();
            }
        }

        private void OnTimerTimeout(object state)
        {
            _currentWeight = 0;
            _stopWatch.Restart();
        }

        public void AddWeight(int weight)
        {
            _currentWeight += weight;
        }

        public bool ShouldWait()
        {
            return (_currentWeight >= _maxWeight);
        }

        public TimeSpan PeriodRemaining()
        {
            return (_period - _stopWatch.Elapsed);
        }
    }
}
