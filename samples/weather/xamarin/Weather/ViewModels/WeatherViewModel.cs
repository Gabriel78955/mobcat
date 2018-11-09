﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.MobCat;
using Microsoft.MobCat.MVVM;
using Weather.Services.Abstractions;
using Xamarin.Essentials;
using System.Linq;


namespace Weather.ViewModels
{
    public class WeatherViewModel : BaseViewModel
    {
        string _cityName;
        string _weatherDescription;
        string _backgroundImage;
        string _currentTemp;
        string _highTemp;
        string _lowTemp;
        bool _isCelsius;

        IForecastsService forecastsService; 
        IImageService imageService;
 
        public WeatherViewModel()
        {
            CityName = "London";
            IsCelsius = true;
            WeatherDescription = "Cloudy";
            CurrentTemp = "17";
            HighTemp = "20";
            LowTemp = "10";
            BackgroundImage = $"https://upload.wikimedia.org/wikipedia/commons/8/82/London_Big_Ben_Phone_box.jpg";
        }

        public string CityName
        {
            get { return _cityName; }
            set
            {

                RaiseAndUpdate(ref _cityName, value);
            }
        }

        public string CurrentTemp
        {
            get { return _currentTemp; }
            set
            {
                RaiseAndUpdate(ref _currentTemp, value);
            }
        }

        public string HighTemp
        {
            get { return _highTemp; }
            set
            {
                RaiseAndUpdate(ref _highTemp, value);
            }
        }

        public string LowTemp
        {
            get { return _lowTemp; }
            set
            {
                RaiseAndUpdate(ref _lowTemp, value);
            }
        }

        public string WeatherDescription
        {
            get { return _weatherDescription; }
            set
            {
                RaiseAndUpdate(ref _weatherDescription, value);
            }
        }

        public string BackgroundImage
        {
            get { return _backgroundImage; }
            set
            {
                RaiseAndUpdate(ref _backgroundImage, value);
            }
        }

        public bool IsCelsius
        {
            get { return _isCelsius; }
            set
            {
                RaiseAndUpdate(ref _isCelsius, value);
            }
        }

        public string TempSymbol
        {
            get { return IsCelsius ? "°C" : "°F"; }
        }

        public string Time
        {
            get { return DateTime.Now.ToShortTimeString(); }
        }

        

        public async override Task InitAsync()
        {
            forecastsService = ServiceContainer.Resolve<IForecastsService>();
            imageService = ServiceContainer.Resolve<IImageService>();

            try
            {
                // Use last known location for quicker resonse
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location == null)
                {
                    location = await Geolocation.GetLocationAsync();
                }

                if (location != null)
                {
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    var place = await Geocoding.GetPlacemarksAsync(location);
                    string city = place.FirstOrDefault()?.Locality;
                    CityName = city;
                    var londonForecast = await forecastsService.GetForecastAsync(city);

                    if (londonForecast != null)
                    {
                        var londonCityWeatherImage = await imageService.GetImageAsync(londonForecast.Name, londonForecast.Overview);
                        Debug.WriteLine($"{londonForecast.Name}: {londonForecast.CurrentTemperature}, {londonForecast.Overview}");
                        Debug.WriteLine(londonCityWeatherImage);
                        WeatherDescription = londonForecast.Overview;
                        CurrentTemp = londonForecast.CurrentTemperature;
                        HighTemp = londonForecast.MaxTemperature;
                        LowTemp = londonForecast.MinTemperature;
                    }
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
                CityName = "Unable to retrieve location - Feature not supported";
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                CityName = "Unable to retrieve location - Need permission";
            }
            catch (Exception ex)
            {
                // Unable to get location
            }
        }

    }
}