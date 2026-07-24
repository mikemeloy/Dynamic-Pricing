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
                    metalType = parent.querySelector("[data-select2-id=SelectedMetalType]"),
                    weight = parent.querySelector("#Weight"),
                    basePrice = parent.querySelector("#BasePrice");

                formData.append('ProductId', _productId);
                formData.append('MetalType', metalType.value);
                formData.append('BasePrice', basePrice.value);
                formData.append('Weight', weight.value);


                const response = await fetch(_saveUrl, {
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