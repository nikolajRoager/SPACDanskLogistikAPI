namespace DanskLogistikAPI.Models
{
    /// <summary>
    /// The ticket logs the transit of some delivery
    /// 
    /// It includes both future transport contracts, and logs of previous succesful or failed deliveries
    /// </summary>
    public class Ticket
    {
        public int id {  get; set; }

        /// <summary>
        /// What is the CURRENT status of this ticked?
        /// </summary>
        public enum StatusType {  Ongoing, Delivered, Lost}
        public StatusType status { get; set; }
        /// <summary>
        /// Where 
        /// </summary>
        public Node Destination { get; set; } = null!;

        /// <summary>
        /// How many places do we pass through?
        /// </summary>
        public int Steps { get; set; }

        /// <summary>
        /// The steps refer onwards to each other in a sort-off linked list structure 
        /// </summary>
        public TransitStep FirstStep { get; set; }= null!;

        /// <summary>
        /// If Status = Ongoing: What step is ongoing right now
        /// If Status = Lost: When was we last seen
        /// If Status = Delivered: simply = to last step in the chain
        /// </summary>
        public TransitStep CurrentStep { get; set; }= null!;
        


    }
}
