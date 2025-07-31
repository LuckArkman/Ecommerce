using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Models.DTOs.Cart;
using ECommerce.Models.DTOs.Product;

namespace ECommerce.Client.Services
{
    public class CartService
    {
        private readonly CartApiClient _cartApiClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private List<CartItemDto> _cartItems = new List<CartItemDto>();
        private string _currentUserId; // Para armazenar o ID do usuário logado

        public event Action OnChange;

        public CartService(CartApiClient cartApiClient, AuthenticationStateProvider authenticationStateProvider)
        {
            _cartApiClient = cartApiClient;
            _authenticationStateProvider = authenticationStateProvider;
            _authenticationStateProvider.AuthenticationStateChanged += AuthenticationStateChangedHandler;
        }

        private async void AuthenticationStateChangedHandler(Task<AuthenticationState> state)
        {
            var authState = await state;
            var user = authState.User;
            _currentUserId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (user.Identity.IsAuthenticated)
            {
                await LoadCart(); // Carrega o carrinho do usuário logado
            }
            else
            {
                _cartItems.Clear(); // Limpa o carrinho se o usuário deslogar
                OnChange?.Invoke();
            }
        }

        public async Task LoadCart()
        {
            if (string.IsNullOrEmpty(_currentUserId))
            {
                _cartItems.Clear(); // Nao ha usuario logado, carrinho vazio
            }
            else
            {
                try
                {
                    _cartItems = (await _cartApiClient.GetCart()).ToList();
                }
                catch (HttpRequestException ex)
                {
                    // Lidar com erros de API, por exemplo, token inválido
                    Console.WriteLine($"Erro ao carregar carrinho: {ex.Message}");
                    _cartItems.Clear();
                }
            }
            OnChange?.Invoke();
        }

        public IEnumerable<CartItemDto> GetCartItems() => _cartItems;
        public decimal GetCartTotal() => _cartItems.Sum(item => item.Subtotal);
        public int GetCartItemCount() => _cartItems.Sum(item => item.Quantity); // Novo método para contagem total

        public async Task AddToCart(ProductDto product, int quantity)
        {
            if (string.IsNullOrEmpty(_currentUserId))
            {
                // Lógica para carrinho de convidado (ex: usar localStorage) ou redirecionar para login
                // Por agora, vamos exigir login.
                throw new InvalidOperationException("É necessário fazer login para adicionar itens ao carrinho.");
            }

            var request = new AddToCartRequest { ProductId = product.Id, Quantity = quantity };
            try
            {
                var updatedItem = await _cartApiClient.AddOrUpdateCartItem(request);
                var existingItem = _cartItems.FirstOrDefault(ci => ci.ProductId == updatedItem.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity = updatedItem.Quantity;
                }
                else
                {
                    _cartItems.Add(updatedItem);
                }
                OnChange?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao adicionar/atualizar item no carrinho: {ex.Message}");
                throw; // Re-lança para o componente lidar
            }
        }

        public async Task UpdateQuantity(int productId, int newQuantity)
        {
            if (string.IsNullOrEmpty(_currentUserId)) return;

            var itemToUpdate = _cartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (itemToUpdate != null)
            {
                if (newQuantity <= 0)
                {
                    await RemoveFromCart(productId);
                }
                else
                {
                    var request = new AddToCartRequest { ProductId = productId, Quantity = newQuantity - itemToUpdate.Quantity }; // Calcula a diferença
                    try
                    {
                        var updatedItem = await _cartApiClient.AddOrUpdateCartItem(request);
                        itemToUpdate.Quantity = updatedItem.Quantity;
                        OnChange?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao atualizar quantidade no carrinho: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        public async Task RemoveFromCart(int productId)
        {
            if (string.IsNullOrEmpty(_currentUserId)) return;
            try
            {
                await _cartApiClient.RemoveCartItem(productId);
                _cartItems.RemoveAll(ci => ci.ProductId == productId);
                OnChange?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao remover item do carrinho: {ex.Message}");
                throw;
            }
        }

        public async Task ClearCart()
        {
            if (string.IsNullOrEmpty(_currentUserId)) return;
            try
            {
                await _cartApiClient.ClearCart();
                _cartItems.Clear();
                OnChange?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao limpar carrinho: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _authenticationStateProvider.AuthenticationStateChanged -= AuthenticationStateChangedHandler;
        }
    }
}