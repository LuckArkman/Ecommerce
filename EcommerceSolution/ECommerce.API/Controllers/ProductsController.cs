using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Interfaces;
using ECommerce.Models.DTOs.Product;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Para IFormFile

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IShippingService _shippingService;

        public ProductsController(IProductService productService, IFileUploadService fileUploadService, IShippingService shippingService)
        {
            _productService = productService;
            _fileUploadService = fileUploadService;
            _shippingService = shippingService;
        }

        // GET: api/Products (Lista todos os produtos)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts([FromQuery] ProductQueryParams queryParams)
        {
            var products = await _productService.GetAllProductsAsync(queryParams);
            return Ok(products);
        }

        // GET: api/Products/{id} (Obtém um produto por ID)
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        // GET: api/Products/categories (Obtém todas as categorias)
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _productService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/Products/CheckShipping?productId=X&zipCode=Y (Verifica frete grátis)
        [HttpGet("CheckShipping")]
        [AllowAnonymous] // Pode ser acessado por não logados
        public async Task<ActionResult<bool>> CheckShipping(int productId, string zipCode)
        {
            var isFree = await _shippingService.IsFreeShipping(productId, zipCode);
            return Ok(isFree);
        }

        // POST: api/Products (Adicionar Novo Produto)
        // Usa [FromForm] para lidar com dados de formulário e IFormFile.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> PostProduct([FromForm] ProductDto productDto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Se um arquivo de imagem foi enviado, faça o upload e atualize a URL
            if (imageFile != null)
            {
                productDto.ImageUrl = await _fileUploadService.UploadFileAsync(imageFile, "product_images");
            }
            // Se imageFile for nulo, productDto.ImageUrl virá do formulário (pode ser vazio ou uma URL alternativa)

            var newProduct = await _productService.AddProductAsync(productDto);
            return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, newProduct);
        }
        
        // PUT: api/Products/{id} (Atualizar Produto Existente)
        // Usa [FromForm] para lidar com dados de formulário e IFormFile.
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutProduct(int id, [FromForm] ProductDto productDto, IFormFile? imageFile)
        {
            if (id != productDto.Id) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Se um novo arquivo de imagem foi enviado, faça o upload
            if (imageFile != null)
            {
                productDto.ImageUrl = await _fileUploadService.UploadFileAsync(imageFile, "product_images");
            }
            // Se imageFile é nulo, productDto.ImageUrl deve manter a URL existente do formulário (campo hidden)

            await _productService.UpdateProductAsync(productDto);
            return NoContent();
        }

        // DELETE: api/Products/{id} (Excluir Produto)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
    }
}