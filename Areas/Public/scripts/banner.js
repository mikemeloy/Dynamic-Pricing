let
  _duration,
  _getUrl;

const
  init = ({ getUrl, duration }) => {
    _getUrl = getUrl;
    _duration = duration;
    initTimer(_duration);
  },
  initTimer = (duration) => {
    const
      banner = document.querySelector("[data-dynamic-price-banner]"),
      el = banner.querySelector('[data-timer]');

    let
      timer = duration,
      minutes,
      seconds;

    setInterval(async function () {
      minutes = parseInt(timer / 60, 10);
      seconds = parseInt(timer % 60, 10);

      seconds = seconds < 10 ? "0" + seconds : seconds;

      el.textContent = `${minutes}:${seconds}`;

      if (--timer < 0) {
        timer = duration;
        await getNewMetalPrices();
      }
    }, 1000);
  },
  getNewMetalPrices = async () => {
    const response = await fetch(_getUrl);
  }

export { init }