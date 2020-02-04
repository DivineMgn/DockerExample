using System.Collections.Generic;
using WebAPI.Models;

namespace WebAPI.Services
{
    public interface IWeatherForecaster
    { 
        public IEnumerable<WeatherForecast> PredictWeather(int forDays = 14);
    }
}
