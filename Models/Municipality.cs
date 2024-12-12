namespace DanskLogistikAPI.Models
{
    /// <summary>
    /// The smallest administrative division, named Municipalities like in Denmark, but anything could work
    /// Used to auto-generate the map
    /// 
    /// I take some inspiration from older Paradox Interactive's Strategy games, and this is equivalent to "provinces" (e.g. Counties in CK2, Provinces in EU4, or provinces in Hoi4) 
    /// </summary>
    public class Municipality
    {
        /// <summary>
        /// Principal tag
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// De-Jure owner (UN Recognized owner)
        /// 
        /// Just like the Paradox Games this system is inspired by, the database requires a single true de-jure controller, and municipalities can not be partially owned
        /// 
        /// For example, Crimea is De-Jure owned by Ukraine
        /// </summary>
        public Country Owner { get; set; }

        /// <summary>
        /// De-Facto owner 
        /// 
        /// Just like the Paradox Games this system is inspired by, the database requires a single true de-Facto controller, and the system can not handle split ownership
        /// 
        /// For example, Crimea is De-Facto owned by Russia
        /// </summary>
        public Country Controller { get; set; }


        /// <summary>
        /// Name of the municipality
        /// We will not check that Name is unique (though it really should be)
        /// Is allowed to be multiple words
        /// </summary>
        public string Name { get; set; }

        //More data could be inserted as needed, e.g. Population
    }
}
