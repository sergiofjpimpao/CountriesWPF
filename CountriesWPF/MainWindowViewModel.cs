using CountriesWPF.Modelos;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace CountriesWPF
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {

        private bool isLoading;
        private List<string> countryNames;
        private List<Country> countries;
        private string selectedCountry;
        private ImageSource selectedCountryFlag;
        public event PropertyChangedEventHandler PropertyChanged;
        private int progressValue;
        /// <summary>
        /// 
        /// </summary>
        public int ProgressValue
        {
            get { return progressValue; }
            set
            {
                if (progressValue != value)
                {
                    progressValue = value;
                    OnPropertyChanged(nameof(ProgressValue));
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// 
        /// </summary>
        public List<string> CountryNames
        {
            get { return countryNames; }
            set
            {
                if (countryNames != value)
                {
                    countryNames = value;
                    countryNames.Sort(); // Sort the list alphabetically
                    OnPropertyChanged(nameof(CountryNames));
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<Country> Countries
        {
            get { return countries; }
            set
            {
                if (countries != value)
                {
                    countries = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string SelectedCountry
        {
            get { return selectedCountry; }
            set
            {
                if (selectedCountry != value)
                {
                    selectedCountry = value;
                    OnPropertyChanged(nameof(SelectedCountry));
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public ImageSource SelectedCountryFlag
        {
            get => selectedCountryFlag;
            set
            {
                if (selectedCountryFlag != value)
                {
                    selectedCountryFlag = value;
                    OnPropertyChanged(nameof(SelectedCountryFlag));
                }
            }
        }
    }
}
