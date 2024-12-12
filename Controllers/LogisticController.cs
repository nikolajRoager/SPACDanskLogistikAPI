using DanskLogistikAPI.Services;
using DanskLogistikAPI.Services.SVGGenerator;
using Microsoft.AspNetCore.Mvc;

namespace DanskLogistikAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(ISVGGenerator MySVGGenerator) : ControllerBase
    {
        private readonly ISVGGenerator MySVGGenerator=MySVGGenerator;

        /*
        [HttpGet]
        public async Task<ActionResult> GetAllProducts()
        {
        }
*/


            /*
        /// <summary>
        /// </summary>
        /// <returns>A list of <see cref="TBD"/> objects representing all .</returns>
        /// <response code="200">Returns the list of products.</response>
        /// <response code="500">If an error occurs while retrieving the products.</response>
        /// <response code="404">Not Found. The product with the specified ID does not exist.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<...>>> GetAllProducts()
        {
            try
            {
                var products = await _repository.GetAllProductsAsync();
                List<ProductDto> productDtos = products.Select(_productFactory.CreateProductDto).ToList();
                return Ok(productDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
            */
        /*
        /// <summary>
        /// Retrieves a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to retrieve.</param>
        /// <returns>An ActionResult containing the product with the specified ID.</returns>
        /// <response code="200">Returns the product with the specified ID.</response>
        /// <response code="404">Not Found. The product with the specified ID does not exist.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            try
            {
                var product = await _repository.GetProductAsync(id);

                if (product == null)
                {
                    return NotFound();
                }

                var productDto = _productFactory.CreateProductDto(product);
                return Ok(productDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="product">The product to create.</param>
        /// <returns>An ActionResult containing the created product.</returns>
        /// <response code="201">Created. The product was successfully created.</response>
        /// <response code="400">Bad Request. The product was not created.</response>
        [HttpPost]
        public async Task<ActionResult<ProductDto>> PostProduct(ProductDto product)
        {
            var newProduct = await _productFactory.CreateProductAsync(product);

            _repository.Add(newProduct);
            await _repository.SaveChanges();

            return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, product);
        }

        /// <summary>
        /// Updates a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="product">The updated product.</param>
        /// <returns>An ActionResult containing the updated product.</returns>
        /// <response code="204">No Content. The product was successfully updated.</response>
        /// <response code="400">Bad Request. The product was not updated.</response>
        /// <response code="404">Not Found. The product with the specified ID does not exist.</response>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> PutProduct(int id, ProductDto product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }
            var productToUpdate = await _repository.GetProductAsync(id);
            if (productToUpdate == null)
            {
                return NotFound();
            }

            var modifiedProduct = await _productFactory.CreateProductAsync(product);

            productToUpdate.ProductNumber = product.ProductNumber;
            productToUpdate.Name = product.Name;
            productToUpdate.Description = product.Description;
            productToUpdate.StockQuantity = product.StockQuantity;
            productToUpdate.ProductAttributes = modifiedProduct.ProductAttributes;

            _repository.UpdateProductAsync(productToUpdate);

            try
            {
                await _repository.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_repository.ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>An ActionResult indicating the result of the delete operation.</returns>
        /// <response code="204">No Content. The product was successfully deleted.</response>
        /// <response code="404">Not Found. The product with the specified ID does not exist.</response>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _repository.GetProductAsync(id);
            if(product == null)
            { 
                return NotFound(); 
            }
            _repository.Delete(product);
            await _repository.SaveChanges();
            
            return NoContent();
        }

        /// <summary>
        /// Search function which supports fuzzy search. There is only a single query, which we will search for in the name, category  and description (if the name, category, and description bools are set to do so)
        /// Be warned that searching description is slow
        /// 
        /// The fuzzy level tells us how willing we are to accept misspellings, the Ignore options allow us to accept any case, common typos (like number 0 instead of letter O), dublicate letters (like teling vs telling), or not penalise strings with different length
        /// 
        /// The search can also check descriptions for exact matches, or greater than or less than
        /// 
        /// The attribute can also refer to StockQuantity or productID
        /// </summary>
        /// <param name="Query">String we search for in the description or name, empty query accepts everything (if you only care about attributes)</param>
        /// <param name="Attributes"> A list containing Key=Value pairs</param>
        /// <param name="FuzzyLevel">Higher levels let more results through (WARNING HIGHER LEVEL MAKES THE SEARCH SLOWER)</param>
        /// <param name="IgnoreCase">Consider different cases same</param>
        /// <param name="IgnoreDuplicates">Disregard duplicate letters in misspelling (common mistake for Dyslexic people)</param>
        /// <param name="IgnoreLength">Use a constant penalty for different length words, instead of a cost per letter length difference</param>
        /// <param name="IgnoreCommonTypos">Disregard very common typos, such as swapped b and p or swapped 0 and O</param>
        /// <param name="Name">Search name for query</param>
        /// <param name="Description">Also search description. This is MUCH slower than Name, since descriptions tend to be longer</param>
        /// <returns></returns>
        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts(
            [FromQuery] List<string> Attributes,
            [FromQuery] string Query="",
            [FromQuery] FuzzyText.FuzzyComparer.Level FuzzyLevel=FuzzyText.FuzzyComparer.Level.Strict,
            [FromQuery] bool IgnoreCase=true,
            [FromQuery] bool IgnoreDuplicates=false,
            [FromQuery] bool Contains=true,
            [FromQuery] bool IgnoreCommonTypos=false,
            [FromQuery] bool Name=true,//Search name for the string
            [FromQuery] bool Description=false//also search description WARNING SLOWER THAN NAME!
            )
        {
            try
            {
                var products = await  _service.GetProductByFuzzySearch(Query,
                    Attributes,
                    FuzzyLevel,
                    IgnoreCase,
                    IgnoreDuplicates,
                    Contains,
                    IgnoreCommonTypos,
                    Name,
                    Description
                    );

                if (products == null || products.Count()==0)
                
                    return NotFound();
                else
                    return Ok(products.Select(_productFactory.CreateProductDto).ToList());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }*/
    }

}
