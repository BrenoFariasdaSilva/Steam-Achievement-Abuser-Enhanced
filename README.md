<div align="center">
  
# [Steam-Achievement-Abuser-Enhanced.](https://github.com/BrenoFariasdaSilva/Steam-Achievement-Abuser-Enhanced) <img src="https://github.com/simple-icons/simple-icons/blob/f38a5eb58fc130ccfbc43c9b1c8567e8217b25a6/icons/steam.svg"  width="3%" height="3%">

</div>

<div align="center">
  
---

Steam-Achievement-Abuser-Enhanced is a significantly extended and automated fork of the original Steam Achievement Abuser / Steam Achievement Manager workflow.  
Based on [Steam Achievement Manager](https://github.com/gibbed/SteamAchievementManager), this fork focuses on **automation, reproducibility, safer pacing, cleaner builds, and proper release packaging**.

It automates launching a helper app that triggers achievements for games you own, removing the need to manually open each title while adding multiple execution strategies and build tooling.

</div>

---

<div align="center">

![GitHub Code Size in Bytes](https://img.shields.io/github/languages/code-size/BrenoFariasdaSilva/Steam-Achievement-Abuser-Enhanced)
![GitHub Commits](https://img.shields.io/github/commit-activity/t/BrenoFariasDaSilva/Steam-Achievement-Abuser-Enhanced/master)
![GitHub Last Commit](https://img.shields.io/github/last-commit/BrenoFariasdaSilva/Steam-Achievement-Abuser-Enhanced)
![GitHub Forks](https://img.shields.io/github/forks/BrenoFariasDaSilva/Steam-Achievement-Abuser-Enhanced)
![GitHub Language Count](https://img.shields.io/github/languages/count/BrenoFariasDaSilva/Steam-Achievement-Abuser-Enhanced)
![GitHub License](https://img.shields.io/github/license/BrenoFariasdaSilva/Steam-Achievement-Abuser-Enhanced)
![GitHub Stars](https://img.shields.io/github/stars/BrenoFariasdaSilva/Steam-Achievement-Abuser-Enhanced)
![wakatime](https://wakatime.com/badge/github/BrenoFariasdaSilva/Steam-Achievement-Abuser-Enhanced.svg)

</div>

<div align="center">
  
![RepoBeats Statistics](https://repobeats.axiom.co/api/embed/4c88886ca92b3212cac1f69aa8d8240584ed7e30.svg "Repobeats analytics image")

</div>

## Table of Contents
- [Steam-Achievement-Abuser-Enhanced. ](#steam-achievement-abuser-enhanced-)
		- [Key features](#key-features)
	- [Table of Contents](#table-of-contents)
	- [Introduction](#introduction)
	- [Requirements](#requirements)
	- [Setup](#setup)
		- [Clone the Repository](#clone-the-repository)
		- [Build (recommended)](#build-recommended)
	- [Usage](#usage)
	- [Results](#results)
	- [Contributing](#contributing)
	- [Collaborators](#collaborators)
	- [License](#license)
		- [Apache License 2.0](#apache-license-20)

## Introduction

This repository provides a small suite of tools that automate the process of completing achievements for games you own on Steam. It is intended for users who want to locally unlock achievements (for testing, correction, or convenience) and who understand the implications of touching Steam's local APIs. Use at your own risk.

The code is organized as a Visual Studio/.NET solution with a native-wrapper library (`SAM.API`) and a set of small console apps that orchestrate achievement activation.

### Key Features

- **Three execution modes**
  - **Manual**: interactive, configurable pacing before execution.
  - **Auto**: single fully automated run with ETA calculation.
  - **Multiple Runs**: automated looping mode with run counter and fixed cooldown between cycles.
- **Estimated Total Runtime (ETA)** printed before execution, based on:
  - number of games
  - open duration per game
  - cooldown between games
- **Safe pacing by default**
  - 5 seconds with the game open
  - 5 seconds cooldown before the next game
  - Designed to reduce Steam instability when processing large libraries.
- **Improved console output**
  - Clear headers
  - Explicit “Enhanced” branding
  - GitHub repository reference on startup.
- **Makefile-based build system**
  - Validates .NET installation
  - Optionally installs the required .NET Framework
  - Builds all projects
  - Collects artifacts into `dist/`
- **Deterministic release pipeline**
  - Clean build directories before packaging
  - Reproducible release outputs
- **Cross-platform release packaging**
  - `.zip`, `.tar.gz`, `.7z`, `.rar`
  - Separate source-code and binary artifacts
- **Cleaner repository hygiene**
  - Proper `.gitignore`
  - Updated LICENSE
  - CONTRIBUTING guidelines included

## Requirements

- Windows 10 or later (tools depend on Steam and some Windows APIs).
- Steam must be installed and running with the account that owns the target games.
- .NET Framework 4.7.1 (build target) — the projects in this repository target **.NET Framework 4.7.1**.

  - To build: install the **.NET Framework 4.7.1 Developer Pack / Targeting Pack** or use a Visual Studio edition that supports .NET Framework projects. On Windows `dotnet build` can be used if MSBuild and the targeting pack are installed.
- Make (optional) — the provided `Makefile` automates the build + artifact collection; on Windows you can run it from PowerShell if `make` is available, or build directly with `dotnet build`.
- Network access (the Auto variants download a small XML index of candidate games).

## Setup

### Clone the Repository

```bash
git clone https://github.com/BrenoFariasdaSilva/Steam-Achievement-Abuser-Enhanced.git
cd Steam-Achievement-Abuser-Enhanced
```

### Build (recommended)

On Windows you can use the included `Makefile` (requires `make`) which will run the `dotnet build` and copy the built binaries to `dist/`:

```powershell
make
```

If you don't have `make` available, build directly from the `src` folder:

```powershell
cd src
dotnet build
```

After building you will find the runtime artifacts in `src/bin/Debug/` (or in `dist/` if you used the Makefile). The files you typically need are:

- `SAM.API.dll` (native wrapper)
- `Steam Achievement Abuser App.exe` (helper app used to trigger achievements for a single game)
- One of the runner apps:
  - `Steam Achievement Abuser Manual.exe` — interactive mode (asks for a pause, prompts before start)
  - `Steam Achievement Abuser Auto.exe` — runs automatically once over your games
  - `Steam Achievement Abuser Multiple Runs.exe` — runs automatically and repeats on a one-hour cycle

## Usage

1. Start Steam and make sure you're logged into the account whose achievements you intend to process.
2. Run one of the runner exes (Manual / Auto / Multiple Runs):

  - Manual: `Steam Achievement Abuser Manual.exe`
    - Prompts for a pause length (ms). Default is 5000 ms (5s open + 5s gap per game).
  - Auto: `Steam Achievement Abuser Auto.exe`
    - Automatically downloads the games index, computes an ETA, and runs once.
  - Multiple Runs: `Steam Achievement Abuser Multiple Runs.exe`
    - Same as Auto but runs in a continuous loop — it waits one hour between cycles.

3. The runner apps will print an estimated total time (in hours) before starting, based on the number of games and the configured per-game open/gap duration.

4. The tool launches the helper (`Steam Achievement Abuser App.exe`) for each game, keeps it open for a fixed period (by default 5s), attempts to close it, then waits the same fixed period before starting the next game. This pacing helps avoid Steam instability.

## Results

When the run completes the helper app will have attempted to unlock achievements for each processed game. Results are visible inside Steam (achievements unlocked). Keep the following in mind:

- This tool modifies local achievement state — use it only on accounts you control and where this behavior is acceptable.
- The default pacing (5s open + 5s gap) is conservative to avoid triggering Steam crashes when processing many games; you can adjust it in Manual mode.
- If you package artifacts for distribution, include `SAM.API.dll` plus the chosen runner exe(s).

## Releases

This project includes a packaging step that produces compressed release artifacts under the `releases/` directory. The packaging creates two logical groups of archives:

- Binary bundles (executables):
  - `Steam-Achievement-Abuser-Enhanced-Executables.zip` — A ZIP archive containing the runtime artifacts staged under the top-level folder `Steam Achievement Abuser Enhanced`. Includes `SAM.API.dll` and the helper/runner executables (App, Manual, Auto, Multiple Runs).
  - `Steam-Achievement-Abuser-Enhanced-Executables.tar.gz` — A gzipped tar of the same staged folder (created when `tar`/`gzip` are available).
  - `Steam-Achievement-Abuser-Enhanced-Executables.7z` / `.rar` — Optional 7z or RAR archives created only if the respective tools (`7z`, `rar`) are installed on the machine performing the packaging.

- Source bundles:
  - `Steam-Achievement-Abuser-Enhanced-Source-Code.zip` — ZIP archive of the repository source. When `git` is available the Makefile uses `git archive` to produce a clean archive from the current `HEAD`.
  - `Steam-Achievement-Abuser-Enhanced-Source-Code.tar.gz` or `.tar` — Created when `gzip`/`tar` are available; on Windows the Makefile will fall back to a plain `.tar` if `gzip` is not present.

How to inspect the produced archives:

On Windows (PowerShell):
```powershell
ls releases
# Unzip: Expand-Archive -Path releases\Steam-Achievement-Abuser-Enhanced-Executables.zip -DestinationPath .\out
# List tar.gz: tar -tzf releases\Steam-Achievement-Abuser-Enhanced-Executables.tar.gz
```

On POSIX systems:
```bash
ls releases
unzip -l releases/Steam-Achievement-Abuser-Enhanced-Executables.zip
tar -tzf releases/Steam-Achievement-Abuser-Enhanced-Executables.tar.gz
```

If you'd like different archive naming (for example including a git short-hash or version tag) or additional exclusions (e.g., exclude `obj`, `bin`, `.vs` from the source bundle), tell me and I can update the Makefile accordingly.

## Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**. If you have suggestions for improving the code, your insights will be highly welcome.
In order to contribute to this project, please follow the guidelines below or read the [CONTRIBUTING.md](CONTRIBUTING.md) file for more details on how to contribute to this project, as it contains information about the commit standards and the entire pull request process.
Please follow these guidelines to make your contributions smooth and effective:

1. **Set Up Your Environment**: Ensure you've followed the setup instructions in the [Setup](#setup) section to prepare your development environment.

2. **Make Your Changes**:
   - **Create a Branch**: `git checkout -b feature/YourFeatureName`
   - **Implement Your Changes**: Make sure to test your changes thoroughly.
   - **Commit Your Changes**: Use clear commit messages, for example:
     - For new features: `git commit -m "FEAT: Add some AmazingFeature"`
     - For bug fixes: `git commit -m "FIX: Resolve Issue #123"`
     - For documentation: `git commit -m "DOCS: Update README with new instructions"`
     - For refactorings: `git commit -m "REFACTOR: Enhance component for better aspect"`
     - For snapshots: `git commit -m "SNAPSHOT: Temporary commit to save the current state for later reference"`
   - See more about crafting commit messages in the [CONTRIBUTING.md](CONTRIBUTING.md) file.

3. **Submit Your Contribution**:
   - **Push Your Changes**: `git push origin feature/YourFeatureName`
   - **Open a Pull Request (PR)**: Navigate to the repository on GitHub and open a PR with a detailed description of your changes.

4. **Stay Engaged**: Respond to any feedback from the project maintainers and make necessary adjustments to your PR.

5. **Celebrate**: Once your PR is merged, celebrate your contribution to the project!

## Collaborators

We thank the following people who contributed to this project:

<table>
  <tr>
    <td align="center">
      <a href="https://github.com/BrenoFariasdaSilva" title="Breno Farias da Silva">
        <img src="https://github.com/BrenoFariasdaSilva.png" width="100px;" alt="Breno Farias da Silva"/><br>
        <sub>
          <b>Breno Farias da Silva</b>
        </sub>
      </a>
    </td>
    <td align="center">
      <a href="https://github.com/sa68ru/Steam-Achievement-Abuser" title="sa68ru">
        <img src="https://github.com/sa68ru.png" width="100px;" alt="sa68ru"/><br>
        <sub>
          <b>sa68ru</b>
        </sub>
      </a>
    </td>
    <td align="center">
      <a href="https://github.com/4G0NYY/Steam-Achievement-Abuser" title="4G0NYY">
        <img src="https://github.com/4G0NYY.png" width="100px;" alt="4G0NYY"/><br>
        <sub>
          <b>4G0NYY</b>
        </sub>
      </a>
    </td>
  </tr>
</table>

## License

### Apache License 2.0

This project is licensed under the [Apache License 2.0](LICENSE). This license permits use, modification, distribution, and sublicense of the code for both private and commercial purposes, provided that the original copyright notice and a disclaimer of warranty are included in all copies or substantial portions of the software. It also requires a clear attribution back to the original author(s) of the repository. For more details, see the [LICENSE](LICENSE) file in this repository.
