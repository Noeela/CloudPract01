
using System.ComponentModel.DataAnnotations;

namespace Practicum.Models
{
    public class ViewModel
    {
        [Required]
        public string Username { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; } = "";

        [Required]
        public string Role { get; set; } = "Farmer";
    }

    public class TelemetryDto
    {
        public string DeviceId { get; set; } = "";

        public double SoilMoisture { get; set; }

        public double Temperature { get; set; }

        public DateTime ReadingTime { get; set; }
    }

    public class DashboardViewModel
    {
        public List<Device> Devices { get; set; } = new();

        public List<TelemetryDto> Telemetry { get; set; } = new();
    }

    public class BlobItemViewModel
    {
        public string Name { get; set; } = "";

        public string Url { get; set; } = "";
    }
}
