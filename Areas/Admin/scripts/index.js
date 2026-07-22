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
        onSave_Click: async () => {
            const
                formData = new FormData(),
                parent = document.querySelector('#dynamic-price-cards'),
                metalType = parent.querySelector("[data-select2-id=SelectedMetalType]"),
                basePrice = parent.querySelector("#BasePrice");

            formData.append('ProductId', _productId);
            formData.append('MetalType', metalType.value);
            formData.append('BasePrice', basePrice.value);

            const response = await fetch(_saveUrl, {
                method: "POST",
                body: formData
            })
        }
    }

export { init }