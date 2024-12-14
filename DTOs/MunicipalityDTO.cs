namespace DanskLogistikAPI.DTOs
{
    public class MunicipalityDTO
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
        public int OwnerId { get; set; }

        /// <summary>
        /// De-Facto owner 
        /// 
        /// Just like the Paradox Games this system is inspired by, the database requires a single true de-Facto controller, and the system can not handle split ownership
        /// 
        /// For example, Crimea is De-Facto owned by Russia
        /// </summary>
        public int ControllerId { get; set; }


        /// <summary>
        /// Name of the municipality
        /// We will not check that Name is unique (though it really should be)
        /// Is allowed to be multiple words
        /// </summary>
        public string Name { get; set; } = null!;
    }
}
