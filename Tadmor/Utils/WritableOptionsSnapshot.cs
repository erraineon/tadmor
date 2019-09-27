using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tadmor.Utils
{
    public class WritableOptionsSnapshot<T> : IWritableOptionsSnapshot<T> where T : class, new()
    {
        private readonly IHostEnvironment _environment;
        private readonly IOptionsMonitor<T> _options;
        private readonly string _section;
        private readonly string _file;

        public WritableOptionsSnapshot(
            IHostEnvironment environment,
            IOptionsMonitor<T> options,
            string section,
            string file)
        {
            _environment = environment;
            _options = options;
            _section = section;
            _file = file;
        }

        public T Value => _options.CurrentValue;
        public T Get(string name) => _options.Get(name);

        public async Task UpdateAsync(Action<T> applyChanges)
        {
            var fileProvider = _environment.ContentRootFileProvider;
            var fileInfo = fileProvider.GetFileInfo(_file);
            var physicalPath = fileInfo.PhysicalPath;
            var fileContent = await File.ReadAllTextAsync(physicalPath);
            var jObject = JsonConvert.DeserializeObject<JObject>(fileContent);
            var sectionObject = jObject.TryGetValue(_section, out var section) ?
                JsonConvert.DeserializeObject<T>(section.ToString()) : Value ?? new T();

            applyChanges(sectionObject);

            jObject[_section] = JObject.Parse(JsonConvert.SerializeObject(sectionObject));
            await File.WriteAllTextAsync(physicalPath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
        }
    }
}