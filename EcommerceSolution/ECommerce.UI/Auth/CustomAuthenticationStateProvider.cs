// ECommerce.Client/Auth/CustomAuthenticationStateProvider.cs
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce.Client.Auth
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        // Este é um exemplo simplificado. Em um projeto real, você
        // armazenaria o token JWT em localStorage/sessionStorage
        // e o validaria/decodificaria aqui.
        // Para simplificar, simularemos login/logout.
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        public void MarkUserAsAuthenticated(string email)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email),
                // new Claim(ClaimTypes.Role, "Admin") // Exemplo de role
            }, "CustomAuth"));
            _currentUser = authenticatedUser;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }

        public void MarkUserAsLoggedOut()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }

        // Em um cenário real, você teria métodos para obter o token JWT
        // e criar ClaimsPrincipal a partir dele.
        // public async Task<bool> Login(string email, string password) { ... }
        // public async Task Logout() { ... }
    }
}