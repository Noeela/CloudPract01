using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Practicum.Models
{
    public class Device
    {

        [Key, MaxLength(30)] public string DeviceId { get; set; } = "";
        public string FieldName { get; set; } = "";
        public string Status { get; set; }= "Idle";

        public DateTime LastUpdated { get; set; }

    }
}
