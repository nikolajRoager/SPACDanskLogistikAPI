using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DanskLogistikAPI.Models
{
    /// <summary>
    /// An individual step of the entire transit, connected to adjacent steps in a single-linked list, since that makes it relatively easy to find in the database
    /// 
    /// The Transit step class does not know or care if it is used to plan a future travel, or log an existing travel
    /// </summary>
    public class TransitStep
    {
        [Key]
        public int Id { get; set; }

        //Depart through this connection from the start to the end node
        public NodeMapping Via {  get; set; } = null!;

        /// <summary>
        /// Used to EITHER store EXPECTED departure time OR log when we ACTUALLY left (If this is in the past)
        /// </summary>
        public DateTime Departure { get; set; }
        public TransitStep? next { get; set; } = null!;
    }
}
