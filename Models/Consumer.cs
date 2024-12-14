namespace DanskLogistikAPI.Models
{
    public class Consumer
    {
        /// <summary>
        /// Principal ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Where are we physically connected to the network
        /// </summary>
        public int NodeId { get; set; }
        
        /// <summary>
        /// Human Friendly Name
        /// </summary>
        public string Name { get; set; } = null!;
    }
}
