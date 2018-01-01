# Chauffeur External Package plugin

This is an extension for [Chauffeur](https://github.com/aaronpowell/chauffeur) that allows you to install Umbraco packages from the Umbraco package feed without logging into the back office.

The intent is to allow you to programmatically download Umbraco packages so you don't have to check their binaries into source control, and so you can add their DocTypes/etc. using Chauffeur easily.

## How it works

Once installed into your Umbraco instance (with Chauffeur obviously) you will have a new deliverable available, `external-package`. This then has a number of sub-commands that can you used to perform operations against external packages.

### `search <pattern>`

```sh
umbraco> external-package search
Here are the results
1) Duplicate content
2) File Picker Data Type
3) Archetype Mapper
4) Bulk User Admin
5) CustomFontIconPicker
6) List Picker
7) Umbraco Properties Generator
8) FABRIC8 - Rackspace Cloud Files With Umbraco
9) Disable Delete 7.4
10) "Omit Segments" Url Provider
11) Umbraco Jet
12) Cachebuster for Angular Views
13) QR Code Property Editor
14) uCondition: Member Predicates
15) Relations Picker
16) Umbraco Restrictions
17) View in Browser
18) Tagliatelle Tag Editor
19) Page count document tree
20) Ubolt Content Picker
21) Gist Shortcode Embed
22) Documento
23) Our.Umbraco.OpeningHours
q) Cancel
Select a package to download>
```

This will search the Umbraco package feed and return you the names of the packages that match the pattern you've provided (if you didn't provide a pattern it'll just give you them in the order they come from the feed).

From here you can select a package to download.

### `download <package id>`

```sh
umbraco> external-package download c1b3515b-99f5-4aac-a381-02f4c15341c9
Downloading the package from the Umbraco package repo
Package saved to the Chauffeur folder and ready for unpacking. Run the following command
external-package unpack c1b3515b-99f5-4aac-a381-02f4c15341c9
```

This will download a package from the package feed and stores it in the Chauffeur folder.

### `unpack <package id>`

```sh
umbraco> external-package unpack c1b3515b-99f5-4aac-a381-02f4c15341c9
Package has been expanded, next you need to run the 'package' deliverable to install it:
pkg package -f:C:\_Projects\github\Chauffeur.ExternalPackages\Chauffeur.ExternalPackages.Demo\App_Data\Chauffeur\c1b3515b-99f5-4aac-a381-02f4c15341c9-unpack
```

This will expand the downloaded package on disk. It'll create a folder within the Chauffeur directory that your package will be unpacked into. You would then use the `package` deliverable that ships with Chauffeur to actually install it into your Umbraco instance, providing the directory that the package lives in (the `-f:<Path>` parameter). This means that the out of the box features of Chauffeur are used where possible.

### `categories`

```sh
umbraco> external-package categories
Here are the results
1) Collaboration
2) Backoffice extensions
3) Developer tools
4) Starter kits
5) Umbraco Pro
6) Website utilities
Select a category to search>
```

This will list all the categories of Umbraco packages, which you can select from and then navigate the available packages.

### `starter-kit`

```sh
umbraco> external-package starter-kit
Here are the results
1) Fanoe
2) Txt
3) Overflow
q) Cancel
Select a kit to install>
```

This will list the available starter kits for your Umbraco version, different versions have a different number of starter kits available (the above is an example of Umbraco 7.5.9). After selecting it'll run like a standard download.