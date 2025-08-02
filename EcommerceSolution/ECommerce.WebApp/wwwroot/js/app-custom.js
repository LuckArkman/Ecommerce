// wwwroot/js/app-custom.js

// Lógica para o cookie consent bar
$(document).ready(function() {
    const cookieConsentKey = 'cookieConsentAccepted';
    if (!localStorage.getItem(cookieConsentKey)) {
        $('#cookie-consent-bar').show();
    } else {
        $('#cookie-consent-bar').hide();
    }

    $('#accept-cookies-btn').on('click', function() {
        localStorage.setItem(cookieConsentKey, true);
        $('#cookie-consent-bar').fadeOut();
    });
});

// Lógica para o contador do carrinho no _Layout.cshtml (precisa de um endpoint AJAX)
// Esta função será chamada no ready do _Layout.cshtml
async function updateCartItemCount() {
    try {
        const response = await fetch('/Cart/GetCartItemCount'); // Endpoint no CartController
        if (response.ok) {
            const count = await response.json();
            $('#cart-item-count').text(count);
        }
    } catch (error) {
        console.error('Erro ao carregar contador do carrinho:', error);
    }
}
// Chame no site.js ou diretamente no _Layout.cshtml scripts section
// updateCartItemCount();