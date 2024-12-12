using System.Xml;

namespace DanskLogistikAPI.Services.SVGGenerator
{
    public interface ISVGGenerator
    {
        /// <summary>
        /// Get map as a string, which can be returned outright
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetMapString(/*Insert SVG arguments: Occupation, current position, current path*/);

        /// <summary>
        /// Overwrite the map database from an inkscape SVG (throw errors if any information is missing or corrupted)
        /// </summary>
        /// <param name="SVGData">Must be an INKSCAPE document obeying the detailedrequiremennts in the manual</param>
        /// <exception cref="NotImplementedException"></exception>
        public void LoadMapFromSVG(StreamReader SVGData);


        /// <summary>
        /// Create a faux document around this string, which presumably is some kind of SVG element, using all required info in our database
        /// </summary>
        /// <returns>A string, with the xml data, it is a string since the database elements want SVG as strings</returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<string> CreateSingleDocument(string subject);


    }
}
