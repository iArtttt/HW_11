using System.Diagnostics;
using System.Text.Json;
using static Parser.Program;

namespace Parser
{
    internal class Program
    {
        public class Country
        {
            public string Name { get; set; }
            public List<District> Districts { get; set;} = new List<District>();
        }
        public class District
        {
            public string Name { get; set; }
            public List<City> Cities { get; set; } = new List<City>();
        }
        public class City
        {
            public string Name { get; set; }
            public int Population { get; set; }
            public double Area { get; set; }
        }
        static void Main(string[] args)
        {
            var countryList = new Dictionary<string, Country>();
            //  Ім'яМіста:площа;населення;Країна(область)
            var timer = Stopwatch.StartNew();
            timer.Start();

            foreach (var line in File.ReadLines("../../../../countries.txt"))
            {
                ReadOnlySpan<char> span = line.AsSpan();

                var cityName = GetCityName(span, out span);

                (int pop, double area) = GetPopArea(span, out span);


                ReadOnlySpan<char> district, country;
                GetCountryDist(span, out district, out country);

                var city = new City { Name = cityName.ToString(), Population = pop, Area = area };

                var countryName = country.ToString();
                if (!countryList.TryGetValue(countryName, out Country existedCountry))
                {
                    existedCountry ??= new Country() { Name = countryName };
                    countryList[countryName] = existedCountry;
                }

                var districtName = district.ToString();
                var dist = new District() { Name = districtName };
                dist.Cities.Add(city);

                existedCountry.Districts.Add(dist);

            }
            timer.Stop();
            Console.WriteLine(timer.Elapsed);
            
            timer.Restart();
            File.WriteAllText("../../../../countries.json", JsonSerializer.Serialize(countryList.Values));

            timer.Stop();
            Console.WriteLine("Json: " + timer.Elapsed);
        }

        private static ReadOnlySpan<char> GetCityName(ReadOnlySpan<char> line, out ReadOnlySpan<char> left)
        {
            var index = line.IndexOf(':');
            left = line.Slice(index + 1);
            return line.Slice(0, index);
        }

        private static (int pop, double area) GetPopArea(ReadOnlySpan<char> line, out ReadOnlySpan<char> left)
        {
            var index = line.IndexOf(';');
            var areaSpan = line.Slice(0, index);

            left = line.Slice(index + 1);
            index = left.IndexOf(';');
            var popSpan = left.Slice(0, index);

            left = left.Slice(index + 1);
            return (int.Parse(popSpan),double.Parse(areaSpan));
        }

        private static void GetCountryDist(ReadOnlySpan<char> line, out ReadOnlySpan<char> district, out ReadOnlySpan<char> country)
        {
            var index = line.IndexOf('(');
            country = line.Slice(0, index);
            district = line[(index + 1)..^1];
        }
    }
}