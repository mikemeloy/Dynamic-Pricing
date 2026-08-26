let _saveUrl;
const
    init = ({ saveRoute }) => {
        _saveUrl = saveRoute;
        setPageEvents();
    },
    setPageEvents = () => {
        const
            container = document.querySelector('[data-container-configure]'),
            save = container.querySelector('[data-save]'),
            upload = container.querySelector('[data-dynamic-price-file] input'),
            downLoad = container.querySelector('[data-dynamic-price-export]');

        save.addEventListener('click', events.save);
        upload.addEventListener('change', events.fileUploaded)
        downLoad.addEventListener('click', events.fileExport);
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
    },
    fileUploaded: ({ currentTarget }) => {
        const { files } = currentTarget;

        for (const file of files) {
            fileHelper.uploadFile(file);
        }
    },
    fileExport: async () => {
       var resp = await fileHelper.downloadFile();
        const data = await resp.blob();

        const
            a = document.createElement('a'),
            url = window.URL.createObjectURL(data);

        a.href = url;
        a.download = "products";
        document.body.append(a);
        a.click();
        a.remove();
        window.URL.revokeObjectURL(url);
    }
}

const fileHelper = {
    downloadFile: async () => {
        const
            url = `export`;

        return await fetch(url, { method: 'GET' })
    },
    uploadFile: async (file) => {
        const
            formData = new FormData(),
            url = `import`,
            notFileType = !(file instanceof File);


        if (notFileType) {
            return {
                success: false,
                error: 'Not a File'
            };
        }

        formData.append('qqfile', file);

        try {
            await fetch(url, { method: "POST", body: formData });
            return { success: true, error: undefined };
        } catch (error) {
            return { success: false, error };
        }
    },
}


export { init }