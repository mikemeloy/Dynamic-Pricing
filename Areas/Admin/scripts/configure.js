let
    _saveUrl,
    _exportUrl,
    _importUrl,
    _errorMessage = "An error has occurred, check system logs for more details";

const
    init = ({ saveUrl, exportUrl, importUrl }) => {
        _saveUrl = saveUrl;
        _exportUrl = exportUrl;
        _importUrl = importUrl;

        setPageEvents();
    },
    setPageEvents = () => {
        const
            container = document.querySelector('[data-container-configure]'),
            save = container.querySelector('[data-save]'),
            uploadProxy = container.querySelector('[data-dynamic-price-upload]'),
            upload = uploadProxy.querySelector('input[type="file"]'),
            downLoad = container.querySelector('[data-dynamic-price-download]');

        save.addEventListener('click', events.save);
        upload.addEventListener('change', (e) => setLoading(() => events.fileUploaded(e)));
        uploadProxy.addEventListener('click', () => upload.click());
        downLoad.addEventListener('click', () => setLoading(events.fileDownload));
    },
    setLoading = async (func) => {
        const el = document.querySelector('[data-dynamic-price-file]');

        el.toggleAttribute('data-test');
        await func();
        el.toggleAttribute('data-test');
    }

const events = {
    save: async () => {
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
            displayBarNotification("Settings saved", 0, 3000);
        } catch (error) {
            console.error(error);
            displayBarNotification(_errorMessage, 1, 3000);
        }
    },
    fileUploaded: async ({ currentTarget }) => {
        try {
            const
                { files } = currentTarget;

            for (const file of files) {
                await fileHelper.uploadFile(file);
            }
            displayBarNotification("Item update complete", 0, 3000);
        } catch (error) {
            console.error(error);
            displayBarNotification(_errorMessage, 1, 3000);
        }
    },
    fileDownload: async () => {
        try {
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
        } catch (error) {
            console.error(error);
            displayBarNotification(_errorMessage, 1, 3000);
        }
    }
}

const fileHelper = {
    downloadFile: async () => {
        return await fetch(_exportUrl, { method: 'GET' })
    },
    uploadFile: async (file) => {
        const
            formData = new FormData(),
            notFileType = !(file instanceof File);

        if (notFileType) {
            return {
                success: false,
                error: 'Not a File'
            };
        }

        formData.append('qqfile', file);

        try {
            await fetch(_importUrl, { method: "POST", body: formData });
            return { success: true, error: undefined };
        } catch (error) {
            return { success: false, error };
        }
    }
}


export { init }