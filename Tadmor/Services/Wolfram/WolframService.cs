using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WolframAlphaNet;
using WolframAlphaNet.Enums;
using WolframAlphaNet.Misc;
using WolframAlphaNet.Objects;

namespace Tadmor.Services.Wolfram
{
    [Options]
    public class WolframOptions
    {
        public string AppId { get; set; }
    }

    [SingletonService]
    public class WolframService
    {
        private readonly WolframAlpha _wolfram;

        public WolframService(IOptionsSnapshot<WolframOptions> options)
        {
            _wolfram = new WolframAlpha(options.Value.AppId) {ReInterpret = true};
        }

        public async Task<List<Pod>> Query(string value)
        {
            var response = await Task.Run(() => _wolfram.Query(value));
            if (response.Error != null)
                throw new Exception(response.Error.Message);
            if (response.DidYouMean.Any())
                throw new Exception($"no results, did you mean: {response.DidYouMean.First().Value}");
            if (!response.Success)
                throw new Exception("i don't know what you mean");
            return response.Pods;
        }
    }
}