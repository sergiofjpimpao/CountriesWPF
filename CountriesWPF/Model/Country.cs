using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CountriesWPF.Modelos
{
    public class Flag
    {
        public string png { get; set; }
    }

    public class Name
    {
        public string common { get; set; }
    }
    public class Maps
    {
        public string openStreetMaps { get; set; }
    }

    public class Country
    {
        public Flag flags { get; set; }
        public Name name { get; set; }
        public Maps maps { get; set; }
        public List<string> capital { get; set; }
        public string region { get; set; }
        public string subregion { get; set; }
        public double area { get; set; }
        public int population { get; set; }
        public Dictionary<string, double> gini { get; set; }
    }
}



