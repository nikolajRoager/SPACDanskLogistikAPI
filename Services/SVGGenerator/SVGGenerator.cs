namespace DanskLogistikAPI.Services.SVGGenerator
{
    public class SVGGenerator : ISVGGenerator
    {


        public Stream GetSVGStream(/*Insert SVG arguments: Occupation, current position, current path*/)
        {
            throw new NotImplementedException();
        }
        
        //Overwrite the map database from an inkscape SVG (throw errors if any information is missing or corrupted)
        public void LoadMapFromSVG(Stream SVG)
        {

        }
    }
}
