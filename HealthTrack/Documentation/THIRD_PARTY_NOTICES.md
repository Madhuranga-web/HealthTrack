# Bundled front-end libraries

The application bundles the following libraries locally so its UI does not depend on a CDN at runtime:

- Bootstrap CSS and JavaScript, version 5.3.8, MIT license. Source: https://getbootstrap.com/ and https://github.com/twbs/bootstrap
- Chart.js, version 4.5.1, MIT license. Source: https://www.chartjs.org/ and https://github.com/chartjs/Chart.js

NuGet dependencies and their versions are declared in the two `.csproj` files. Restore resolves those dependencies from NuGet; package binaries are not included in the source ZIP.
