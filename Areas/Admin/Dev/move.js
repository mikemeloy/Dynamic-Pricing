/*
* Simple script to expidite development by moving these files to the main NOP project.
*/

var fs = require('fs')
//C:/Repos/nopCommerce_4.90.0/Clean/src/Presentation/Nop.Web/Plugins/i7MEDIA.Plugin.Misc.Dynamic.Pricing/
const
  sourceRoot = '../',
  destinationRoot = 'C:/Repos/nopCommerce_4.90.0/Clean/src/Presentation/Nop.Web/Plugins/i7MEDIA.Plugin.Misc.Dynamic.Pricing/Areas/Admin',
  files = [
    '/Scripts/index.js',
    '/Styles/index.css',
    '/Views/_product.dynamic.price.cshtml'
  ]

files.forEach(file => {
  const source = `${sourceRoot}/${file}`,
    destination = `${destinationRoot}/${file}`;

  fs.copyFile(source, destination, (err) => {
    if (err) {
      console.error(`Unable to move ${file}`, err);
    } else {
      console.log(`Moved ${file}`);
    }
  });
});