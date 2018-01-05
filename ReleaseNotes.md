## New in 0.3.0 (Unreleased)

* Added support for running package actions
  * These may fail, if the action depends on `HttpContext` or another dependency that we don't pre-configure
* Fixed bug with when downloading/unpacking package that has previously been done

## New in 0.2.0 (2018/01/02)

* Fixing missing NuGet dependencies
* Adding a way to install starter kits without user interaction
* Adding help documentation

## New in 0.1.2 (2018/01/02)

* Rewrote the build script to use the proper Fake 5 syntax

## New in 0.1.1 (2018/01/02)

* Cleaning up the NuGet package
* Showing build/NuGet status on readme

## New in 0.1.0 (2018/01/02)

Everything is new! This is the first proper build of the plugin containing the basic functionality to find, download, unpack and prepare to install a package from the Umbraco package feed.