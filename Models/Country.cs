namespace DanskLogistikAPI.Models
{
    /// <summary>
    /// Someone who can own and control municipalities
    /// </summary>
    public class Country
    {
        /// <summary>
        /// Principal tag
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Country name
        ///We will not check that Name is unique (though it really should be)
        ///Is allowed to be multiple words
        ///Should be VARCHAR(64)
        ///That is more than enough, even if you are called:
        ///United Republics and Kingdoms of Turan, Iran, Iraq, and Hindustan
        /// </summary>
        public string Name { get; set; }

        public byte R {  get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        bool Access {  get; set; }
    }
}
