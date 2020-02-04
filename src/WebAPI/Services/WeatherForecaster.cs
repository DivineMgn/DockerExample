using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Models;

namespace WebAPI.Services
{
    public class WeatherForecaster : IWeatherForecaster
    {
        private static readonly string[] Summaries = new[] {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly Random _randomizer;

        public WeatherForecaster()
        {
            _randomizer = new Random();
        }

        public IEnumerable<WeatherForecast> PredictWeather(int forDays = 14)
        { 
            if (forDays <= 0)
                throw new ArgumentOutOfRangeException(nameof(forDays), "Can`t predict weather for less than 1 days.");

            var nowDate = DateTime.Now.Date;

            return Enumerable.Range(1, forDays).Select(index => new WeatherForecast {
                Date = nowDate.AddDays(index),
                TemperatureC = _randomizer.Next(-30, 40),
                Summary = Summaries[_randomizer.Next(Summaries.Length)]
            });
        }
    }
}
