using DanskLogistikAPI.DataAccess;
using DanskLogistikAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml;

namespace DanskLogistikAPI.Repositories
{
    /// <summary>
    /// Repository responsible for modifying the fixed map, this includes municipalities, countries, nodes and connections
    /// </summary>
    public interface IMapRepository
    {

        /// <summary>
        /// Save all changes made since last time we saved
        /// </summary>
        /// <returns></returns>
        public Task SaveChanges();

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
        public Task<SVGSnippet> AddSnippetAsync(string Name, string input);

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
        public async Task<SVGSnippet> AddSnippetAsync(string Name, string input)
        {
            SVGSnippet New;
            XmlDocument TempDoc;
            try
            {
                //First verification step, can we use loadXml
                TempDoc = new();
                TempDoc.LoadXml(input);
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
                await context.AddAsync(New);

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
                SVGSnippet New;
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
    }
}
