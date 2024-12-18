﻿namespace DanskLogistikAPI.Models
{
    /// <summary>
    /// A warehouse contains any number of products at a fixed location, Products can be send FROM warehouses
    /// </summary>
    public class ProductSource
    {
        /// <summary>
        /// Principal ID
        /// </summary>
        public int Id { get; set; }
        

        /// <summary>
        /// Warehouse may or may not have a given name (If not, an auto-generated name based on the Node will be used)
        /// Need not be unique
        /// </summary>
        public string? Name { get; set; }


        /// <summary>
        /// Where are we physically connected to the network?
        /// </summary>
        public Node Location { get; set; } = null!;
    }
}
