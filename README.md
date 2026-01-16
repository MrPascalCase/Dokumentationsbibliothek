## About

A Blazor WebAssembly application for browsing and searching images from the Dokumentationsbibliothek St. Moritz.

A live development version can be found at
https://mrpascalcase.github.io/Dokumentationsbibliothek

The application might eventually be published at
https://www.biblio-stmoritz.ch/dokumentationsbibliothek (→ Bilder suchen)

It is being developed as a replacement for the existing frontend currently available at
https://app.dasch.swiss/project/ZjLtCB-rRpG-NglRpxy1dg/search


## Todo

- ~~routing does not work initially on github pages:
  https://mrpascalcase.github.io/Dokumentationsbibliothek/search
  -> 404~~
- ~~resize does not work initially~~
- ~~execute search immediately with 'enter'~~
- ~~show image preview as overlay~~
- ~~fix the placeholder in the image preview~~
- search by ~~author, decade~~, year, subject
- ~~fix season/jahreszeit~~
- image details should have links to search by ~~author,~~ year, ~~decade~~, subject
- ~~API support for multiple search terms~~
- ~~UI support for multiple search terms~~
- layout optimization for mobile
- focus-issue with the input
- layout: force same width with scaling of rows?
- ~~explain search:~~
    + ~~better support for "." (st. moritz) (also ca.)~~
    + ~~better support for multiple words in close proximity~~
    + ~~add more tests for justification logic.~~
- add a magnifying glass icon to the search bar
- ~~use a single strategy for javascript loading~~
- ~~check and resolve warnings that images are loaded twice~~
- click outside image-preview should close the overlay
- ~~reaching /search?query=post does not show the number of images found statistics~~
- ~~fix "Erfassungsdatum"~~
- improve detaillayout
- ~~bild id in the detail view is wrong~~
- ~~fix search autor:\"Steiner, Hans\" (mutiple people of the same name)~~