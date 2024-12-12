using DanskLogistikAPI.DataAccess;
using DanskLogistikAPI.Models;
using DanskLogistikAPI.Repositories;
using DanskLogistikAPI.Services.SVGGenerator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace DanskLogistikAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogisticController(ISVGGenerator MySVGGenerator,IMapRepository _MapRepository) : ControllerBase
    {
        private readonly ISVGGenerator MySVGGenerator=MySVGGenerator;
        private readonly IMapRepository MapRepository=_MapRepository;


        /// <summary>
        /// Mainly for administration, returns the Map with all municipalities marked as "occupied" by different colours, and all lines and random nodes selected
        /// Used so a graphics designer can verify that the uploaded map and graphics elements work
        /// </summary>
        /// <returns>SVG image content.</returns>
        /// <response code="200">Returns the SVG map</response>
        /// <response code="503">The database is missing required data to generate the map</response>
        [HttpGet("RawMap.svg")]
        public IActionResult GetSVGMap()
        {

            var ExampleXml= MySVGGenerator.GetMapString();
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
        [HttpGet("SVG/id/{id}")]
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
                    string content="";
                    while (reader.Peek() >= 0)
                        content+=reader.ReadLine();
                    var New = await MapRepository.AddSnippetAsync(name,content);

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
    }

}
