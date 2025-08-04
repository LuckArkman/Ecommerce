$(document).ready(function() {
    const registerModalElement = document.getElementById('registerModal');
    const registerModal = new bootstrap.Modal(registerModalElement); // Crie a instância do modal Bootstrap
    const registerForm = $('#registerForm');
    const registerMessage = $('#registerMessage');
    const redirectToLoginBtn = $('#redirectToLoginBtn');

    // 1. Lógica para envio do formulário de registro via AJAX
    registerForm.on('submit', async function(e) {
        e.preventDefault(); // Impede o envio padrão do formulário

        registerMessage.hide().removeClass('alert-success alert-danger').text(''); // Limpa mensagens anteriores
        
        // Desabilitar o botão para evitar múltiplos cliques
        const submitButton = registerForm.find('button[type="submit"]');
        submitButton.prop('disabled', true).text('Registrando...');

        // Coleta dos valores dos campos
        const email = $('#registerEmail').val();
        const password = $('#registerPassword').val();
        const confirmPassword = $('#registerConfirmPassword').val();

        // Validação básica do cliente (já presente, apenas confirmando)
        let isValid = true;
        
        // Remove classes de validação anteriores
        $('#registerEmail').removeClass('is-invalid');
        $('#registerPassword').removeClass('is-invalid');
        $('#registerConfirmPassword').removeClass('is-invalid');

        if (!email || !email.includes('@')) {
            $('#registerEmail').addClass('is-invalid');
            isValid = false;
        }
        if (!password || password.length < 6) {
            $('#registerPassword').addClass('is-invalid');
            isValid = false;
        }
        if (password !== confirmPassword) {
            $('#registerConfirmPassword').addClass('is-invalid');
            isValid = false;
        }

        if (!isValid) {
            registerMessage.text('Por favor, corrija os erros no formulário.').addClass('alert-danger').show();
            submitButton.prop('disabled', false).text('Registrar'); // Habilita o botão
            return;
        }

        const formData = {
            email: email,
            password: password,
            confirmPassword: confirmPassword
        };

        // Obter o Anti-Forgery Token
        const antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();
        if (!antiForgeryToken) {
            registerMessage.text('Erro de segurança: Anti-Forgery Token ausente.').addClass('alert-danger').show();
            submitButton.prop('disabled', false).text('Registrar');
            return;
        }

        try {
            // Chamada AJAX para o AccountController MVC (não diretamente para a API de backend)
            const response = await fetch('/Account/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': antiForgeryToken // Envie o Anti-Forgery Token
                },
                body: JSON.stringify(formData)
            });

            if (response.ok) {
                registerMessage.text('Conta criada com sucesso! Você já pode fazer login.').addClass('alert-success').show();
                registerForm[0].reset(); // Limpa o formulário
                // Opcional: Fechar o modal após um pequeno atraso
                setTimeout(() => {
                    registerModal.hide();
                    window.location.href = '/Identity/Account/Login';
                }, 2000);
            } else {
                let errorMessageText = 'Falha no registro. Por favor, tente novamente.';
                try {
                    const errorData = await response.json(); // Tenta ler a resposta JSON
                    if (errorData.message) {
                        errorMessageText = `Falha no registro: ${errorData.message}`;
                    } else if (errorData.errors) {
                        // Se a API retornar erros de ModelState (ValidationProblemDetails)
                        let apiErrors = Object.values(errorData.errors).flat().join('\n');
                        errorMessageText = `Falha no registro:\n${apiErrors}`;
                    } else {
                        errorMessageText = `Falha no registro: ${response.statusText}`;
                    }
                } catch (jsonError) {
                    // Não foi possível ler JSON, usar o statusText
                    errorMessageText = `Falha no registro: ${response.statusText}.`;
                }
                registerMessage.text(errorMessageText).addClass('alert-danger').show();
            }
        } catch (error) {
            console.error('Erro de rede durante o registro:', error);
            registerMessage.text('Erro de conexão. Por favor, tente novamente mais tarde.').addClass('alert-danger').show();
        } finally {
            submitButton.prop('disabled', false).text('Registrar'); // Habilita o botão
        }
    });

    // 2. Lógica para redirecionar para a página de login
    redirectToLoginBtn.on('click', function() {
        registerModal.hide(); // Esconde o modal de registro
        window.location.href = '/Identity/Account/Login'; // Redireciona para a página de login
    });
});