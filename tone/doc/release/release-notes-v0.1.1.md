# Release Notes

## Fixed

- `dump` command now contains better handling for file specific errors (issue #25, thx @tmr0861 for reporting this)
- `tag` and `dump` now should no longer write metadata based exceptions to stdout (issue #26, thx @advplyr for reporting this)

## Added

- `dump` now supports `--format="ChptFmtNative"` to dump chapters only
- *Serilog* library is now used to provide logging beyond command line output
- `--log-level` and `--log-file` can now be used to review error output that has been suppressed to keep it parseable
- `tone` is now also published as `musl` for `arm` and `arm64` builds (issue #31, thx @garlic82 for reporting this)
- `ffmetadata` has been improved and import has been prepared for integration in the next releases

## Changed

- `docker` builds now use `musl` deployment wherever necessary (issue #31)

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

