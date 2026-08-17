let
  _saveUrl;

const
  init = ({ saveRoute }) => {
    _saveUrl = saveRoute;
    setPageEvent();
  },
  setPageEvent = () => {
    const
      save = document.querySelector('[data-dynamic-price-save]');

    save.addEventListener('click', event.onSave_Click);
  },
  getTableSelections = () => {
    if (Array.isArray(window.selectedIds)) {
      return window.selectedIds;
    }

    return [];
  }

const
  event = {
    onSave_Click: async ({ currentTarget }) => {
      currentTarget.disabled = true;

      try {
        const
          formData = new FormData(),
          patternIds = getTableSelections();

        formData.append('PatternIds', patternIds);

        const
          response = await fetch(_saveUrl, {
            method: "POST",
            body: formData
          });

        if (response.status !== 200) {
          throw new Error();
        }

        displayBarNotification?.("Success! All products associated with the selected pattern(s) have been set to dynamic pricing", 0, 1000 * 5);

      } catch (error) {
        console.error(error);
        displayBarNotification?.("An error has occurred, please check logs for more information", 2, 1000 * 5);
      }

      currentTarget.disabled = false;
    }
  }

export { init }