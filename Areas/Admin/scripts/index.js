let
    _productId,
    _saveUrl;

const
    init = ({ productId, saveRoute }) => {
        _saveUrl = saveRoute;
        _productId = productId
        setPageEvent();
    },
    setPageEvent = () => {
        const
            parent = document.querySelector('#dynamic-price-cards'),
            save = parent.querySelector('footer button');

        save.addEventListener('click', event.onSave_Click);
    }

const
    event = {
        onSave_Click: async ({ currentTarget }) => {
            currentTarget.disabled = true;

            try {
                const
                    formData = new FormData(),
                    parent = document.querySelector('#dynamic-price-cards'),
                    getFormValue = (selector) => {
                        const el = parent.querySelector(selector);

                        return el ? el.value : '';
                    },
                    metalType = getFormValue("#SelectedMetalType"),
                    weight = getFormValue("#Weight"),
                    basePrice = getFormValue("#BasePrice"),
                    modifierType = getFormValue('#PriceModifierType'),
                    modifierValue = getFormValue('#PriceModifier');

                formData.append('ProductId', _productId);
                formData.append('MetalType', metalType);
                formData.append('BasePrice', basePrice);
                formData.append('Weight', weight);
                formData.append('PriceModifierType', modifierType);
                formData.append('PriceModifier', modifierValue)

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