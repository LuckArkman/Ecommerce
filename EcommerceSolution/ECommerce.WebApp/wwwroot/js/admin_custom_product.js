// admin_custom_product.js (ou dentro da View)

$(document).ready(function () {
    const isFreeShippingCheckbox = $('#isFreeShipping');
    const freeShippingRegionsGroup = $('#freeShippingRegionsGroup');

    // Função para alternar a visibilidade
    function toggleFreeShippingRegions() {
        if (isFreeShippingCheckbox.is(':checked')) {
            freeShippingRegionsGroup.show();
        } else {
            freeShippingRegionsGroup.hide();
        }
    }

    // Chama a função ao carregar a página
    toggleFreeShippingRegions();

    // Chama a função ao mudar o estado do checkbox
    isFreeShippingCheckbox.on('change', toggleFreeShippingRegions);
});