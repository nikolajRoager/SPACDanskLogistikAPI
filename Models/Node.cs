namespace DanskLogistikAPI.Models
{
    /// <summary>
    /// A node in the transport network, and a valid place for consumers and warehouses to be
    /// Can both represent individual cities, railway junctions, Airports, Islands, or important geographic features
    /// 
    /// Does NOT include physical location, as that is not needed in this database, all information about transportation distance or time is stored by the connection class
    /// </summary>
    public class Node
    {
        public bool isAirport {  get; set; }

        /// <summary>
        /// Principla ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Where is this node located
        /// Can be used to access municipality-wide stats, such as who owns this
        /// </summary>
        public Municipality Location { get; set; } = null!;

        /// <summary>
        /// Oft the name of the city, need not be unique 
        /// </summary>
        public string Name { get; set; } = null!;

        public float x { get; set; }
        public float y { get; set; }

        //Neighbouring nodes, and respective connections
        public ICollection<NodeMapping> Neighbors= null!;
    }
}
