# Release Notes

## changes
- add: `--order-by` and `--limit` parameters
- add: `--taggers` option to specify the order in which taggers are applied (e.g. `--taggers="remove,*"` first applies the remove tagger, then all others)
- add: `--script` and `--script-tagger-parameter` to create custom scriptable javascript taggers
- fix: various `mp4` tag format fixes

## known issues
- flag options (e.g. `--dry-run`) cannot be followed by arguments (e.g. `tone tag --meta-album="album" --dry-run input.mp3`) ([spectre.console 825])
  - workaround: append flag options at the end (`tone tag --meta-album="album" input.mp3 --dry-run`)
- `--meta-*` options cannot be set to empty values ([spectre.console 842])
  - workaround: use `--meta-remove-property` instead

[spectre.console 825]: https://github.com/spectreconsole/spectre.console/issues/825
[spectre.console 842]: https://github.com/spectreconsole/spectre.console/issues/842
