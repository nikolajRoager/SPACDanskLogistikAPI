using DanskLogistikAPI.DataAccess;
using DanskLogistikAPI.DTOs;
using DanskLogistikAPI.Models;
using DanskLogistikAPI.Services.SVGGenerator;
using Microsoft.EntityFrameworkCore;
using System.Xml;

namespace DanskLogistikAPI.Repositories
{
    /// <summary>
    /// Repository responsible for modifying the fixed map, this includes municipalities, countries, nodes and connections
    /// </summary>
    public interface IMapRepository
    {
        public void DeleteMap();

        /// <summary>
        /// Save all changes made since last time we saved
        /// </summary>
        /// <returns></returns>
        public Task SaveChanges();

        /// <summary>
        /// Return all nodes
        /// </summary>
        /// <returns></returns>
        public IQueryable<Node> GetAllNode();

        /// <summary>
        /// Return all municipalities
        /// </summary>
        /// <returns></returns>
        public IQueryable<Municipality> GetAllMunicipality();

        /// <summary>
        /// Return all countries
        /// </summary>
        /// <returns></returns>
        public IQueryable<Country> GetAllCountry();

        /// <summary>
        /// Return all nodes
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<NodeDTO>> GetAllNodeDTOAsync();

        /// <summary>
        /// Return all municipalities
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<MunicipalityDTO>> GetAllMunicipalityDTOAsync();

        /// <summary>
        /// Return all countries
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<CountryDTO>> GetAllCountryDTOAsync();

        /// <summary>
        /// Return all SVG snippets
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<SVGSnippet>> GetAllSnippetsAsync();

        /// <summary>
        /// Get SVG Snippet by Id number
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Task<SVGSnippet?> GetSnippetAsync(int Id);

        /// <summary>
        /// Get SVG Snippet by name
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Task<SVGSnippet?> GetSnippetAsync(string Name);

        /// <summary>
        /// Add to repository after verifying that
        /// 0) Name is unique
        /// 1) This is a valid xml file
        /// 2) The namespace is that of SVG
        /// 3) There is a Node with id=Name or inkscape:label=Name (in the latter case, we will rename it)
        /// 
        /// If this is not sattisfied, we throw an error
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public Task<SVGSnippet> AddSnippetAsync(string Name, StreamReader reader);

        /// <summary>
        /// Add to repository after verifying that
        /// 0) (If name has changed) Name is unique
        /// 1) This is a valid xml file
        /// 2) The namespace is that of SVG
        /// 3) There is a Node with id=Name or inkscape:label=Name (in the latter case, we will rename it)
        /// 
        /// If this is not sattisfied, we throw an error
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public Task<SVGSnippet?> UpdateSnippetAsync(SVGSnippet input);

        /// <summary>
        /// Delete svg by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task DeleteSnippetAsync(int id);

        
        /// <summary>
        /// Main loading function, deletes the saved map, including all countries, municipalities, Nodes and connections
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public Task LoadMap(StreamReader file);
        MunicipalityDTO setMunicipalityOwner(string municipality, string? owner, string? controller);



        public Task<SVGSnippet> AddOrUpdateSnippetAsync(SVGSnippet input);
        public Task<Node> AddOrUpdateNodeAsync(Node input);
        public Task<Municipality> AddOrUpdateMunicipalityAsync(Municipality input);
        public Task<Country> AddOrUpdateCountryAsync(Country input);

    }

    /// <summary>
    /// Repository responsible for modifying the fixed map, this includes municipalities, countries, nodes and connections
    /// </summary>
    public class MapRepository(LogisticContext _context) : IMapRepository
    {
        private readonly LogisticContext context = _context;
        /// <summary>
        /// Save all changes made since last time we saved
        /// </summary>
        /// <returns></returns>
        public async Task SaveChanges()
        {
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Return all SVG snippets
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SVGSnippet>> GetAllSnippetsAsync()
        {
            return await context.Snippets.ToListAsync();
        }

        public static MunicipalityDTO ToDTO(Municipality M)
        {
            return new MunicipalityDTO
            {
                Id = M.Id,
                ControllerId = M.Controller.Id,
                OwnerId = M.Owner.Id,
                Name = M.Name,
            };
        }
        public static NodeDTO ToDTO(Node N)
        {
            return new NodeDTO
            {
                isAirport=N.isAirport,
                Id = N.Id,
                LocationId = N.Location.Id,
                Name = N.Name
            };
        }
        public static CountryDTO ToDTO(Country C)
        {
            return new CountryDTO
            {
                Id = C.Id,
                Name = C.Name,
                Access = C.Access,
                DeJureOutlineId = C.DeJureOutline.Id

            };
        }

        /// <summary>
        /// Delete entire map, and all information requiring the map
        /// </summary>
        /// <returns></returns>
        public void DeleteMap()
        {
            context.Nodes.RemoveRange(context.Nodes);
            context.Connections.RemoveRange(context.Connections);
            context.municipalities.RemoveRange(context.municipalities);
            context.Countries.RemoveRange(context.Countries);
        }

        /// <summary>
        /// Return all municipalities
        /// </summary>
        /// <returns></returns>
        public IQueryable<Municipality> GetAllMunicipality()
        {
            return context.municipalities.Include(m => m.Owner).Include(m => m.Controller).Include(m=> m.Outline).AsQueryable();
        }

        /// <summary>
        /// Return all countries
        /// </summary>
        /// <returns></returns>
        public  IQueryable<Country> GetAllCountry()
        {
            return context.Countries.Include(c => c.DeJureOutline).AsQueryable();
        }

        /// <summary>
        /// Return all SVG snippets
        /// </summary>
        /// <returns></returns>
        public  IQueryable<Node> GetAllNode()
        {
            return context.Nodes.Include(N => N.Location).AsQueryable();
        }

        /// <summary>
        /// Return all municipalities
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<MunicipalityDTO>> GetAllMunicipalityDTOAsync()
        {
            return await (context.municipalities.Include(m => m.Owner).Include(m => m.Controller)).Select(M => ToDTO(M)).ToListAsync();
        }

        /// <summary>
        /// Return all countries
        /// </summary>
        /// <returns></returns>
        public  async Task<IEnumerable<CountryDTO>> GetAllCountryDTOAsync()
        {
            return await context.Countries.Include(c => c.DeJureOutline).Select(C => ToDTO(C)).ToListAsync();
        }

        /// <summary>
        /// Return all SVG snippets
        /// </summary>
        /// <returns></returns>
        public  async Task<IEnumerable<NodeDTO>> GetAllNodeDTOAsync()
        {
            return await context.Nodes.Include(N => N.Location).Select(N => ToDTO(N)).ToListAsync();
        }


        /// <summary>
        /// Get SVG Snippet by Id number
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<SVGSnippet?> GetSnippetAsync(int Id)
        {
            return await context.Snippets.FirstOrDefaultAsync(x => x.Id == Id);
        }

        /// <summary>
        /// Get SVG Snippet by name
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<SVGSnippet?> GetSnippetAsync(string Name)
        {
            return await context.Snippets.FirstOrDefaultAsync(x => x.Name.ToLower() == Name.ToLower());
        }

        /// <summary>
        /// Add to repository after verifying that
        /// 0) Name is unique
        /// 1) This is a valid xml file
        /// 2) The namespace is that of SVG
        /// 3) There is a Node with id=Name or inkscape:label=Name (in the latter case, we will rename it)
        /// 
        /// If this is not sattisfied, we throw an error
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public async Task<SVGSnippet> AddSnippetAsync(string Name, StreamReader reader)
        {
            SVGSnippet New;
            XmlDocument TempDoc;
            try
            {
                //First verification step, can we use loadXml
                TempDoc = new();
                string content="";
                string? line;
                while ((line=reader.ReadLine())!=null)
                {
                    content += line;
                }
                TempDoc.LoadXml(content);
            }
            catch (Exception E)
            {
                throw new Exception($"The object content was not a valid XML (required by SVG standard), parser returned error: ({E.Message})");
            }
            var root = TempDoc.DocumentElement;
            if (root?.Name != "svg")
                throw new Exception($"The object content is not an svg, root node svg not found");

            //For quickly finding the things in the xml document, create a namespace manager
            //I know what the structure of my template looks like
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(TempDoc.NameTable);
            nsmgr.AddNamespace("ns", "http://www.w3.org/2000/svg");
            //Inkscape
            nsmgr.AddNamespace("inkscape", "http://www.inkscape.org/namespaces/inkscape");

            var IdMatch = root.SelectSingleNode($"//ns:*[@id=\"{Name}\"]", nsmgr);

            if (IdMatch == null)
            {
                //Try inkscape label instead

                var LabelMatch = root.SelectSingleNode($"//ns:*[@inkscape:label=\"{Name}\"]", nsmgr);
                if (LabelMatch == null || LabelMatch.Attributes == null/*this is not null, but I just need to spell it out literally for the compiler*/)
                    throw new Exception($"The object is a valid SVG, but did not contain a node with \"id={Name}\" or \"inkscape:label={Name}\"");
                else
                {
                    LabelMatch.Attributes.SetNamedItem(TempDoc.CreateNode(XmlNodeType.Attribute, "id", "http://www.w3.org/2000/svg"));
                    IdMatch = LabelMatch;
                }

            }


            //Only store the matching node explicitly, we will use it to build up everything
            New = new SVGSnippet { Name = Name, Content = IdMatch.OuterXml };


            try
            {
                await context.Snippets.AddAsync(New);

                //Now save, that is up to whoever called this
                //await context.SaveChangesAsync();
            }
            catch (Exception E)
            {
                new Exception($"Error saving the SVG object: ({E.Message})");
            }

            return New;
        }

        /// <summary>
        /// Add to repository after verifying that
        /// 
        /// Version which takes a snippet, assuming that it has already been verified
        /// 
        /// 0) (If name has changed) Name is unique
        /// 1) This is a valid xml file
        /// 2) The namespace is that of SVG
        /// 3) There is a Node with id=Name or inkscape:label=Name (in the latter case, we will rename it)
        /// 
        /// If this is not sattisfied, we throw an error
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public async Task<SVGSnippet?> AddSnippetAsync(SVGSnippet input)
        {
            if (input != null)
            {
                //The tests we do here are a little simplified, since this is already our SVGSnippet object, it will not be a full file
                XmlDocument TempDoc;
                try
                {
                    //First verification step, can we use loadXml
                    TempDoc = new();
                    TempDoc.LoadXml(input.Content);
                }
                catch (Exception E)
                {
                    throw new Exception($"The object content was not a valid XML (required by SVG standard), parser returned error: ({E.Message})");
                }
                //This time the root is our target obejct
                var Node= TempDoc.DocumentElement;

                if (Node == null || Node.Attributes?["id"]?.Value!=input.Name)
                    throw new Exception($"The object did not contain a valid SVG node, with \"id={input.Name}\"");


                //Now add it at the back
                var New = new SVGSnippet { Name = input.Name, Content = input.Content};
                try
                {
                    await context.Snippets.AddAsync(New);

                    //Now save, that is up to whoever called this
                    //await context.SaveChangesAsync();
                }
                catch (Exception E)
                {
                    throw new Exception($"Error saving the SVG object: ({E.Message})");
                }


                return New;
            }
            else
                return null;
        }
        
        /// <summary>
        /// Look for something with this name and update it, or add a new one
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<SVGSnippet> AddOrUpdateSnippetAsync(SVGSnippet input)
        {
            var Entity = await context.Snippets.FirstOrDefaultAsync(a => a.Name == input.Name);
            if (Entity != null)
            {
                context.Snippets.Entry(Entity).State = EntityState.Modified;
                Entity.Name = input.Name;
                Entity.Content = input.Content;
                return Entity;
            }
            else
            {
                //Then add it to the back
                try
                {
                    await context.Snippets.AddAsync(input);
                    //Now save, that is up to whoever called this
                    //await context.SaveChangesAsync();
                }
                catch (Exception E)
                {
                    throw new Exception($"Error saving the SVG object: ({E.Message})");
                }
                return input;
            }
        }

        public async Task<Node> AddOrUpdateNodeAsync(Node input)
        {
            var Entity = await context.Nodes.FirstOrDefaultAsync(a => a.Name == input.Name);
            if (Entity != null)
            {
                context.Nodes.Entry(Entity).State = EntityState.Modified;
                Entity.Name = input.Name;
                Entity.isAirport = input.isAirport;
                Entity.x = input.x;
                Entity.y = input.y;
                Entity.Location = input.Location;
                return Entity;
            }
            else
            {
                //Then add it to the back
                try
                {
                    await context.Nodes.AddAsync(input);
                    //Now save, that is up to whoever called this
                    //await context.SaveChangesAsync();
                }
                catch (Exception E)
                {
                    throw new Exception($"Error saving the SVG object: ({E.Message})");
                }
                return input;
            }
        }

        public async Task<Municipality> AddOrUpdateMunicipalityAsync(Municipality input)
        {
            var Entity = await context.municipalities.FirstOrDefaultAsync(a => a.Name == input.Name);
            if (Entity != null)
            {
                context.municipalities.Entry(Entity).State = EntityState.Modified;
                Entity.Name = input.Name;
                Entity.Owner = input.Owner;
                Entity.Controller = input.Controller;
                Entity.Outline = input.Outline;
                return Entity;
            }
            else
            {
                //Then add it to the back
                try
                {
                    await context.municipalities.AddAsync(input);
                    //Now save, that is up to whoever called this
                    //await context.SaveChangesAsync();
                }
                catch (Exception E)
                {
                    throw new Exception($"Error saving the SVG object: ({E.Message})");
                }
                return input;
            }
        }

        public async Task<Country> AddOrUpdateCountryAsync(Country input)
        {
            var Entity = await context.Countries.FirstOrDefaultAsync(a => a.Name == input.Name);
            if (Entity != null)
            {
                context.Countries.Entry(Entity).State = EntityState.Modified;
                Entity.Name = input.Name;
                Entity.DeJureOutline = input.DeJureOutline;
                Entity.Access= input.Access;
                return Entity;
            }
            else
            {
                //Then add it to the back
                try
                {
                    await context.Countries.AddAsync(input);
                    //Now save, that is up to whoever called this
                    //await context.SaveChangesAsync();
                }
                catch (Exception E)
                {
                    throw new Exception($"Error saving the SVG object: ({E.Message})");
                }
                return input;
            }

        }


        /// <summary>
        /// Add to repository after verifying that
        /// 0) (If name has changed) Name is unique
        /// 1) This is a valid xml file
        /// 2) The namespace is that of SVG
        /// 3) There is a Node with id=Name or inkscape:label=Name (in the latter case, we will rename it)
        /// 
        /// If this is not sattisfied, we throw an error
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public async Task<SVGSnippet?> UpdateSnippetAsync(SVGSnippet input)
        {
            var Entity = await context.Snippets.FirstOrDefaultAsync(a => a.Id == input.Id);
            if (Entity != null)
            {
                context.Snippets.Entry(Entity).State = EntityState.Modified;
                //The tests we do here are a little simplified, since this is already our SVGSnippet object, it will not be a full file
                XmlDocument TempDoc;
                try
                {
                    //First verification step, can we use loadXml
                    TempDoc = new();
                    TempDoc.LoadXml(input.Content);
                }
                catch (Exception E)
                {
                    throw new Exception($"The object content was not a valid XML (required by SVG standard), parser returned error: ({E.Message})");
                }
                //This time the root is our target obejct
                var Node= TempDoc.DocumentElement;

                if (Node == null || Node.Attributes?["id"]?.Value!=input.Name)
                    throw new Exception($"The object did not contain a valid SVG node, with \"id={input.Name}\"");
                Entity.Name = input.Name;
                Entity.Content = input.Content;
            }
            else
                return null;
            return Entity;
        }

        /// <summary>
        /// Delete svg by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteSnippetAsync(int id)
        {
            var Entity = await context.Snippets.FirstOrDefaultAsync(a => a.Id == id);
            if (Entity != null)
            {
                context.Snippets.Remove(Entity);
            }
            else
                throw new ArgumentException($"Illegal ID:{id} to delete");
        }

        public async Task LoadMap(StreamReader file)
        {
            
            DeleteMap();
            
            await SaveChanges();
            

        }


        public MunicipalityDTO setMunicipalityOwner(string municipality, string? owner, string? controller)
        {
            var M = context.municipalities.Include(M=> M.Owner).Include(M=> M.Controller).FirstOrDefault(M=>M.Name==municipality);
            if (M == null)
                throw new ArgumentException($"Municipality {municipality} not found");
            if (owner!=null)
            {
                var C = context.Countries.FirstOrDefault(C => C.Name == owner);
                if (C == null)
                    throw new ArgumentException($"country {owner} not found");
                M.Owner = C;
            }
            if (controller!=null)
            {
                var C = context.Countries.FirstOrDefault(C => C.Name == controller);
                if (C == null)
                    throw new ArgumentException($"country {controller} not found");
                M.Controller = C;
            }

            return ToDTO(M);
        }
    }
}
