# Publishing and releases

The project is published at <https://github.com/rutra20/SnapLayouts10>.

## Repository layout

- `src/` — application source
- `docs/` — testing, compatibility, design, and publishing docs
- `assets/` — application icon
- `.github/` — CI, release workflow, issue templates, PR template
- `build.ps1` — compiles with the .NET Framework C# compiler
- `package.ps1` — creates portable/source archives and `SHA256SUMS.txt`

`dist/` is generated locally and ignored by Git. Binaries belong in GitHub Releases, not in the
repository.

## Local build and tests

```powershell
cd C:\path\to\SnapLayouts10
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\test-geometry.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\package.ps1 -SkipBuild
```

## Publishing a new version

1. Update the version in `src/AssemblyInfo.cs` and `CHANGELOG.md`.
2. Update release filenames/links in `README.md` if the version changed.
3. Build, run geometry tests, and package locally.
4. Commit and push:

```powershell
git add .
git commit -m "release: 1.4.0"
git push origin main
```

5. Create and push a version tag:

```powershell
git tag -a v1.4.0 -m "SnapLayouts 10 1.4.0"
git push origin v1.4.0
```

`.github/workflows/release.yml` then builds, tests, packages, and publishes the GitHub Release with:

- `SnapLayouts10.exe`
- `SnapLayouts10-1.4.0-portable.zip`
- `SnapLayouts10-1.4.0-source.zip`
- `SHA256SUMS.txt`

The version in `AssemblyInfo.cs` should match the tag.

## CI

`.github/workflows/build.yml` runs on pushes and pull requests:

- checkout
- build with `build.ps1`
- geometry tests
- package
- upload artifacts

The build must pass before a release is published.

## Repository settings

- Public repository, MIT license detected from `LICENSE`
- Topics: `windows-10`, `snap-layouts`, `snap-assist`, `winforms`, `csharp`, `productivity`, `windows`
- Issues enabled with templates
- Private vulnerability reporting enabled

## Social preview

GitHub does not expose social preview through the API. In **Settings → Social preview**, upload
[`social-preview.png`](social-preview.png) (1280 × 640).

## Code signing and SmartScreen

Release binaries are unsigned. SmartScreen may warn on first download. Options:

- publish the source and SHA-256 so users can verify;
- sign the executable with a code-signing certificate;
- do not claim that antivirus software will not react — the app sends Win32 messages to other
  windows and can trigger false positives.

## Trademarks and naming

- Do not use Microsoft logos or the word "official".
- Recommended description: **"Unofficial Snap Layouts for Windows 10"**.
- Keep `NOTICE.md` in the repository and in the portable package.
