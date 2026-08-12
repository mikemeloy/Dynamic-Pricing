let _saveUrl;
const
    init = ({ saveRoute }) => {
        _saveUrl = saveRoute;
        setPageEvents();
    },
    setPageEvents = () => {
        const
            container = document.querySelector('[data-container-configure]'),
            save = container.querySelector('[data-save]');

        save.addEventListener('click', events.save);
    }

const events = {
    save: async ({ currentTarget }) => {
        currentTarget.disabled = true;
        const
            formData = new FormData(),
            container = document.querySelector('[data-container-configure]'),
            getValue = (selector) => {
                const input = container.querySelector(selector);
                return input.value;
            };

        try {
            formData.append('ApiKey', getValue("#ApiKey"));
            formData.append("ApiEndpoint", getValue("#ApiEndpoint"));
            formData.append("WeightConversion", getValue("#WeightConversion"));
            formData.append("CartPriceLock", getValue("#CartPriceLock"));

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