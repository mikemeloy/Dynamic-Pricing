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
  }

const
  event = {
    onSave_Click: async ({ currentTarget }) => {
      currentTarget.disabled = true;

      try {
        const
          formData = new FormData();

        formData.append('PatternIds', selectedIds);
 
        const
          response = await fetch(_saveUrl, {
            method: "POST",
            body: formData
          });

        console.table(response);

      } catch (error) {
        console.error(error);
      }

      currentTarget.disabled = false;
    }
  }

export { init }