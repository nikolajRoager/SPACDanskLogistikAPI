using System.ComponentModel.DataAnnotations;

namespace DanskLogistikAPI.Models
{
    /// <summary>
    /// A type of connection betwixt two nodes, with information about expect time
    /// 
    /// This version of the database does not access the municipalities it may or may not pass through (After all, it could be a sea or air connection)
    ///
    /// If the connection does use a fixed land connection, you should add individual nodes for each municipality we pass through
    /// 
    /// For instance, include a Railway junction in Assens which the train from Middelfart to Odense passes through (even if it doesn't stop there)
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// Principal ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Node at one end, the order of A and B does not matter
        /// </summary>
        public Node A { get; set; }
        /// <summary>
        /// Node at one end, the order of A and B does not matter
        /// </summary>
        public Node B { get; set; }
        
        /// <summary>
        /// What type of connection is this? Used for display, information and pathfinding purposes
        /// </summary>
        public enum Mode { Rail, Road, Air, Sea};

        /// <summary>
        /// What type of connection is this? Used for display and information purposes
        /// </summary>
        public Mode mode { get; set; }

        /// <summary>
        /// Connections may or may not have a Name, may be unique ("Storebæltsbroen") or not (E45 motorvej)
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// Estimated travel time, this is used for finding the best path, and closes warehouse, true shipment time may warry
        /// 
        /// this system DOES NOT understand that shipments happen on individual trains, trucks or planes which may depart at fixed times
        /// </summary>
        public TimeSpan? Time { get; set; }
    }
}
