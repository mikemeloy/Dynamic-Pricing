import { toCurrency } from '/Plugins/i7MEDIA.Plugin.Misc.Core/Scripts/utils.js';

let
  _banner,
  _getUrl;

const
  init = ({ getUrl, secondsSinceLastUpdate, cartPriceLock, getRoute }) => {

    _getUrl = getRoute;
    initTimer({ cartPriceLock, secondsSinceLastUpdate });
  },
  initTimer = ({ cartPriceLock, secondsSinceLastUpdate }) => {
    _banner = document.querySelector("[data-dynamic-price-banner]");

    const
      el = _banner.querySelector('[data-timer]');

    let
      timer = secondsSinceLastUpdate,
      minutes,
      seconds;

    const
      interval = setInterval(async function () {
        minutes = parseInt(timer / 60, 10);
        seconds = parseInt(timer % 60, 10);

        seconds = (seconds < 10)
          ? `0${seconds}`
          : seconds;

        el.textContent = `${minutes}:${seconds}`;

        if (--timer < 0) {
          timer = secondsSinceLastUpdate;
          try {
            await getNewMetalPrices();
          } catch (error) {
            console.error(error);
            clearInterval(interval);
            _banner.remove();
          }
        }
      }, 1000);
  },
  getNewMetalPrices = async () => {
    const
      response = await fetch(_getUrl),
      metalTypes = await response.json();

    for (const { ApiSymbol, CurrentValue, PreviousValue } of metalTypes) {
      const
        el = _banner.querySelector(`[data-metal-symbol="${ApiSymbol}"]`);

      if (!el) {
        continue;
      }

      const
        current = el.querySelector('[data-current]'),
        indicator = el.querySelector('[data-positive]'),
        delta = el.querySelector('[data-delta]');

      indicator.dataset.positive = Math.sign(CurrentValue - PreviousValue) === -1 ? "False" : "True";

      current.innerText = toCurrency(CurrentValue);
      delta.innerText = toCurrency(CurrentValue - PreviousValue);
    }
  }

export { init }