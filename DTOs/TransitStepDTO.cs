using DanskLogistikAPI.Models;

namespace DanskLogistikAPI.DTOs
{
    public class TransitStepDTO
    {
        public int Id { get; set; }

        //I have unpacked some basic information, which the user normally wants to have handy
        public string FromName { get; set; } = null!;
        public int FromID {  get; set; }
        public string ToName { get; set; } = null!;
        public int ToID {  get; set; }
        public Connection.Mode Mode {  get; set; }
        public string? ConnectionName { get; set; }

        /// <summary>
        /// Used to EITHER store EXPECTED departure time OR log when we ACTUALLY left (If this is in the past)
        /// </summary>
        public DateTime Departure { get; set; }
    }
}
