import { toCurrency } from '/Plugins/i7MEDIA.Plugin.Misc.Core/Scripts/utils.js';

let
  _banner,
  _getUrl,
  _cartLockCreateUrl,
  _confirmMessage,
  _cartLockUris;

const
  init = async ({ getRoute, cartLockCreateRoute, secondsSinceLastUpdate, priceUpdateInterval, confirmMessage, notificationUris }) => {
    _getUrl = getRoute;
    _cartLockCreateUrl = cartLockCreateRoute;
    _confirmMessage = confirmMessage;
    _banner = document.querySelector("[data-dynamic-price-banner]");
    _cartLockUris = Array.isArray(notificationUris)
      ? notificationUris
      : [];

    initPriceUpdateTimer({ secondsSinceLastUpdate, priceUpdateInterval });
    await initCartLockTimer();
  },
  initCartLockTimer = async () => {
    let
      isCheckoutPage = _cartLockUris.includes(location.pathname),
      cartPriceLock = isCheckoutPage
        ? await createCartLock()
        : 0;

    if (cartPriceLock == 0) {
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
          } else {
            disableSubmitButton()
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
      isCheckoutPage = _cartLockUris.includes(location.pathname);

    if (!isCheckoutPage) {
      return false;
    }

    return confirm(_confirmMessage);
  },
  createCartLock = async () => {
    const result = await fetch(_cartLockCreateUrl, { method: "POST" });
    return await result.json()
  },
  disableSubmitButton = () => {
    const
      btn = document.querySelector('.confirm-order-button');

    if (!btn) {
      return;
    }

    const clone = btn.cloneNode();
    btn.replaceWith(clone);

    clone.addEventListener('click', (e) => {
      e.preventDefault();

      const
        reload = reloadPageOnTimerExpiry();

      if (reload) {
        location.reload();
      }
    });
  };


export { init }