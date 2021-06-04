using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocTracker.Controllers {
   [ApiController]
   [Route("[controller]")]
   public class WeatherForecastController : ControllerBase {
      private static readonly string[] Summaries = new[]
      {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
      };
        

      private readonly ILogger<WeatherForecastController> _logger;
      private readonly IConfiguration _configuration;
      private string AppConfigVal;

      public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration) {
         _logger = logger;
         _configuration = configuration;
         AppConfigVal = configuration["TestKey"];
      }

      /// <summary>
      /// Gets 5 days of forecast starting from tomorrow
      /// </summary>
      /// <remarks>
      /// temperatures are random<br />
      /// summaries are random too and not really related to temperatures
      /// </remarks>
      [HttpGet]
      public IEnumerable<WeatherForecast> Get() {
         var rng = new Random();
         return Enumerable.Range(1, 5).Select(index => new WeatherForecast {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = rng.Next(-20, 55),
            Summary = Summaries[rng.Next(Summaries.Length)],
            ConfigVal = AppConfigVal
         })
         .ToArray();
      }
   }
}
