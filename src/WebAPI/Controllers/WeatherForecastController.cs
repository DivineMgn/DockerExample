﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IWeatherForecaster _weatherForecaster;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            IWeatherForecaster weatherForecaster
            )
        {
            _logger = logger;
            _weatherForecaster = weatherForecaster;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                return _weatherForecaster.PredictWeather();
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"Get weather forecast executed in {sw.Elapsed.TotalMilliseconds} ms.");
            }
        }
    }
}
