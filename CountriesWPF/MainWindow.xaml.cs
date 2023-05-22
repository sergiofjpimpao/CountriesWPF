using CountriesWPF.Modelos;
using CountriesWPF.Modelos.Servicos;
using Microsoft.Web.WebView2.Core;
using Syncfusion.SfSkinManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;

namespace CountriesWPF
{
    public partial class MainWindow : Window
    {
        private List<Country> countries;
        private NetworkService networkService;
        private ApiService apiService;
        private DialogService dialogService;
        private DataService dataService;
        private MainWindowViewModel viewModel;
        private IProgress<int> progress;
        bool loadAPI;

        public MainWindow()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            InitializeComponent();
            Loaded += OnLoaded;

            networkService = new NetworkService();
            apiService = new ApiService();
            dialogService = new DialogService();
            dataService = new DataService();
            viewModel = new MainWindowViewModel();
            DataContext = viewModel;
            LoadCountriesAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetVisualStyle("Windows11Dark");
            SetSizeMode("Default");
        }

        /// <summary>
        /// Sets the visual style of the application.
        /// </summary>
        /// <param name="visualStyle">The name of the visual style to apply.</param>
        private void SetVisualStyle(string visualStyle)
        {
            VisualStyles style;
            if (Enum.TryParse(visualStyle, out style) && style != VisualStyles.Default)
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.SetVisualStyle(this, style);
                SfSkinManager.ApplyStylesOnApplication = false;
            }
        }
        /// <summary>
        /// Sets the size mode of the application.
        /// </summary>
        /// <param name="sizeMode">The name of the size mode to apply.</param>
        private void SetSizeMode(string sizeMode)
        {
            SizeMode mode;
            if (Enum.TryParse(sizeMode, out mode) && mode != SizeMode.Default)
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.SetSizeMode(this, mode);
                SfSkinManager.ApplyStylesOnApplication = false;
            }
        }
        /// <summary>
        /// Loads the countries asynchronously from either the API or the local database.
        /// </summary>
        private async void LoadCountriesAsync()
        {            
            var connection = networkService.CheckConnection();
            var apiconnection = await networkService.CheckApiConnection("https://restcountries.coam/v3.1/all");

            if (!connection.IsSuccess || !apiconnection.IsSuccess)
            {
                LoadLocalCountries();
                loadAPI = false;
            }
            else
            {
                progress = new Progress<int>(value => viewModel.ProgressValue = value);
                await LoadApiCountriesAsync(progress);
                loadAPI = true;
            }

            if (countries != null && countries.Count > 0)
            {
                viewModel.CountryNames = countries.Select(c => c.name.common).ToList();
            }
            else if (connection.IsSuccess)
            {
                dialogService.ShowMessage("API Error","No countries found from the API.");
            }

            if (loadAPI)
            {
                DateTime now = DateTime.Now;
                string currentDateTimeString = now.ToString("dd-MM-yyyy HH:mm:ss");
                LoadingSource.Text = "Data loaded from the internet at " + currentDateTimeString + ".";
            }
            else
            {
                LoadingSource.Text = "Data loaded from the fallback database.";
            }
        }
        /// <summary>
        /// Loads the countries asynchronously from the API.
        /// </summary>
        /// <param name="progress">The progress object to report the loading progress.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadApiCountriesAsync(IProgress<int> progress)
        {
            viewModel.IsLoading = true;

            var response = await apiService.GetCountriesAsync("https://restcountries.com", "/v3.1/all?fields=name,flags,region,subregion,capital,population,area,gini,maps");

            if (response.IsSuccess)
            {
                countries = (List<Country>)response.Result;

                await DownloadFlagImagesAsync(countries, progress);

                viewModel.IsLoading = false;

                dataService.DeleteData();
                dataService.SaveData(countries);
            }
            else
            {
                dialogService.ShowMessage("API Error", $"Failed to retrieve countries from API: {response.Message}");
                LoadLocalCountries();                
            }
        }
        /// <summary>
        /// Loads the countries from the local database.
        /// </summary>
        private void LoadLocalCountries()
        {
            countries = dataService.GetData();

            if (countries == null || countries.Count == 0)
            {
                dialogService.ShowMessage("Missing data","This application requires an internet connection to load data.");
                Application.Current.Shutdown();
            }

        }
        /// <summary>
        /// Downloads the flag images asynchronously for the given countries.
        /// </summary>
        /// <param name="countries">The list of countries to download flag images for.</param>
        /// <param name="progress">The progress object to report the download progress.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task DownloadFlagImagesAsync(List<Country> countries, IProgress<int> progress)
        {
            const string flagDirectory = "Flags";
            Directory.CreateDirectory(flagDirectory);

            using (HttpClient httpClient = new HttpClient())
            {
                int totalCount = countries.Count;
                int completedCount = 0;

                foreach (var country in countries)
                {
                    string flagFilePath = Path.Combine(flagDirectory, $"{country.name.common}.png");

                    await DownloadFlagImageAsync(httpClient, country.flags.png, flagFilePath);

                    completedCount++;
                    int progressPercentage = (int)((double)completedCount / totalCount * 100);
                    progress.Report(progressPercentage);
                }
            }
        }
        /// <summary>
        /// Downloads a flag image asynchronously from the given URL and saves it to the specified file path.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for downloading the image.</param>
        /// <param name="imageUrl">The URL of the flag image.</param>
        /// <param name="filePath">The file path to save the downloaded image.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task DownloadFlagImageAsync(HttpClient httpClient, string imageUrl, string filePath)
        {
            using (HttpResponseMessage response = await httpClient.GetAsync(imageUrl))
            {
                if (response.IsSuccessStatusCode)
                {
                    using (Stream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }
                }
            }
        }
        /// <summary>
        /// Handles the selection change event of the country combo box.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private async void ComboCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedCountry = ComboCountries.SelectedItem as string;

            if (!string.IsNullOrEmpty(selectedCountry))
            {
                Country country = countries?.FirstOrDefault(c => c.name.common == selectedCountry);

                if (country != null)
                {
                    CountryNameHeader.Text = country.name.common;
                    CountryName.Text = "Name: " + country.name.common;
                    CountryCapital.Text = "Capital: " + (country.capital?.Count > 0 ? string.Join(", ", country.capital) : "N/A");
                    CountryRegion.Text = "Region: " + (country.region ?? "N/A");
                    CountrySubRegion.Text = "Subregion: " + (country.subregion ?? "N/A");
                    CultureInfo culture = CultureInfo.InvariantCulture;
                    CountryArea.Text = "Area: " + (country.area > 0 ? $"{country.area:#,##0.##} km2" : "N/A");
                    CountryPopulation.Text = "Population: " + (country.population > 0 ? $"{country.population:#,##0.##}" : "N/A");
                    var giniStrings = country.gini?.Select(entry => $"{entry.Value} ({entry.Key})") ?? Enumerable.Empty<string>();
                    string giniString = string.Join(Environment.NewLine, giniStrings);
                    CountryGini.Text = "Gini: " + (string.IsNullOrEmpty(giniString) ? "N/A" : giniString);
                    LoadFlagImage(country.name.common);

                    if (loadAPI) { 
                    string openStreetMapsUrl = country.maps.openStreetMaps;
                    if (!openStreetMapsUrl.StartsWith("http://") && !openStreetMapsUrl.StartsWith("https://"))
                    {
                        openStreetMapsUrl = "https://" + openStreetMapsUrl;
                    }
                    webView.NavigationCompleted += WebView_NavigationCompleted;
                    await webView.EnsureCoreWebView2Async();
                    webView.CoreWebView2.Navigate(openStreetMapsUrl);
                    }
                }
            }
        }
        /// <summary>
        /// Loads and displays the flag image for the specified country. If no flag image is found, displays the default image.
        /// </summary>
        /// <param name="countryName">The name of the country.</param>
        private void LoadFlagImage(string countryName)
        {
            string flagFileName = $"{countryName}.png";
            string flagFilePath = Path.Combine("Flags", flagFileName);

            if (File.Exists(flagFilePath))
            {
                var flagImage = new BitmapImage();
                flagImage.BeginInit();
                flagImage.CacheOption = BitmapCacheOption.OnLoad;
                flagImage.UriSource = new Uri(flagFilePath, UriKind.Relative);
                flagImage.EndInit();
                flagImage.Freeze(); // Make it accessible from the UI thread
                viewModel.SelectedCountryFlag = flagImage;
            }
            else
            {
                var defaultImage = new BitmapImage();
                defaultImage.BeginInit();
                defaultImage.CacheOption = BitmapCacheOption.OnLoad;
                defaultImage.UriSource = new Uri("flagnotdound.png", UriKind.Relative);
                defaultImage.EndInit();
                defaultImage.Freeze(); // Make it accessible from the UI thread
                viewModel.SelectedCountryFlag = defaultImage;
            }
        }
        /// <summary>
        /// Event handler for the navigation completed event of the WebView control.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // Inject JavaScript to hide unwanted elements and adjust height
            const string javascript = @"
                        var mapElement = document.getElementById('map');
                        if (mapElement) {
                            var elementsToHide = document.querySelectorAll('h1, header, .primary, .secondary, #sidebar');
                            for (var i = 0; i < elementsToHide.length; i++) {
                                elementsToHide[i].style.display = 'none';
                            }
                            mapElement.style.height = '400px'; // Adjust the desired height here
                        }
                        var bodyElement = document.querySelector('body');
                        if (bodyElement) {
                            bodyElement.style.marginTop = '0';
                            bodyElement.style.paddingTop = '0';
                        }
                        var contentElement = document.getElementById('content');
                        if (contentElement) {
                            contentElement.style.marginTop = '-55px'; // Adjust the desired offset here
                        }
                    ";

            await webView.ExecuteScriptAsync(javascript);
        }
    }
}