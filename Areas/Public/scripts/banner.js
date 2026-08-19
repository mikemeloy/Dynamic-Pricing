import { toCurrency } from '/Plugins/i7MEDIA.Plugin.Misc.Core/Scripts/utils.js';

let
  _banner,
  _getUrl;

const
  init = ({ getRoute, cartPriceLock, secondsSinceLastUpdate, priceUpdateInterval }) => {

    _getUrl = getRoute;
    _banner = document.querySelector("[data-dynamic-price-banner]");
    initPriceUpdateTimer({ secondsSinceLastUpdate, priceUpdateInterval });
    initCartLockTimer({ cartPriceLock });
  },
  initCartLockTimer = ({ cartPriceLock }) => {
    if (!cartPriceLock) {
      console.info('no price lock');
      return;
    }

    let
      minutes,
      seconds;

    const
      el = _banner.querySelector('[data-timer]'),
      interval = setInterval(async function () {
        minutes = parseInt(cartPriceLock / 60, 10);
        seconds = parseInt(cartPriceLock % 60, 10);

        seconds = (seconds < 10)
          ? `0${seconds}`
          : seconds;

        el.textContent = `${minutes}:${seconds}`;

        if (--cartPriceLock > 0) {
          return;
        }

        try {
          location.reload();
        } catch (error) {
          console.error(error);
          clearInterval(interval);
          _banner.remove();
        }
      }, 1000);
  },
  initPriceUpdateTimer = ({ priceUpdateInterval, secondsSinceLastUpdate }) => {
    let
      timer = (priceUpdateInterval - secondsSinceLastUpdate);

    const
      interval = setInterval(async function () {

        if (--timer < 0) {
          timer = priceUpdateInterval;
          try {
            console.info("Updated metal prices");
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