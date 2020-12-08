using System;
using System.Threading.Tasks;

namespace Tadmor.Services.Translation
{
    [SingletonService]
    public class NoopTranslationService
    {

        public async Task<string> BadTranslate(string input)
        {
            throw new Exception("free translation API has been disabled");
        }

        public ValueTask<string> Translate(string text, string direction)
        {
            throw new Exception("free translation API has been disabled");
        }

        public async Task<string> DetectAndTranslate(string text, string destinationLanguage)
        {
            throw new Exception("free translation API has been disabled");
        }
    }
}