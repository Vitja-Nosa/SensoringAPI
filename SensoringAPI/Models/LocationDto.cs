using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SensoringAPI.Models;

[Owned]
public class LocationDto
{
    [Range(-90, 90, ErrorMessage = "Latitude moet tussen -90 en 90 liggen.")]
    public double Latitude { get; set; } = 0;

    [Range(-180, 180, ErrorMessage = "Longitude moet tussen -180 en 180 liggen.")]
    public double Longitude { get; set; } = 0;
}

