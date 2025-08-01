using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ECommerce.Client.Auth;
using ECommerce.Client.Components; // Importa o namespace onde está CustomAuthenticationStateProvider
using Microsoft.AspNetCore.Components.Authorization; // Importa o namespace para AuthenticationStateProvider

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Adiciona o componente raiz da aplicação Blazor ao elemento #app no index.html
builder.RootComponents.Add<App>("#app");
// Adiciona o HeadOutlet para gerenciar o conteúdo da tag <head>
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configura o HttpClient para fazer requisições HTTP a partir do cliente.
// O BaseAddress é definido para a base da URL da aplicação (onde o servidor está hospedado).
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// --- SEÇÃO DE AUTENTICAÇÃO E AUTORIZAÇÃO ---
// Registra os serviços de autorização core do Blazor.
// Se você estiver usando CascadingAuthenticationState em seus componentes, pode ser necessário AddCascadingAuthenticationState()
builder.Services.AddAuthorizationCore();

// Registra seu CustomAuthenticationStateProvider como a implementação de AuthenticationStateProvider.
// Isso permite que o Blazor injete seu provedor de estado de autenticação nos componentes.
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

// Opcional: Se você usa o CascadingAuthenticationState, descomente a linha abaixo e comente a AddAuthorizationCore() acima.
// builder.Services.AddCascadingAuthenticationState();
// --- FIM DA SEÇÃO DE AUTENTICAÇÃO E AUTORIZAÇÃO ---

await builder.Build().RunAsync();