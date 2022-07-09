# Release Notes

## changes
- add: `tone.json` serializer for `dump` command with optional `JSONPath` support (`tone dump my-file.m4b --format=json --query="$.meta.album"`)
- add: support new metadata format `tone.json` including `--auto-import` (`tone tag my-file.m4b --meta-tone-json-file="tone.json" --auto-import="tonejson"`)
- fix: cover tagger did not find covers under specific circumstances
- 
## known issues

The following issues are known, part of an external library and already reported:

- flag options (e.g. `--dry-run`) cannot be followed by arguments (e.g. `tone tag --meta-album="album" --dry-run input.mp3`) ([spectre.console 825])
  - workaround: append flag options at the end (`tone tag --meta-album="album" input.mp3 --dry-run`)
- `--meta-*` options cannot be set to empty values ([spectre.console 842])
  - workaround: use `--meta-remove-property` instead
- Value starting with `-` is mistreated as extra option (e.g. `--meta-description "-5 degrees"`)  ([spectre.console 890])
  - workaround: use `--meta-description="-5 degrees"` instead (with `=`)
- Invalid handling of parameter values starting with double quotes ("), e.g. `--meta-description'"quoted" value'` ([spectre.console 891])
- Invalid handling of `--meta-recording-date="2022-07-05"` ([atldotnet 155])


[spectre.console 825]: https://github.com/spectreconsole/spectre.console/issues/825
[spectre.console 842]: https://github.com/spectreconsole/spectre.console/issues/842
[spectre.console 890]: https://github.com/spectreconsole/spectre.console/issues/890
[spectre.console 891]: https://github.com/spectreconsole/spectre.console/issues/891
[atldotnet 155]: https://github.com/Zeugma440/atldotnet/issues/155
