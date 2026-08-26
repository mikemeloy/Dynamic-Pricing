/*
* Simple script to expidite development by moving these files to the main NOP project.
*/

var fs = require('fs')
//
const
  sourceRoot = '../',
  destinationRoot = 'C:/Repos/nopCommerce_4.90.4/src/Presentation/Nop.Web/Plugins/i7MEDIA.Plugin.Misc.Dynamic.Pricing/Areas/Admin/',
  files = [
    '/Scripts/product.js',
    '/Styles/product.css',
    '/Scripts/pattern.js',
    '/Styles/pattern.css',
    '/Scripts/configure.js',
    '/Styles/configure.css',
    '/Views/_product.dynamic.price.cshtml',
    '/Views/_pattern.dynamic.price.cshtml',
    '/Views/configure.cshtml'
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