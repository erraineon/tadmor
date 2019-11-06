using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tadmor.Utils
{
    public class WritableOptionsSnapshot<T> : IWritableOptionsSnapshot<T> where T : class, new()
    {
        private readonly IHostEnvironment _environment;
        private readonly IOptionsSnapshot<T> _options;
        private readonly string _optionsPath;
        private readonly JToken _originalJsonValue;
        private readonly string _section;

        public WritableOptionsSnapshot(
            IHostEnvironment environment,
            IOptionsSnapshot<T> options,
            string section,
            string optionsPath)
        {
            _environment = environment;
            _options = options;
            _section = section;
            _optionsPath = optionsPath;
            _originalJsonValue = JToken.FromObject(Value);
        }

        public T Value => _options.Value;

        public T Get(string name)
        {
            return _options.Get(name);
        }

        public void Dispose()
        {
            var newValue = JToken.FromObject(Value);
            if (!JToken.DeepEquals(newValue, _originalJsonValue))
            {
                var fileInfo = _environment.ContentRootFileProvider.GetFileInfo(_optionsPath);
                var optionsPath = fileInfo.PhysicalPath;
                var optionsRoot = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(optionsPath));
                optionsRoot[_section] = newValue;
                File.WriteAllText(optionsPath, JsonConvert.SerializeObject(optionsRoot, Formatting.Indented));
            }
        }
    }
}