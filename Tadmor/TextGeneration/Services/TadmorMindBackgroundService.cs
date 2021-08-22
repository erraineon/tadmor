using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Tadmor.TextGeneration.Interfaces;
using Tadmor.TextGeneration.Models;

namespace Tadmor.TextGeneration.Services
{
    public class TadmorMindBackgroundService : BackgroundService
    {
        private readonly TadmorMindOptions _tadmorMindOptions;
        private readonly ITadmorMindThoughtsRepository _tadmorMindThoughtsRepository;
        private readonly ITadmorMindClient _tadmorMindClient;

        public TadmorMindBackgroundService(
            TadmorMindOptions tadmorMindOptions,
            ITadmorMindThoughtsRepository tadmorMindThoughtsRepository,
            ITadmorMindClient tadmorMindClient)
        {
            _tadmorMindOptions = tadmorMindOptions;
            _tadmorMindThoughtsRepository = tadmorMindThoughtsRepository;
            _tadmorMindClient = tadmorMindClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_tadmorMindThoughtsRepository.Count < (_tadmorMindOptions.BufferSize ?? 128))
                {
                    var entries = await _tadmorMindClient.GenerateEntriesAsync();
                    foreach (var entry in entries) _tadmorMindThoughtsRepository.Add(entry);
                }
            }
        }
    }
}