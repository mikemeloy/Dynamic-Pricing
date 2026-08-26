let
    _productId,
    _saveUrl;

const
    init = ({ productId, saveRoute, selectedMetalType, errorMessage, isExcluded }) => {
        _saveUrl = saveRoute;
        _productId = productId
        setPageEvent();
        setPageWarning({ selectedMetalType, errorMessage, isExcluded });
        moveSectionBelowPrice();
    },
    setPageEvent = () => {
        const
            parent = document.querySelector('#dynamic-price-cards'),
            save = parent.querySelector('footer button');

        save.addEventListener('click', event.onSave_Click);
    },
    setPageWarning = ({ selectedMetalType, errorMessage, isExcluded }) => {
        var dynamicPricingIsSetup = selectedMetalType !== 0 || isExcluded === "True";

        if (dynamicPricingIsSetup) {
            return;
        }

        displayBarNotification?.(errorMessage, 2, 1000 * 30)
    },
    moveSectionBelowPrice = () => {
        const
            priceSection = document.querySelector('nop-card:has([id="product-price"])'),
            dynamicPriceSection = document.querySelector('#dynamic-price-cards');

        if (!priceSection || !dynamicPriceSection) {
            return;
        }

        priceSection.moveBefore(dynamicPriceSection, null);
    }

const
    event = {
        onSave_Click: async ({ currentTarget }) => {
            currentTarget.disabled = true;

            try {
                const
                    formData = new FormData(),
                    container = document.querySelector('#dynamic-price-cards'),
                    getFormValue = (selector, options = { bool: false }) => {
                        const input = container.querySelector(selector);
                        return options.bool ? input.checked : input.value;
                    };

                formData.append('ProductId', _productId);
                formData.append('MetalType', getFormValue("#SelectedMetalType"));
                formData.append('BasePrice', getFormValue("#BasePrice"));
                formData.append('Weight', getFormValue("#Weight"));
                formData.append('PriceModifierType', getFormValue('#PriceModifierType'));
                formData.append('PriceModifier', getFormValue('#PriceModifier'));
                formData.append('Exclude', getFormValue('#Exclude', { bool: true }));

                const
                    response = await fetch(_saveUrl, {
                        method: "POST",
                        body: formData
                    });



            } catch (error) {
                console.error(error);
            }

            currentTarget.disabled = false;
        }
    }

export { init }