# Release Notes

## Fixed

- `tone` now supports `chapters.txt` files with more than 24 hours of content (issue #24, thx @MrBrN197 for reporting this)

## Changed

- 

## Added

- `dump` command now supports `--exclude-property` to exclude specific properties from a *dump* (e.g. `tone dump --format="json" --exclude-property="embeddedPictures" --exclude-property="chapters" --query='$.meta' my-audio-book.m4b`)
  - Note: If you combine `--exclude-property` and `--include-property`, `--include-property` will be applied first, so it makes not much sense only to be able to exclude properties, that you included first

## known issues

The following issues are known, part of an external library and already reported:

- flag options (e.g. `--dry-run`) cannot be followed by arguments (e.g. `tone tag --meta-album="album" --dry-run input.mp3`) ([spectre.console 825])
  - workaround: append flag options at the end (`tone tag --meta-album="album" input.mp3 --dry-run`)
- `--meta-*` options cannot be set to empty values ([spectre.console 842])
  - workaround: use `--meta-remove-property` instead
- Value starting with `-` is mistreated as extra option (e.g. `--meta-description "-5 degrees"`)  ([spectre.console 890])
  - workaround: use `--meta-description="-5 degrees"` instead (with `=`)
- Invalid handling of parameter values starting with double quotes ("), e.g. `--meta-description'"quoted" value'` ([spectre.console 891])

[spectre.console 825]: https://github.com/spectreconsole/spectre.console/issues/825
[spectre.console 842]: https://github.com/spectreconsole/spectre.console/issues/842
[spectre.console 890]: https://github.com/spectreconsole/spectre.console/issues/890
[spectre.console 891]: https://github.com/spectreconsole/spectre.console/issues/891
[atldotnet 155]: https://github.com/Zeugma440/atldotnet/issues/155
