using DanskLogistikAPI.Models;
using DanskLogistikAPI.Repositories;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Xml;

namespace DanskLogistikAPI.Services.SVGGenerator
{
    public class SVGGenerator(IMapRepository _mapRepository) : ISVGGenerator
    {
        IMapRepository mapRepository = _mapRepository;
        /// <summary>
        /// Get map as a string, which can be returned outright
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<string> GetMapString(/*Insert SVG arguments: Occupation, current position, current path*/)
        {
            var Doc = await CreateEmptySvg();

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(Doc.NameTable);
            nsmgr.AddNamespace("ns", "http://www.w3.org/2000/svg");

            var Layer0 = Doc.SelectSingleNode("//ns:g[@id=\"layer0\"]", nsmgr);

            //This is literally never null, but the compiler thinks it could be null, hence I place an unneccessary check
            if (Layer0 == null)
                throw new Exception($"template svg was missing layer0");

            var Countries = mapRepository.GetAllCountry();
            var Nodes = mapRepository.GetAllNode();
            var Municipalities = mapRepository.GetAllMunicipality();
            var Connections = mapRepository.GetAllConnection();

            foreach (Country C in Countries)
            {
                //Create xml nodes for de-jure background
                XmlDocumentFragment fragment = Doc.CreateDocumentFragment();
                fragment.InnerXml = C.DeJureOutline.Content;
                //The layer0 is not 0
                Layer0?.AppendChild(fragment);
            }

            //We are going to base municipalities on the occupied template element, but replace the path data with the path data for the municipality
            var occupiedSnippet = await mapRepository.GetSnippetAsync("occupied");

            //It is not, if it was we would not have gotten here, but whatever, got to keep the compiler happy
            if (occupiedSnippet == null)
                throw new Exception($"template svg snippet \"occupied\" was missing");


            var railSnippet = await mapRepository.GetSnippetAsync("Rail");
            var roadSnippet = await mapRepository.GetSnippetAsync("Road");
            var seaSnippet = await mapRepository.GetSnippetAsync("Sea");
            var airSnippet = await mapRepository.GetSnippetAsync("Air");

            if (railSnippet == null)
                throw new Exception($"template svg snippet \"Rail\" was missing");
            if (roadSnippet == null)
                throw new Exception($"template svg snippet \"Road\" was missing");
            if (seaSnippet == null)
                throw new Exception($"template svg snippet \"Sea\" was missing");
            if (airSnippet == null)
                throw new Exception($"template svg snippet \"Air\" was missing");

            foreach (Municipality M in Municipalities)
            {
                if (M.Owner != M.Controller)
                {
                    //Create xml nodes for de municipality 
                    XmlDocumentFragment municipalityShape = Doc.CreateDocumentFragment();
                    municipalityShape.InnerXml = M.Outline.Content;


                    //And the one we are copying from
                    XmlDocumentFragment occupationShape = Doc.CreateDocumentFragment();
                    occupationShape.InnerXml = occupiedSnippet.Content;


                    var OccDattr = occupationShape?.FirstChild?.Attributes?["d"];
                    var MunDattr = municipalityShape?.FirstChild?.Attributes?["d"];

                    var OccStyleattr = occupationShape?.FirstChild?.Attributes?["style"];

                    //The null tests are mainly to shut up the compiler
                    if (OccDattr == null)
                        throw new Exception($"template svg snippet \"occupied\" was missing path data, it must be a path");
                    if (OccStyleattr == null)
                        throw new Exception($"template svg snippet \"occupied\" was missing style data");
                    if (occupationShape == null)
                        throw new Exception($"template svg snippet \"{M.Name}\" was missing path data, it must be a path");


                    OccDattr.Value = MunDattr?.Value;
                    OccStyleattr.Value = OccStyleattr.Value.Replace("occupationTemplate", $"occupation{M.Controller.Id}");
                    Layer0?.AppendChild(occupationShape);
                }
            }


            //Display selected and unselected nodes
            foreach (Node N in Nodes)
            {

                //Add the 
                XmlDocumentFragment NodeRef = Doc.CreateDocumentFragment();
                NodeRef.InnerXml =
                    $"<use" +
                    $"  href=\"{(N.Location.Controller.Access ? "#node" : "#nodeOccupied")}\"" +
                    $"  x=\"{N.x}\"" +
                    $"  y=\"{N.y}\"" +
                    $"  id=\"{"Node" + N.Name}\"" +
                    $"  xmlns=\"http://www.w3.org/2000/svg\"" +
                    $"/>";
                Layer0?.AppendChild(NodeRef);

                if (N.isAirport && N.Location.Controller.Access)
                {
                    XmlDocumentFragment AirRef = Doc.CreateDocumentFragment();
                    AirRef.InnerXml =
                        $"<use" +
                        $"  href=\"#airplane\"" +
                        $"  x=\"{N.x}\"" +
                        $"  y=\"{N.y}\"" +
                        $"  id=\"{"Node" + N.Name}\"" +
                        $"  xmlns=\"http://www.w3.org/2000/svg\"" +
                        $"/>";
                    Layer0?.AppendChild(AirRef);

                }
            }

            //Display unselected connections

            foreach (Connection C in Connections)
            {
                XmlDocumentFragment fragment = Doc.CreateDocumentFragment();
                switch (C.mode)
                {
                    case Connection.Mode.Air:
                        fragment.InnerXml = airSnippet.Content;
                        break;
                    case Connection.Mode.Sea:
                        fragment.InnerXml = seaSnippet.Content;
                        break;
                    case Connection.Mode.Road:
                        fragment.InnerXml = roadSnippet.Content;
                        break;
                    case Connection.Mode.Rail:
                        fragment.InnerXml = railSnippet.Content;
                        break;
                }

                var d = fragment?.FirstChild?.Attributes?["d"];

                if (d != null)
                {
                    d.Value = $"m {C.A.x} {C.A.y} {C.B.x - C.A.x} {C.B.y - C.A.y}";
                }
                else
                    throw new ArgumentException($"The SVG Template did not have a Rail template element with x and y");

                //It is OBVIOUSLY not null, it it was the above check would have failed, I only try to get the compiler to STFU
                if (fragment != null)
                    Layer0?.AppendChild(fragment);

            }

            return Doc.OuterXml;
        }

        /// <summary>
        /// Overwrite the map database from an inkscape SVG (throw errors if any information is missing or corrupted)
        /// </summary>
        /// <param name="SVGData">Must be an INKSCAPE document obeying the detailedrequiremennts in the manual</param>
        /// <exception cref="NotImplementedException"></exception>
        public async Task LoadMapFromSVG(StreamReader file)
        {
            //First open the inkscape SVG map an XML, we are going to load countries, municipalities, and their shape from this file
            XmlDocument MapXml = new();
            {
                string content = "";
                string? line = "";
                while ((line = file.ReadLine()) != null)
                {
                    content += line;
                }
                MapXml.LoadXml(content);
            }



            //XML Nodes for the municipalities and country de-jure territories 
            List<XmlNode> MunicipalityNodes = new List<XmlNode>();
            List<XmlNode> CountryBackgroundNodes = new List<XmlNode>();




            //For quickly finding the things in the xml document, create a namespace manager
            //I know what the structure of my template looks like
            XmlNamespaceManager Map_nsmgr = new XmlNamespaceManager(MapXml.NameTable);
            Map_nsmgr.AddNamespace("ns", "http://www.w3.org/2000/svg");
            Map_nsmgr.AddNamespace("inkscape", "http://www.inkscape.org/namespaces/inkscape");

            //We will update the repository only after everything has been loaded here
            List<Node> NewNodes = new();
            List<Connection> NewConnections = new();
            List<Municipality> NewMunicipalities = new();
            List<Country> NewCountries = new();
            List<SVGSnippet> NewSnippets = new();

            var MapRoot = MapXml.DocumentElement;

            if (MapRoot == null)
                throw new ArgumentException("The SVG file did not contain a name");
            var MapAttributes = MapRoot.Attributes;
            if (MapAttributes == null)
                throw new ArgumentException("The SVG file root did not have attributes ");

            XmlNode? countries = MapRoot.SelectSingleNode(@"//ns:g[@id=""Countries""]", Map_nsmgr);
            if (countries == null)
                countries = MapRoot.SelectSingleNode(@"//ns:g[@inkscape:label=""Countries""]", Map_nsmgr);


            if (countries == null)
                throw new ArgumentException("The SVG file root did not contain a group with id, or inkscabe:label Countries");

            //Loop through all countries
            foreach (XmlNode country in countries.ChildNodes)
            {
                if (country.Attributes == null)
                    throw new ArgumentException("The SVG file contained a country without attributes, this is not allowed");

                //Get access status and country name 
                var countryName = country.Attributes["inkscape:label"];
                var countryStatus = country.Attributes["access"];

                if (countryName == null)
                    countryName = country.Attributes["id"];

                if (countryName == null)
                    throw new ArgumentException("The SVG file contained a country with no inkscape:label attribute");


                bool access = false;
                if (countryStatus != null && countryStatus.Value == "true")
                    access = true;


                XmlNode? DeJureBackground = country.SelectSingleNode(@"ns:*[@id=""" + countryName.Value + @"_Background""]", Map_nsmgr);

                if (DeJureBackground == null)
                {
                    DeJureBackground = country.SelectSingleNode(@"ns:*[@inkscape:label=""Background""]", Map_nsmgr);
                    if (DeJureBackground == null)
                        throw new ArgumentException($"The SVG file contained a country {countryName.Value} with no child with Id {countryName.Value}_Background or inkscape:label=Background");

                    //This is not null, if we have found the inkscape label, we know it exist, the ? gets the compiler to stfu
                    var newId = MapXml.CreateNode(XmlNodeType.Attribute, "id", "http://www.w3.org/2000/svg");
                    newId.Value = $"id=\"" + countryName.Value + "_Background\"";
                    DeJureBackground?.Attributes?.SetNamedItem(newId);

                }

                //Null check is not needed, but the compiler thinks this might be null, it just removes the warnings
                if (DeJureBackground == null)
                    throw new ArgumentException($"The SVG file contained a country {countryName.Value} with no child with Id {countryName.Value}_Background or inkscape:label=Background");



                var BackgroundSnippet = new SVGSnippet { Name = countryName.Value + "_background", Content = DeJureBackground.OuterXml };

                NewSnippets.Add(BackgroundSnippet);

                if (BackgroundSnippet == null)
                    throw new ArgumentException($"Could not add SVG background for {countryName.Value}");


                var thisCountry = new Country
                {
                    Access = access,
                    DeJureOutline = BackgroundSnippet,
                    Name = countryName.Value
                };

                NewCountries.Add(thisCountry);

                //Loop through all children of the country, check if they are municipalities, or if they are our de-jure background
                foreach (XmlNode municipality in country.ChildNodes)
                {
                    if (municipality == null || municipality.Attributes == null)
                        throw new ArgumentException($"The SVG file contained a municipality in {countryName.Value} with no inkscape:label or id");


                    //Get name from label or id, check label first
                    var MNameAttr = municipality.Attributes["inkscape:label"];
                    if (MNameAttr == null)
                    {
                        MNameAttr = municipality.Attributes["id"];
                        if (MNameAttr == null)
                            throw new ArgumentException($"The SVG file contained a municipality in {countryName.Value} with no inkscape:label or id");
                    }
                    else
                    {
                        //Move from label to id for consistency
                        var newId = MapXml.CreateNode(XmlNodeType.Attribute, "id", "http://www.w3.org/2000/svg");
                        newId.Value = $"id=\"" + MNameAttr.Value + "\"";
                        municipality.Attributes.SetNamedItem(newId);
                    }

                    if (municipality.Attributes["style"] == null)
                        throw new ArgumentException($"The SVG file contained a municipality in {countryName.Value} with no style");

                    //Verify that this is not the backgorund we already added
                    string MName = MNameAttr.Value;
                    if (MName.ToLower() != "background" && MName.ToLower() != countryName.Value + "_background")
                    {
                        //Check for dublicate municipalities
                        if (NewMunicipalities.Count(m => m.Name == MName) > 0)
                            throw new ArgumentException($"The SVG file contained multiple municipalities named {MName}");

                        var MunicipalitySnippets = new SVGSnippet { Name = MName, Content = municipality.OuterXml };
                        NewSnippets.Add(MunicipalitySnippets);

                        if (MunicipalitySnippets == null)
                            throw new ArgumentException($"The municipality shape named {MName} could not be added");

                        var ThisMunicipality = new Municipality
                        {
                            Name = MName,
                            Controller = thisCountry,
                            Owner = thisCountry,
                            Outline = MunicipalitySnippets
                        };

                        NewMunicipalities.Add(ThisMunicipality);
                        MunicipalityNodes.Add(municipality);
                    }
                }
            }

            //Now add all nodes
            XmlNode? AllNodes = MapRoot.SelectSingleNode(@"//ns:g[@inkscape:label=""Nodes""]", Map_nsmgr);
            if (AllNodes == null)
                AllNodes = MapRoot.SelectSingleNode(@"//ns:g[@id=""Nodes""]", Map_nsmgr);
            if (AllNodes == null)
                throw new Exception("The SVG map file did not contain a layer with inkscape:label or id = Nodes ");
            List<Node> Airports = new();

            //This layer contains all Nodes (i.e. transport junctions, most often cities)
            //Loop through all Nodes
            foreach (XmlNode Node in AllNodes.ChildNodes)
            {
                if (Node != null)
                {
                    var cx = Node.Attributes?["cx"]?.Value;
                    var cy = Node.Attributes?["cy"]?.Value;
                    var Name = Node.Attributes?["inkscape:label"]?.Value;
                    //If no municipality is defined, just assume that it is the same as the node name
                    var Municipality = Node.Attributes?["municipality"]?.Value;
                    if (Municipality == null)
                        Municipality = Name;
                    if (float.TryParse(cx, out float x) && float.TryParse(cy, out float y) && Name != null && Municipality != null)
                    {
                        //Now check that the Node is in a legal location, and if yes, put it there
                        var myMunicipality = NewMunicipalities.FirstOrDefault(M => M.Name == Municipality);
                        if (myMunicipality == null)
                            throw new ArgumentException($"ERROR Municipality {Municipality} for {Name} does not exist");

                        if (NewNodes.Any(N => N.Name == Name))
                            throw new ArgumentException($"The SVG file contained multiple nodes named {Name}");

                        bool airport = Node.Attributes?["airport"]?.Value == "true";//The attribute is likely not there for something which isn't an airport
                        var node = new Node
                        {
                            Name = Name,
                            Location = myMunicipality,
                            isAirport = airport,
                            x = x,
                            y = y,
                            Neighbors = new Collection<NodeMapping>()
                        };
                        if (airport)
                            Airports.Add(node);
                        NewNodes.Add(node);
                    }
                    else
                        throw new ArgumentException($"The Node {(Name == null ? "null" : Name)} is missing cx, cy, or inkscape:label attribute");
                }
            }

            //Finally add Connections
            XmlNode? AllConnections = MapRoot.SelectSingleNode(@"//ns:g[@inkscape:label=""Connections""]", Map_nsmgr);
            if (AllConnections == null)
                AllConnections = MapRoot.SelectSingleNode(@"//ns:g[@id=""Connections""]", Map_nsmgr);

            if (AllConnections == null)
                throw new Exception("The SVG map file did not contain a layer with inkscape:label or id = Nodes ");



            if (!float.TryParse(AllConnections?.Attributes?["KmPerMM"]?.Value, out float KmPerSVGmm))
                throw new Exception("The Connection element did not contain attribute KmPerMM");
            if (!float.TryParse(AllConnections?.Attributes?["railSpeed"]?.Value, out float railSpeed))
                throw new Exception("The Connection element did not contain attribute railSpeed");
            if (!float.TryParse(AllConnections?.Attributes?["roadSpeed"]?.Value, out float roadSpeed))
                throw new Exception("The Connection element did not contain attribute railSpeed");
            if (!float.TryParse(AllConnections?.Attributes?["airSpeed"]?.Value, out float airSpeed))
                throw new Exception("The Connection element did not contain attribute railSpeed");
            if (!float.TryParse(AllConnections?.Attributes?["seaSpeed"]?.Value, out float seaSpeed))
                throw new Exception("The Connection element did not contain attribute railSpeed");

            //The Connection contains any number of collection lines possibly with multiple stop, any two
            foreach (XmlNode ThisConnection in AllConnections.ChildNodes)
            {

                Connection.Mode type;

                float speed;
                switch (ThisConnection.Name)
                {
                    case "rail":
                        type = Connection.Mode.Rail;
                        speed = railSpeed;
                        break;
                    case "road":
                        type = Connection.Mode.Road;
                        speed = roadSpeed;
                        break;
                    case "sea":
                        type = Connection.Mode.Sea;
                        speed = seaSpeed;
                        break;
                    case "air":
                        throw new Exception("The Connections contained an air connection, that is not allowed as they are auto-generated");
                    default:
                        continue;//Skip invalid, as it may be whitespaces
                }


                //Get all stops in the connection
                var stops = ThisConnection.SelectNodes("ns:stop", Map_nsmgr);
                var Name = ThisConnection?.Attributes?["name"]?.Value;

                if (stops != null)
                {
                    var NodeA = stops[0]?.Attributes?["node"]?.Value;//Previous stop on this railway
                    for (int i = 1; i < stops.Count; i++)
                    {
                        var NodeB = stops[i]?.Attributes?["node"]?.Value;
                        if (NodeA != null && NodeB != null)
                        {
                            if (!NewConnections.Any(C => C.mode == type && ((C.A.Name == NodeA && C.B.Name == NodeB) || (C.B.Name == NodeA && C.A.Name == NodeB))))
                            {
                                var A = NewNodes.FirstOrDefault(N => N.Name == NodeA);
                                var B = NewNodes.FirstOrDefault(N => N.Name == NodeB);

                                if (A == null)
                                    throw new ArgumentException($"The SVG file requested a railway from non-existing node {NodeA}");
                                if (B == null)
                                    throw new ArgumentException($"The SVG file requested a railway from non-existing node {NodeB}");

                                //Get distance betwixt nodes, assuming straight line paths
                                float dist = MathF.Sqrt(MathF.Pow(A.y - B.y, 2) + MathF.Pow(A.x - B.x, 2))*KmPerSVGmm;
                                var Con = new Connection
                                {
                                    A = A,
                                    B = B,
                                    mode = type,
                                    Name = (Name == null ? $"{type}: {NodeA}, {NodeB}" : Name),
                                    Time = TimeSpan.FromHours(dist / speed)
                                };
                                NewConnections.Add(Con);
                                
                                //Add mappings between nodes and connections
                                A.Neighbors.Add(
                                    new NodeMapping
                                    {
                                        Connection = Con,
                                        End = B,
                                        Start = A

                                    });
                                B.Neighbors.Add(
                                new NodeMapping
                                {
                                    Connection = Con,
                                    End = A,
                                    Start = B
                                });
                                }
                        }
                        NodeA = NodeB;//Now this stop is the previous stop
                    }

                }
            }

            //Now auto-generate air-connections, it is assumed you can fly between all directly
            for (int i = 0; i < Airports.Count(); ++i)
                for (int j = 0; j < i; ++j)
                {
                    var A = Airports[i];
                    var B = Airports[j];



                    //Get distance betwixt nodes, assuming straight line paths
                    float dist = MathF.Sqrt(MathF.Pow(A.y - B.y, 2) + MathF.Pow(A.x - B.x, 2))*KmPerSVGmm;
                    var Con = new Connection
                    {
                        A = A,
                        B = B,
                        mode = Connection.Mode.Air,
                        Name = $"Plane between {B.Name} and {B.Name}",
                        Time = TimeSpan.FromHours(dist / airSpeed)
                    };
                    NewConnections.Add(Con);
                    //Add mappings between nodes and connections
                    B.Neighbors.Add(
                        new NodeMapping
                        {
                            Connection = Con,
                            End = A,
                            Start = B,
                        });
                    A.Neighbors.Add(
                        new NodeMapping
                        {
                            Connection = Con,
                            End = B,
                            Start = A,
                        });
                }


            //Now update everything
            //First delete all the existing, to make room for the new
            mapRepository.DeleteMap();
            await mapRepository.SaveChanges();

            for (int i = 0; i < NewSnippets.Count(); ++i)
            {
                NewSnippets[i] = await mapRepository.AddOrUpdateSnippetAsync(NewSnippets[i]);
            }
            for (int i = 0; i < NewCountries.Count(); ++i)
            {
                NewCountries[i] = await mapRepository.AddOrUpdateCountryAsync(NewCountries[i]);
            }
            for (int i = 0; i < NewMunicipalities.Count(); ++i)
            {
                NewMunicipalities[i] = await mapRepository.AddOrUpdateMunicipalityAsync(NewMunicipalities[i]);
            }
            for (int i = 0; i < NewNodes.Count(); ++i)
            {
                NewNodes[i] = await mapRepository.AddOrUpdateNodeAsync(NewNodes[i]);
            }
            for (int i = 0; i < NewConnections.Count(); ++i)
            {
                NewConnections[i] = await mapRepository.AddOrUpdateConnectionAsync(NewConnections[i]);
            }



        }

        /// <summary>
        /// Create an empty Xml Document, which is a valid SVG, and which has all the required Definitions in <defs />, this throws errors if the database does not contain the required SVG snippets
        /// </summary>
        /// <returns></returns>
        private async Task<XmlDocument> CreateEmptySvg()
        {
            //Thie header works well enough
            string HeaderAndRoot = $"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n" +
            $"<!--AUTOGENERATED municipality map -->\n" +
            $"<svg\n" +
            //SIZE DOES NOT MATTER, The frontend can and should auto-resize, it is easier to do in the browser than on my part
            $"   width=\"318mm\"\n" +
            $"   height=\"234mm\"\n" +
            $"   viewBox=\"0 0 318 234\"\n" +
            $"   version=\"1.1\"\n" +
            $"   id=\"svg1\"\n" +
            $"   xmlns=\"http://www.w3.org/2000/svg\">\n" +
            $"   <defs id=\"defs1\"/>" +
            $"   <g id=\"layer0\"/>" +
            $"</svg>\n";
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(HeaderAndRoot);


            XmlNamespaceManager nsmgr = new XmlNamespaceManager(Doc.NameTable);
            nsmgr.AddNamespace("ns", "http://www.w3.org/2000/svg");
            //Create defs, it is not null, i LITERALLY JUST CREATED IT
            var defs = Doc.SelectSingleNode("//ns:defs", nsmgr);
            if (defs == null)
                throw new Exception("Concratulations!, you are watching an error message, which I thought was 100% impossible to ever trigger: Somehow, the internal xml generator believes the <defs> element does not exist, 3 lines after generating it, that should never ever happe..");
            //Now load all the basic settings, and put them in defs, that allows us to refer back to them, without actually showing them in the svg:
            //Including them in defs might make small svgs a little bloated, but it will MASSIVELY reduce the size of large maps

            //Let us make a quick function for loading something from the database into the defs
            var addDef = async (string name) =>
            {
                var snippet = await mapRepository.GetSnippetAsync(name);

                if (snippet == null)
                    throw new Exception($"template svg snippet \"{name}\" was missing");
                XmlDocumentFragment fragment = Doc.CreateDocumentFragment();
                fragment.InnerXml = snippet.Content;
                defs.AppendChild(fragment);

            };

            //Wait for everything to be found and added, unfortunately the names are hardcoded

            //This does not work, the repository is not threading friendly
            //List<Task> addDefs = new List<Task> { addDef("occupationTemplate"), addDef("occupied"), addDef("background"), addDef("node"), addDef("airplane"), addDef("rail"),addDef("road"),addDef("sea")};
            //await Task.WhenAll(addDefs);
            await addDef("occupied");
            await addDef("background");
            await addDef("node");
            await addDef("nodeOccupied");
            await addDef("airplane");
            await addDef("rail");
            await addDef("road");
            await addDef("sea");


            //We will need one occupation definition per country, this will be based on the occupation template, but we will need to extract and use the colour from the country background
            var snippet = await mapRepository.GetSnippetAsync("occupationTemplate");

            if (snippet == null)
                throw new Exception($"template svg snippet \"occupationTemplate\" was missing");

            var Countries = mapRepository.GetAllCountry();

            foreach (var country in Countries)
            {
                //I think the easiest thing to do is to regex catch anything which looks like fill:#******; directly from the outline as a string
                //Catch 6 occurances of eutger 0 to 9, A to F, or a to f, seperated by ;
                string pattern = @"fill:#([0-9A-Fa-f]{6})";
                Match match = Regex.Match(country.DeJureOutline.Content, pattern);
                string countryFill = match.Success ? match.Value : @"fill:#012345";

                XmlDocumentFragment OccupationFragment = Doc.CreateDocumentFragment();
                OccupationFragment.InnerXml = Regex.Replace(Regex.Replace(snippet.Content, pattern, countryFill), "occupationTemplate", "occupation" + country.Id);

                //This is not null, I just want the warnings to SHUT UP
                if (OccupationFragment != null)
                    defs.AppendChild(OccupationFragment);
            }



            return Doc;
        }

        /// <summary>
        /// Create a faux document around this string, which presumably is some kind of SVG element, using all required info in our database
        /// </summary>
        /// <returns>A string, with the xml data, it is a string for easy upload</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<string> CreateSingleDocument(string subject)
        {
            var Doc = await CreateEmptySvg();

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(Doc.NameTable);
            nsmgr.AddNamespace("ns", "http://www.w3.org/2000/svg");

            var Layer0 = Doc.SelectSingleNode("//ns:g[@id=\"layer0\"]", nsmgr);


            XmlDocumentFragment fragment = Doc.CreateDocumentFragment();
            fragment.InnerXml = subject;

            //It is not null, I know they are there, because I made it, I just check it to make the compiler shut up
            Layer0?.AppendChild(fragment);
            if (Layer0 != null)
                Doc.DocumentElement?.AppendChild(Layer0);

            return Doc.OuterXml;
        }
    }
}
