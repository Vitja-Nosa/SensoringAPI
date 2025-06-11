using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SensoringAPI.ModelsDto;

namespace SensoringAPI.Models;

public class WasteDetection
{
    public int Id { get; set; }

    [Required(ErrorMessage = "CameraId is verplicht.")]
    public string CameraId { get; set; }

    [Required(ErrorMessage = "Datum en tijd zijn verplicht.")]
    public DateTime DateTime { get; set; }

    [Required(ErrorMessage = "Locatie is verplicht.")]
    [FromBody]
    public required LocationDto Location { get; set; }

    [Required(ErrorMessage = "Type is verplicht.")]
    [AllowedValues("Glas", "Papier", "Plastic Fles", "Plastic Overig", "Rookwaar", "Blikje", "Gft", 
        ErrorMessage = "Ongeldig type afval. Toegestane waarden: Glas, Papier, Plastic Fles, Plastic Overig, Rookwaar, Blikje, Gft.")]
    public required string Type { get; set; }

    public int? WeatherId { get; set; }
    public WeatherData? WeatherData { get; set; }

    [Required(ErrorMessage = "Confidence is verplicht.")]
    [Range(0.0, 1.0, ErrorMessage = "Confidence moet tussen 0.0 en 1.0 liggen.")]
    public float Confidence { get; set; }
}
