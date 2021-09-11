using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tadmor.TextGeneration.Interfaces;
using Tadmor.TextGeneration.Models;

namespace Tadmor.TextGeneration.Services
{
    public class TadmorMindThoughtProducer : BackgroundService
    {
        private readonly Gpt3TadmorMindOptions _tadmorMindOptions;
        private readonly ITadmorMindThoughtsRepository _tadmorMindThoughtsRepository;
        private readonly ITadmorMindClient _tadmorMindClient;
        private readonly ILogger<TadmorMindThoughtProducer> _logger;

        public TadmorMindThoughtProducer(
            Gpt3TadmorMindOptions tadmorMindOptions,
            ITadmorMindThoughtsRepository tadmorMindThoughtsRepository,
            ITadmorMindClient tadmorMindClient,
            ILogger<TadmorMindThoughtProducer> logger)
        {
            _tadmorMindOptions = tadmorMindOptions;
            _tadmorMindThoughtsRepository = tadmorMindThoughtsRepository;
            _tadmorMindClient = tadmorMindClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_tadmorMindThoughtsRepository.Count < (_tadmorMindOptions.BufferSize ?? 16))
                {
                    try
                    {
                        var entries = await _tadmorMindClient.GenerateEntriesAsync();
                        foreach (var entry in entries) _tadmorMindThoughtsRepository.Add(entry);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "error while retrieving tadmor mind thoughts");
                        await GetDelayTask(stoppingToken);
                    }
                }
                else
                {
                    await GetDelayTask(stoppingToken);
                }
            }
        }

        private static Task GetDelayTask(CancellationToken stoppingToken)
        {
            return Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }
}