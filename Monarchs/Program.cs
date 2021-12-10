using Newtonsoft.Json;

namespace Monarchs { 
public class Program
{
        public static async Task Main()
        {
            Console.WriteLine("The monarchs.\n\n");

            //Get the list of monarchs.
            IList<King>? kingList = await GetMonarchsListAsync();

            if (kingList == null || kingList.Any() == false)
            { 
                Console.WriteLine("Missing data"); 
                Console.ReadKey();
                return;
            }

            // 1- Number of monarchs:
            PrintNbrOfMonarchInTheList(kingList.Count);

            // 2- Longest rulled monarch:
            PrintTheLongestRulledMonarch(kingList);

            // 3- Longest rulled house:
            PrintTheLongestRulledHouse(kingList);

            // 4- Most common first name:
            PrintTheMostCommonFirstName(kingList);

            Console.ReadKey();
        }


        private static async Task<IList<King>?> GetMonarchsListAsync()
        {
            var webClient = new HttpClient();

            HttpResponseMessage response = await webClient.GetAsync(
                "https://gist.githubusercontent.com/christianpanton/10d65ccef9f29de3acd49d97ed423736/raw/b09563bc0c4b318132c7a738e679d4f984ef0048/kings");

            string result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IList<King>>(result);
        }

        private static void PrintNbrOfMonarchInTheList(int nbrMonarchs)
        {
            Console.WriteLine($"1- {(nbrMonarchs == 1 ? "Only one monarch found" : $"A total of {nbrMonarchs} monarchs")} has rulled so far.\n\n");
        }

        private static void PrintTheLongestRulledMonarch(IEnumerable<King> kingList)
        {
            // Order the list by rulling period descending to have the higest on top.
            // Then select the first monarch.
            var longestRulingKing = kingList?.OrderByDescending(x => x.RuledPeriod).FirstOrDefault();
            Console.WriteLine($"2- The longest ruling monarch {longestRulingKing?.Description}\n\n");
        }

        private static void PrintTheLongestRulledHouse(IEnumerable<King> kingList)
        {
            // Group the list by the House property excluding the Commonewalth rulers in a new list
            // with house name, rulling period, first monarch and last monarch.
            var house = kingList
                .GroupBy(x => x.House)?
                .Where(h => h.Key != null && !h.Key.Equals("Commonwealth", StringComparison.OrdinalIgnoreCase))
                .Select(h => 
                    new
                    {
                        House = h.Key,
                        RullingPerid = h.Sum(d => d.RuledPeriod),
                        FirstMonarch = h.OrderBy(d => d.RuledFrom).FirstOrDefault(),
                        LastMonarch = h.OrderByDescending(d => d.RuledTo).FirstOrDefault(),
                        Monarchs = h.Select(d => d)
                    });

            // Then order by the sum of the years ruled by each monarch of the house
            // in a descending order to have the higest on top.
            // And finaly select the first in the list.
            var longestRulingHouse = house?.OrderByDescending(x => x.RullingPerid).FirstOrDefault();
            if (longestRulingHouse == null)
            {
                Console.WriteLine("No data available.");
                return;
            }

            Console.WriteLine($"3- The longest ruling house is '{longestRulingHouse.House}' " +
                $"who ruled for {longestRulingHouse.RullingPerid} years.\n" +
                $"   Starting with {longestRulingHouse.FirstMonarch?.Name} {longestRulingHouse.FirstMonarch?.Period} " +
                $"untill {longestRulingHouse.LastMonarch?.Name} {longestRulingHouse.LastMonarch?.Period}.\n\n");
        }

        private static void PrintTheMostCommonFirstName(IEnumerable<King> kingList)
        {
            // Use a key selecotor to get the first name of a monarch.
            // Then group by the result. And order by the count each first name descending to have the higest on top.
            // And finaly select the first in the list.

            Func<King, string> firstName = x => x.Name?.Split(' ').FirstOrDefault() ?? "Unknown";

            var houses = kingList
                .GroupBy(firstName)?
                .Select(x => new { Name = x.Key, Count = x.Count() });

            var mostCommontName = houses?.OrderByDescending(x => x.Count).FirstOrDefault();
            if (mostCommontName == null)
            {
                Console.WriteLine("No data available.");
                return;
            }

            Console.WriteLine($"4- The most comon name is '{mostCommontName.Name}' with {mostCommontName.Count} monarchs having this name.\n\n");
        }
    }

    [Serializable]
    public class King
    { 
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nm")]
        public string? Name { get; set; }

        [JsonProperty("cty")]
        public string? Country { get; set; }

        [JsonProperty("hse")]
        public string? House { get; set; }

        [JsonProperty("yrs")]
        public string? Period { get; set; }


        public int RuledFrom => int.TryParse(Period?.Split((char)45).FirstOrDefault(), out int year) ? year : 0;
        public int RuledTo => int.TryParse(Period?.Split((char)45).LastOrDefault(), out int year) ? year : DateTime.Today.Year;
        public int RuledPeriod => RuledTo - RuledFrom;
        public bool StillRulling => RuledTo == DateTime.Today.Year;
        public string Description => $"{(StillRulling ? "is" : "was")} '{Name}' of '{House}'.\n   Ruled for {RuledPeriod} years, from {RuledFrom} untill {(StillRulling ? "today" : RuledTo)}.";
    }
}