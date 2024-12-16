using DanskLogistikAPI.DataAccess;
using DanskLogistikAPI.DTOs;
using DanskLogistikAPI.Models;
using DanskLogistikAPI.Repositories;
using DanskLogistikAPI.Services.SVGGenerator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.OpenApi.Models;

namespace DanskLogistikAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogisticController(ISVGGenerator MySVGGenerator,IMapRepository _MapRepository, IPathfinder _pathfinder) : ControllerBase
    {
        private readonly ISVGGenerator MySVGGenerator=MySVGGenerator;
        private readonly IMapRepository MapRepository=_MapRepository;
        private readonly IPathfinder pathfinder = _pathfinder;

        

        //Endpoints for SVG snippets, and setup of the map, Mainly for administration

        /// <summary>
        /// Mainly for administration, returns the Map with all municipalities marked as "occupied" by different colours, and all lines and random nodes selected
        /// Used so a graphics designer can verify that the uploaded map and graphics elements work
        /// </summary>
        /// <returns>SVG image content.</returns>
        /// <response code="200">Returns the SVG map</response>
        /// <response code="503">The database is missing required data to generate the map</response>
        [HttpGet("RawMap.svg")]
        public async Task<IActionResult> GetSVGMap()
        {

            var ExampleXml= await MySVGGenerator.GetMapString();
            return Content(ExampleXml,"image/svg+xml");
        }
        
        /// <summary>
        /// Get list of all SVG snippets (with names and content) stored in database
        /// </summary>
        /// <returns>List of SVG image content.</returns>
        /// <response code="200">Returns all the SVG snippets</response>
        /// <response code="204">No SVG images existed</response>
        [HttpGet("GetSVGs")]
        public async Task<ActionResult<IEnumerable<SVGSnippet>>> GetSVG()
        {

            var Result = await MapRepository.GetAllSnippetsAsync();

            if (Result.Count() == 0)
                return NoContent();
            else
                return Ok(Result);

        }
        
        /// <summary>
        /// Get SVG snippets by ID (with names and content) stored in database
        /// </summary>
        /// <returns>SVG image content.</returns>
        /// <response code="200">Returns SVG snippets</response>
        /// <response code="404">Not found </response>
        [HttpGet("SVG/get/{id}")]
        public async Task<ActionResult<SVGSnippet>> GetSVG(int id)
        {
            var Result = await MapRepository.GetSnippetAsync(id);

            if (Result == null)
                return NotFound();
            else
                return Ok(Result);
        }
        
        /// <summary>
        /// Get SVG snippets by Name (with ID and content) stored in database
        /// </summary>
        /// <returns>SVG image content.</returns>
        /// <response code="200">Returns SVG snippets</response>
        /// <response code="404">Not found </response>
        
        [HttpGet("SVG/name/{name}")]
        public async Task<ActionResult<SVGSnippet>> GetSVG(string name)
        {
            var Result = await MapRepository.GetSnippetAsync(name);

            if (Result == null)
                return NotFound();
            else
                return Ok(Result);
        }

        //Get the SVG as a viewable SVG outright
        [HttpGet("SVG/images/{name}.svg")]
        public async Task<IActionResult> GetSVGDisplayable(string name)
        {
            var Result = await MapRepository.GetSnippetAsync(name);

            if (Result == null)
                return NotFound();
            string Out;
            try
            {
                Out=await MySVGGenerator.CreateSingleDocument(Result.Content);
            }
            catch (Exception e)
            {
                return Problem("Could not convert snippet to SVG, error: "+e.Message);
            }

            return Content(Out,"image/svg+xml");


        }

        /// <summary>
        /// Try uploading this SVG to this name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <response code="201">Created object</response>
        /// <response code="403">The provided file or data failed one or more tests, or could not be saved</response>

        [HttpPost("SVG/upload")]
        public async Task<ActionResult<IEnumerable<SVGSnippet>>> PostSVG(string name, IFormFile file)
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
                try
                {
                    var New = await MapRepository.AddSnippetAsync(name,reader);
                    await MapRepository.SaveChanges();
                    return CreatedAtAction(nameof(GetSVG), new { Id = New.Id }, New);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
        }

        /// <summary>
        /// Delete an svg element
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpDelete("SVG/Delete/{id}")]
        public async Task<ActionResult> DeleteSVG(int id)
        {
            try
            {
                await MapRepository.DeleteSnippetAsync(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            await MapRepository.SaveChanges();
            
            return Ok();
        }

        //Wipe entire map
        [HttpDelete("DeleteAll")]
        public async Task<ActionResult> DeleteAll()
        {
            try
            {
                MapRepository.DeleteMap();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            await MapRepository.SaveChanges();
            
            return Ok();
        }

        /// <summary>
        /// Http Update function, replace the entire map, this obviously deletes all data
        /// </summary>
        /// <returns></returns>
        [HttpPut("Map/Upload")]
        public async Task<ActionResult<SVGSnippet>> PutMap(IFormFile file)
        {

            using (var reader = new StreamReader(file.OpenReadStream()))
                try
                {
                    MapRepository.DeleteMap();
                    await MySVGGenerator.LoadMapFromSVG(reader);
                    await MapRepository.SaveChanges();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
        }

        /// <summary>
        /// Http Update function, replace this snippet with the new one
        /// </summary>
        /// <param name="id"></param>
        /// <param name="New"></param>
        /// <returns></returns>
        [HttpPut("Put/{id}")]
        public async Task<ActionResult<SVGSnippet>> PutSvg(int id, SVGSnippet New)
        {
            if (id != New.Id)
            {
                return BadRequest();
            }
            try
            {
                //Call the repository update, and return a variety of errors if saving or finding the product failed
                var Updated = await MapRepository.UpdateSnippetAsync(New);
                if (Updated == null)
                {
                    return NotFound();
                }
                await MapRepository.SaveChanges();

                //If we got here it is ok, return that we modified this, and the destination to get it back
                return AcceptedAtAction(nameof(GetSVG), new {id=Updated.Id}, Updated);
            }
            catch (Exception e)
            {
                //This is mainly for debugging, I do not expect an end user to be able to understand this
                return BadRequest("There was an error updating the SVG, got serverside error: "+e.Message);
            }
        }




        [HttpPatch("Municipalities/{name}/set")]
        public async Task<ActionResult<MunicipalityDTO>> SetOwner(string name, [FromQuery] string? owner=null, [FromQuery] string? controller=null)
        {
            try
            {
                MunicipalityDTO Result = MapRepository.setMunicipalityOwner(name, owner, controller);
                await MapRepository.SaveChanges();

                return Ok(Result);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPatch("Municipalities/{country}/occupies")]
        public async Task<ActionResult<IEnumerable<MunicipalityDTO>>> SetOwner(string country, [FromQuery] List<string> municipalities)
        {
            try
            {
                List<MunicipalityDTO> Out=new();
                
                foreach(var name in municipalities)
                {
                    Out.Add(MapRepository.setMunicipalityOwner(name, null, country));

                }
                await MapRepository.SaveChanges();

                return Ok(Out);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpGet("GetMunicipality")]
        public async Task<ActionResult<IEnumerable<MunicipalityDTO>>> GetMunicipalities()
        {
            var Result = await MapRepository.GetAllMunicipalityDTOAsync();
            if (Result.Count() == 0)
                return NoContent();
            else
                return Ok(Result);
        }

        [HttpGet("GetNodes")]
        public async Task<ActionResult<IEnumerable<NodeDTO>>> GetNodes()
        {
            var Result = await MapRepository.GetAllNodeDTOAsync();
            if (Result.Count() == 0)
                return NoContent();
            else
                return Ok(Result);
        }

        [HttpGet("GetCountries")]
        public async Task<ActionResult<IEnumerable<NodeDTO>>> GetCountries()
        {
            var Result = await MapRepository.GetAllCountryDTOAsync();
            if (Result.Count() == 0)
                return NoContent();
            else
                return Ok(Result);
        }


        [HttpGet("GetConnections")]
        public async Task<ActionResult<IEnumerable<ConnectionDTO>>> GetConnections()
        {
            var Result = await MapRepository.GetAllConnectionDTOAsync();
            if (Result.Count() == 0)
                return NoContent();
            else
                return Ok(Result);
        }
        [HttpGet("GetConnectionMaps")]
        public async Task<ActionResult<IEnumerable<NodeMappingDTO>>> GetConnectionMaps()
        {
            var Result = await MapRepository.GetAllNodeMappingDTOAsync();
            if (Result.Count() == 0)
                return NoContent();
            else
                return Ok(Result);
        }


        [HttpGet("Connections/Get/{id}")]
        public async Task<ActionResult<ConnectionDTO>> GetConnection(int id)
        {
            var Result = await MapRepository.GetConnectionDTOAsync(id);

            if (Result == null)
                return NotFound();
            else
                return Ok(Result);
        }

        [HttpGet("Pathfind")]
        public async Task<ActionResult<TicketDTO>> Pathfind([FromQuery] string Start, [FromQuery]string Stop,[FromQuery] DateTime? StartTime=null,[FromQuery] bool allowFly=true, [FromQuery] bool allowSea=true, [FromQuery] bool allowRail=true, [FromQuery] bool allowRoad=true)
        {
            //If no time was supplied, use server time now
            if (StartTime == null)
                StartTime = DateTime.Now;

            try
            {
                var result =await pathfinder.findPath(Start,Stop,(DateTime)StartTime,allowFly,allowSea,allowRail,allowRoad);
                if (result == null)
                    return BadRequest("No path sattisfies the conditions");
                else
                {
                    var ticket = new TicketDTO(result);
                    return Ok(ticket);
                }
            }
            catch (Exception e)
            {
                return Problem("Pathfinder returned error: "+e.Message);
            }

        }
    }
}
