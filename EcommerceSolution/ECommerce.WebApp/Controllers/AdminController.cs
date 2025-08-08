using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Models.DTOs.Order;
using ECommerce.Models.DTOs.Product;
using ECommerce.Models.DTOs.User;
using ECommerce.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
#if !DEBUG // <-- Se NÃO estiver em modo DEBUG, aplique Authorize
    [Authorize(Roles = "Admin")] // Apenas usuários com o papel "Admin" podem acessar
    #endif
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminController> _logger; // <-- ADICIONE ESTA LINHA

        public AdminController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<AdminController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger; // <-- ADICIONE ESTA LINHA
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var viewModel = new DashboardViewModel();
            
            // ***** GARANTA QUE O DASHBOARDSUMMARY É SEMPRE INICIALIZADO AQUI *****
            // Isso previne NullReferenceException na View
            viewModel.DashboardSummary = new DashboardSummaryDto(); 

            try
            {
                var apiResponse = await client.GetAsync("api/Dashboard/summary");
                apiResponse.EnsureSuccessStatusCode(); 

                var jsonContent = await apiResponse.Content.ReadAsStringAsync(); // Leia o conteúdo como string
                _logger.LogInformation($"API Dashboard Summary JSON: {jsonContent}"); // Log o JSON recebido

                // Tente deserializar. Se a string JSON for vazia ou "null", DeserializeObject retornará null.
                var dashboardSummary = JsonConvert.DeserializeObject<DashboardSummaryDto>(jsonContent);
                
                // Se a deserialização resultar em null (API retornou JSON vazio ou "null"), use um novo DTO vazio
                viewModel.DashboardSummary = dashboardSummary ?? new DashboardSummaryDto(); 
                _logger.LogInformation($"DashboardSummary preenchido. Total de Pedidos: {viewModel.DashboardSummary.Sales.TotalOrders}");

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro HTTP ao carregar dashboard (GET). Status: {StatusCode}", ex.StatusCode);
                ViewBag.ErrorMessage = $"Erro ao carregar dashboard: {ex.Message}";
                if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("Admin Dashboard: API retornou 401/403. Redirecionando para login.");
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (JsonSerializationException ex) // Catch específico para erros de JSON
            {
                _logger.LogError(ex, "Erro de JSON ao deserializar dashboard. Conteúdo inválido.");
                ViewBag.ErrorMessage = $"Erro ao processar dados do dashboard: Formato inválido.";
                // viewModel.DashboardSummary já é new DashboardSummaryDto()
            }
            catch (Exception ex) // Outras exceções inesperadas
            {
                _logger.LogError(ex, "Erro inesperado ao carregar dashboard.");
                ViewBag.ErrorMessage = $"Erro inesperado: {ex.Message}";
            }

            return View(viewModel);
        }
        
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportPdf()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            try
            {
                var apiResponse = await client.GetAsync("api/Dashboard/export/pdf");
                apiResponse.EnsureSuccessStatusCode(); // Isso vai lançar erro se a API retornar 401/403/500

                var pdfBytes = await apiResponse.Content.ReadAsByteArrayAsync();
                return File(pdfBytes, "application/pdf", "RelatorioVendas.pdf");
            }
            catch (HttpRequestException ex)
            {
                // Lidar com erro da API (ex: 401 Unauthorized da API)
                ViewBag.ErrorMessage = $"Erro ao gerar PDF: {ex.Message}";
                return View("Dashboard"); // Retorna para o dashboard com erro
            }
        }
        
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportExcel()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            try
            {
                var apiResponse = await client.GetAsync("api/Dashboard/export/excel");
                apiResponse.EnsureSuccessStatusCode();

                var excelBytes = await apiResponse.Content.ReadAsByteArrayAsync();
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RelatorioVendas.xlsx");
            }
            catch (HttpRequestException ex)
            {
                ViewBag.ErrorMessage = $"Erro ao gerar Excel: {ex.Message}";
                return View("Dashboard"); // Retorna para o dashboard com erro
            }
        }

        // Action para exibir o formulário de adição de produto
        [HttpGet]
        public async Task<IActionResult> ProductAdd()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var viewModel = new ProductAddViewModel();
            // return View(viewModel); // <--- REMOVA ESTA LINHA!
            try
            {
                var categoriesResponse = await client.GetAsync("api/products/categories");
                categoriesResponse.EnsureSuccessStatusCode();
                viewModel.Categories = JsonConvert.DeserializeObject<List<CategoryDto>>(await categoriesResponse.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException ex) { ViewBag.ErrorMessage = $"Erro ao carregar categorias: {ex.Message}"; }
            return View(viewModel);
        }

        // Action para processar a adição de produto
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Manter esta proteção para ações de escrita
        public async Task<IActionResult> ProductAdd(ProductDto productDto)
        {
            var viewModel = new ProductAddViewModel();

            if (!ModelState.IsValid)
            {
                var _client = _httpClientFactory.CreateClient("ECommerceApi");
                // viewModel = new ProductAddViewModel(); // Nao recrie o ViewModel aqui, ele já está inicializado
                viewModel.Product = productDto; // Mantenha os dados do formulário
                try
                {
                    var categoriesResponse = await _client.GetAsync("api/products/categories");
                    categoriesResponse.EnsureSuccessStatusCode();
                    viewModel.Categories = JsonConvert.DeserializeObject<List<CategoryDto>>(await categoriesResponse.Content.ReadAsStringAsync());
                }
                catch (HttpRequestException ex) { ViewBag.ErrorMessage = $"Erro ao carregar categorias: {ex.Message}"; }
                return View(viewModel); // Retorna a View com erros de validação
            }

            var client = _httpClientFactory.CreateClient("ECommerceApi");
            try
            {
                var apiResponse = await client.PostAsJsonAsync("api/products", productDto);
                apiResponse.EnsureSuccessStatusCode();
                TempData["SuccessMessage"] = "Produto adicionado com sucesso!";
                return RedirectToAction("ProductManage");
            }
            catch (HttpRequestException ex)
            {
                ViewBag.ErrorMessage = $"Erro ao adicionar produto: {ex.Message}";
                // Recarregue as categorias para a View antes de retornar
                var _client = _httpClientFactory.CreateClient("ECommerceApi");
                try
                {
                    var categoriesResponse = await _client.GetAsync("api/products/categories");
                    categoriesResponse.EnsureSuccessStatusCode();
                    viewModel.Categories = JsonConvert.DeserializeObject<List<CategoryDto>>(await categoriesResponse.Content.ReadAsStringAsync());
                }
                catch (HttpRequestException loadEx) { ViewBag.ErrorMessage += $" (Erro ao recarregar categorias: {loadEx.Message})"; }
                return View(viewModel); // Retorna para o formulário com erro
            }
        }

        // Action para gerenciar (listar e editar) produtos
        [HttpGet]
        public async Task<IActionResult> ProductManage()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var viewModel = new ProductManageViewModel();
            // return View(viewModel); // <--- REMOVA ESTA LINHA!
            try
            {
                var productsResponse = await client.GetAsync("api/products");
                productsResponse.EnsureSuccessStatusCode();
                viewModel.Products = JsonConvert.DeserializeObject<List<ProductDto>>(await productsResponse.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException ex) { ViewBag.ErrorMessage = $"Erro ao carregar produtos: {ex.Message}"; }
            return View(viewModel);
        }

        // Action para exibir o formulário de edição de produto
        [HttpGet]
        public async Task<IActionResult> ProductEdit(int id)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var viewModel = new ProductEditViewModel();
            try
            {
                var productResponse = await client.GetAsync($"api/products/{id}");
                productResponse.EnsureSuccessStatusCode();
                viewModel.Product = JsonConvert.DeserializeObject<ProductDto>(await productResponse.Content.ReadAsStringAsync());

                var categoriesResponse = await client.GetAsync("api/products/categories");
                categoriesResponse.EnsureSuccessStatusCode();
                viewModel.Categories = JsonConvert.DeserializeObject<List<CategoryDto>>(await categoriesResponse.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException ex) { ViewBag.ErrorMessage = $"Erro ao carregar produto para edição: {ex.Message}"; }
            return View(viewModel);
        }

        // Action para processar a edição de produto
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Manter esta proteção
        public async Task<IActionResult> ProductEdit(ProductDto productDto)
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var viewModel = new ProductEditViewModel();
            // return View(viewModel); // <--- REMOVA ESTA LINHA!

            if (!ModelState.IsValid)
            {
                viewModel.Product = productDto;
                try
                {
                    var categoriesResponse = await client.GetAsync("api/products/categories");
                    categoriesResponse.EnsureSuccessStatusCode();
                    viewModel.Categories = JsonConvert.DeserializeObject<List<CategoryDto>>(await categoriesResponse.Content.ReadAsStringAsync());
                }
                catch (HttpRequestException ex) { ViewBag.ErrorMessage = $"Erro ao carregar categorias: {ex.Message}"; }
                return View(viewModel);
            }
            
            try
            {
                var apiResponse = await client.PutAsJsonAsync($"api/products/{productDto.Id}", productDto);
                apiResponse.EnsureSuccessStatusCode();
                TempData["SuccessMessage"] = "Produto atualizado com sucesso!";
                return RedirectToAction("ProductManage");
            }
            catch (HttpRequestException ex)
            {
                ViewBag.ErrorMessage = $"Erro ao atualizar produto: {ex.Message}";
                try
                {
                    var categoriesResponse = await client.GetAsync("api/products/categories");
                    categoriesResponse.EnsureSuccessStatusCode();
                    viewModel.Categories = JsonConvert.DeserializeObject<List<CategoryDto>>(await categoriesResponse.Content.ReadAsStringAsync());
                }
                catch (HttpRequestException loadEx) { ViewBag.ErrorMessage += $" (Erro ao recarregar categorias: {loadEx.Message})"; }
                return View(viewModel);
            }
        }

        // Action para listar todas as ordens de serviço
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var viewModel = new OrdersViewModel();
            try
            {
                var apiResponse = await client.GetAsync("api/orders/all");
                apiResponse.EnsureSuccessStatusCode();
                viewModel.Orders = JsonConvert.DeserializeObject<List<OrderDto>>(await apiResponse.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException ex) { ViewBag.ErrorMessage = $"Erro ao carregar ordens de serviço: {ex.Message}"; }
            return View(viewModel);
        }

        // Action para listar todos os usuários
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var viewModel = new UsersViewModel();
            try
            {
                // Este endpoint deve ser criado na API: /api/users/all (ou via IUserService)
                var apiResponse = await client.GetAsync("api/users/all");
                apiResponse.EnsureSuccessStatusCode();
                viewModel.Users = JsonConvert.DeserializeObject<List<UserProfileDto>>(await apiResponse.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException ex) { ViewBag.ErrorMessage = $"Erro ao carregar usuários: {ex.Message}"; }
            return View(viewModel);
        }

        // Action para listar compras com pagamento pendente
        [HttpGet]
        public async Task<IActionResult> PendingOrders()
        {
            var client = _httpClientFactory.CreateClient("ECommerceApi");
            var viewModel = new PendingOrdersViewModel();
            // return View(viewModel); // <--- REMOVA ESTA LINHA!
            try
            {
                var apiResponse = await client.GetAsync("api/orders?status=Pending");
                apiResponse.EnsureSuccessStatusCode();
                viewModel.Orders = JsonConvert.DeserializeObject<List<OrderDto>>(await apiResponse.Content.ReadAsStringAsync());
            }
            catch (HttpRequestException ex) { ViewBag.ErrorMessage = $"Erro ao carregar pedidos pendentes: {ex.Message}"; }
            return View(viewModel);
        }
    }