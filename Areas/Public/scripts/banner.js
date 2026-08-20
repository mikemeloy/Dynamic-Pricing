import { toCurrency } from '/Plugins/i7MEDIA.Plugin.Misc.Core/Scripts/utils.js';

let
  _banner,
  _getUrl,
  _confirmMessage,
  _notificationUris = [];

const
  init = ({ getRoute, cartPriceLock, secondsSinceLastUpdate, priceUpdateInterval, confirmMessage, notificationUris }) => {
    _getUrl = getRoute;
    _confirmMessage = confirmMessage;
    _banner = document.querySelector("[data-dynamic-price-banner]");
    _notificationUris = Array.isArray(notificationUris)
      ? notificationUris
      : [];

    initPriceUpdateTimer({ secondsSinceLastUpdate, priceUpdateInterval });
    initCartLockTimer({ cartPriceLock });
  },
  initCartLockTimer = ({ cartPriceLock }) => {
    if (cartPriceLock <= 0) {
      console.info('no price lock');
      return;
    }

    toggleTimer();

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

        if (cartPriceLock-- > -1) {
          return;
        }

        try {
          toggleTimer();
          clearInterval(interval);
          const
            reloadPage = reloadPageOnTimerExpiry();

          if (reloadPage) {
            location.reload();
          }

        } catch (error) {
          clearInterval(interval);
          _banner.remove();
          console.error(error);
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

    console.table(metalTypes);

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
  },
  toggleTimer = () => {
    const timerEl = _banner.querySelector("[data-dynamic-price-timer]");

    timerEl.hidden = !timerEl.hidden;
  },
  reloadPageOnTimerExpiry = () => {
    const
      isCheckoutPage = _notificationUris.includes(location.pathname);

    if (!isCheckoutPage) {
      return false;
    }

    return confirm(_confirmMessage);
  };


export { init }