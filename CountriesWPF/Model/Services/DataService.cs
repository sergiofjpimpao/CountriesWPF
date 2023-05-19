using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountriesWPF.Modelos.Servicos
{
    public class DataService
    {
        private SQLiteConnection connection;
        private SQLiteCommand command;
        private DialogService dialogService;

        /// <summary>
        /// Provides data access and manipulation functionality for the countries' data.
        /// </summary>
        public DataService()
        {
            dialogService = new DialogService();
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }

            var path = @"Data\CountryData.sqlite";
            try
            {
                connection = new SQLiteConnection("Data Source=" + path);
                connection.Open();
                string sqlcommand = "CREATE TABLE IF NOT EXISTS countries (Name TEXT, Capital TEXT, Region TEXT, Subregion TEXT, Area REAL, Population INTEGER, FlagPng TEXT, GiniKey TEXT, GiniValue REAL)";
                command = new SQLiteCommand(sqlcommand, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                dialogService.ShowMessage("Error", e.Message);
            }
        }

        /// <summary>
        /// Saves the provided list of countries' data into the database.
        /// </summary>
        /// <param name="countries">The list of countries to be saved.</param>
        public void SaveData(List<Country> countries)
        {
            try
            {
                foreach (var country in countries)
                {
                    string sql = "INSERT INTO countries (Name, Capital, Region, Subregion, Area, Population, FlagPng, GiniKey, GiniValue) " +
                                 "VALUES (@Name, @Capital, @Region, @Subregion, @Area, @Population, @FlagPng, @GiniKey, @GiniValue)";

                    command = new SQLiteCommand(sql, connection);
                    command.Parameters.AddWithValue("@Name", country.name.common);
                    command.Parameters.AddWithValue("@Capital", country.capital?.FirstOrDefault());
                    command.Parameters.AddWithValue("@Region", country.region);
                    command.Parameters.AddWithValue("@Subregion", country.subregion);
                    command.Parameters.AddWithValue("@Area", country.area);
                    command.Parameters.AddWithValue("@Population", country.population);
                    command.Parameters.AddWithValue("@FlagPng", country.flags?.png);
                    command.Parameters.AddWithValue("@GiniKey", country.gini?.Keys.FirstOrDefault());
                    command.Parameters.AddWithValue("@GiniValue", country.gini?.Values.FirstOrDefault());

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                dialogService.ShowMessage("Saving Error", e.Message);
            }
        }

        /// <summary>
        /// Retrieves the list of countries' data from the database.
        /// </summary>
        /// <returns>The list of countries.</returns>
        public List<Country> GetData()
        {
            List<Country> countries = new List<Country>();

            try
            {
                string sql = "SELECT Name, Capital, Region, Subregion, Area, Population, FlagPng, GiniKey, GiniValue FROM countries";
                command = new SQLiteCommand(sql, connection);

                // Read each record
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Country country = new Country
                    {
                        name = new Name
                        {
                            common = Convert.IsDBNull(reader["Name"]) ? null : (string)reader["Name"]
                        },
                        capital = Convert.IsDBNull(reader["Capital"]) ? new List<string>() : ((string)reader["Capital"]).Split(',').ToList(),
                        region = Convert.IsDBNull(reader["Region"]) ? null : (string)reader["Region"],
                        subregion = Convert.IsDBNull(reader["Subregion"]) ? null : (string)reader["Subregion"],
                        area = Convert.IsDBNull(reader["Area"]) ? 0.0 : (double)reader["Area"],
                        population = Convert.IsDBNull(reader["Population"]) ? 0 : (int)(long)reader["Population"],
                        flags = new Flag
                        {
                            png = Convert.IsDBNull(reader["FlagPng"]) ? null : (string)reader["FlagPng"]
                        }
                    };

                    object giniKey = reader["GiniKey"];
                    object giniValue = reader["GiniValue"];

                    if (!Convert.IsDBNull(giniKey) && !Convert.IsDBNull(giniValue))
                    {
                        country.gini = new Dictionary<string, double>
                {
                    { (string)giniKey, (double)giniValue }
                };
                    }

                    countries.Add(country);
                }

                connection.Close();

                return countries;
            }
            catch (Exception e)
            {
                dialogService.ShowMessage("Loading Error", e.Message);
                return null;
            }
        }




        /// <summary>
        /// Deletes all the countries' data from the database.
        /// </summary>
        public void DeleteData()
        {
            try
            {
                string sql = "DELETE FROM countries";
                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                dialogService.ShowMessage("Error", e.Message);
            }
        }
    }
    }
