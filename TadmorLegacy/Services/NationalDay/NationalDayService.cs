using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Tadmor.Services.NationalDay
{
    [TransientService]
    public class NationalDayService
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public async Task<IList<Holiday>> GetTodaysHolidays()
        {
            var rawContent = await HttpClient.GetStringAsync("https://www.checkiday.com/api/3/?d=today");
            var jo = JObject.Parse(rawContent);
            var holidays = jo["holidays"]
                .Select(t => new Holiday((string) t["name"], (string) t["url"]))
                .ToList();
            return holidays;
        }
    }
}