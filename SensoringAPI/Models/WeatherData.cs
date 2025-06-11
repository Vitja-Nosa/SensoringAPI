using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SensoringAPI.ModelsDto;

namespace SensoringAPI.Models
{
    public class WeatherData
    {
        public int Id { get; set; }
        public double Temperature { get; set; }

        [Required(ErrorMessage = "Locatie is verplicht.")]
        [FromBody]
        public required LocationDto Location { get; set; }

        [Required(ErrorMessage = "Weersomstandigheid is verplicht.")]
        [AllowedValues("Zonnig", "Regenachtig", "Bewolkt", "Sneeuw", "Onweer", "Mistig", "Winderig", null,
       ErrorMessage = "Ongeldige weersomstandigheid. Kies uit: Zonnig, Regenachtig, Bewolkt, Sneeuw, Onweer, Mistig, Winderig.")]
        public string WeatherCondition { get; set; }
        public DateTime Time { get; set; }
    }
}
