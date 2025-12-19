# Makefile for Steam Achievement Abuser Enhanced

# Detect OS
OS := $(shell uname 2>NUL || echo Windows)

# Output directory for final executables
DIST_DIR := dist

# Default target
all: package

# Packaging output
PKG_DIR := releases

# Install .NET SDK
install-dotnet:
ifeq ($(OS), Windows)
	@where dotnet >NUL 2>&1 || ( \
		echo Installing .NET SDK... && \
		powershell -Command "iwr -useb https://dot.net/v1/dotnet-install.ps1 | iex" && \
		echo .NET SDK installed. Please restart your terminal \
	)
else ifeq ($(OS), Darwin)
	@dotnet --version >/dev/null 2>&1 || brew install --cask dotnet-sdk
else
	@dotnet --version >/dev/null 2>&1 || curl -sSL https://dot.net/v1/dotnet-install.sh | bash
endif

# Check if .NET SDK is installed
check-dotnet:
ifeq ($(OS), Windows)
	@where dotnet >NUL 2>&1 || (echo Error: .NET SDK is not installed. && exit 1)
else
	@dotnet --version > /dev/null 2>&1 || (echo Error: .NET SDK is not installed. && exit 1)
endif

# Build the .NET project
prebuild-clean:
	@echo Cleaning previous build artifacts...
ifeq ($(OS), Windows)
	@if exist $(DIST_DIR) rmdir /S /Q $(DIST_DIR)
	@if exist src\bin rmdir /S /Q src\bin
else
	@rm -rf $(DIST_DIR) src/bin
endif

build: prebuild-clean install-dotnet check-dotnet
	cd src && dotnet build
	@$(MAKE) copy-artifacts

# Copy built artifacts into $(DIST_DIR)
copy-artifacts:
	@echo Copying build artifacts to $(DIST_DIR)...
ifeq ($(OS), Windows)
	@if not exist $(DIST_DIR) mkdir $(DIST_DIR)
	@for /r src\bin\Debug %%f in (*.exe) do @copy "%%f" $(DIST_DIR)\ >nul 2>&1
	@for /r src\bin\Debug %%g in (SAM.API.dll) do @copy "%%g" $(DIST_DIR)\ >nul 2>&1
else
	@mkdir -p $(DIST_DIR)
	@find src/bin/Debug -type f -name "*.exe" -exec cp {} $(DIST_DIR) \;
	@cp src/bin/Debug/net10.0/SAM.API.dll $(DIST_DIR)/
endif

# Prepare releases directory (clean then recreate)
prepare-releases:
	@echo Preparing '$(PKG_DIR)' (clean)...
ifeq ($(OS), Windows)
	@if exist $(PKG_DIR) rmdir /S /Q $(PKG_DIR)
	@mkdir $(PKG_DIR) >nul 2>&1 || true
else
	@rm -rf $(PKG_DIR)
	@mkdir -p $(PKG_DIR)
endif

# Package artifacts from $(DIST_DIR) into deterministic archives under $(PKG_DIR)
package-artifacts: build prepare-releases
	@echo Packaging artifacts from '$(DIST_DIR)' into '$(PKG_DIR)'...

ifeq ($(OS), Windows)
	@powershell -NoProfile -Command "$$dist = '$(DIST_DIR)'; $$pkg = '$(PKG_DIR)'; if(-not (Test-Path $$dist)) { Write-Host 'No artifacts to package (dist missing)'; exit 0 }; $$base = 'Steam-Achievement-Abuser-Enhanced-Executables'; $$staging = Join-Path $$pkg $$base; $$folder = 'Steam Achievement Abuser Enhanced'; if (Test-Path $$staging) { Remove-Item -Recurse -Force $$staging -ErrorAction SilentlyContinue }; New-Item -ItemType Directory -Path (Join-Path $$staging $$folder) -Force | Out-Null; Copy-Item -Path (Join-Path $$dist '*') -Destination (Join-Path $$staging $$folder) -Recurse -Force; $$zip = Join-Path $$pkg ($$base + '.zip'); if (Test-Path $$zip) { Remove-Item $$zip -Force -ErrorAction SilentlyContinue }; Compress-Archive -Path (Join-Path $$staging $$folder) -DestinationPath $$zip -Force; if (Get-Command 7z -ErrorAction SilentlyContinue) { & 7z a -t7z (Join-Path $$pkg ($$base + '.7z')) (Join-Path $$staging $$folder) } ; if (Get-Command rar -ErrorAction SilentlyContinue) { & rar a (Join-Path $$pkg ($$base + '.rar')) (Join-Path $$staging $$folder) } ; if (Get-Command tar -ErrorAction SilentlyContinue) { & tar -C $$staging -czf (Join-Path $$pkg ($$base + '.tar.gz')) $$folder } ; Remove-Item -Recurse -Force $$staging; Write-Host 'Created packages:' (Get-ChildItem -Path $$pkg -Filter ($$base + '*')).FullName"

else
	@# POSIX packaging for artifacts
	@ts=$$(date +%Y%m%d%H%M%S); base=Steam-Achievement-Abuser-Enhanced-Executables; staging="$(PKG_DIR)/$$base"; folder="Steam Achievement Abuser Enhanced"; \
		mkdir -p $$staging/"$$folder"; \
		cp -r "$(DIST_DIR)/." $$staging/"$$folder"/; \
		if [ -f "$(PKG_DIR)/$$base.zip" ]; then rm -f "$(PKG_DIR)/$$base.zip"; fi; \
		echo "Creating $(PKG_DIR)/$$base.zip"; (cd $(PKG_DIR) && zip -r "$$base.zip" "$$base/$$folder") > /dev/null 2>&1 || echo "zip not available"; \
		if [ -f "$(PKG_DIR)/$$base.tar.gz" ]; then rm -f "$(PKG_DIR)/$$base.tar.gz"; fi; \
		echo "Creating $(PKG_DIR)/$$base.tar.gz"; (cd $(PKG_DIR) && tar -czf "$$base.tar.gz" "$$base/$$folder") > /dev/null 2>&1 || echo "tar not available"; \
		if command -v 7z >/dev/null 2>&1; then echo "Creating $(PKG_DIR)/$$base.7z"; (cd $(PKG_DIR) && 7z a -t7z "$$base.7z" "$$base/$$folder" > /dev/null); else echo "7z not installed, skipping .7z"; fi; \
		if command -v rar >/dev/null 2>&1; then echo "Creating $(PKG_DIR)/$$base.rar"; (cd $(PKG_DIR) && rar a "$$base.rar" "$$base/$$folder" > /dev/null); else echo "rar not installed, skipping .rar"; fi; \
		echo "Packaging complete. Files in $(PKG_DIR):"; ls -1 "$(PKG_DIR)" || true; \
		rm -rf $$staging
endif

# Package repository source code into its own archive(s)
package-source: prepare-releases
	@echo Packaging source code into '$(PKG_DIR)'...


ifeq ($(OS), Windows)
	@powershell -NoProfile -Command "$$pkg = '$(PKG_DIR)'; $$base = 'Steam-Achievement-Abuser-Enhanced-Source-Code'; $$staging = Join-Path $$pkg $$base; if (Test-Path $$staging) { Remove-Item -Recurse -Force $$staging -ErrorAction SilentlyContinue }; New-Item -ItemType Directory -Path $$staging -Force | Out-Null; if (Get-Command git -ErrorAction SilentlyContinue) { & git archive --format=zip -o (Join-Path $$pkg ($$base + '.zip')) HEAD; if (Get-Command gzip -ErrorAction SilentlyContinue) { & git archive --format=tar HEAD | & gzip > (Join-Path $$pkg ($$base + '.tar.gz')) } elseif (Get-Command tar -ErrorAction SilentlyContinue) { & git archive --format=tar HEAD > (Join-Path $$pkg ($$base + '.tar')); Write-Host 'gzip not found; created plain .tar instead:' (Join-Path $$pkg ($$base + '.tar')) } else { Write-Host 'gzip/tar not available; skipping .tar.gz creation' } } else { Copy-Item -Path (Join-Path (Get-Location) '*') -Destination $$staging -Recurse -Force -Exclude '.git','$(PKG_DIR)','$(DIST_DIR)' ; $$zip = Join-Path $$pkg ($$base + '.zip'); if (Test-Path $$zip) { Remove-Item $$zip -Force -ErrorAction SilentlyContinue }; Compress-Archive -Path (Join-Path $$staging '*') -DestinationPath $$zip -Force; if (Get-Command tar -ErrorAction SilentlyContinue) { & tar -C $$staging -czf (Join-Path $$pkg ($$base + '.tar.gz')) . } ; Remove-Item -Recurse -Force $$staging } ; Write-Host 'Created source packages:' (Get-ChildItem -Path $$pkg -Filter ($$base + '*')).FullName"

else
	@# POSIX packaging for source
	@base=Steam-Achievement-Abuser-Enhanced-Source-Code; \
	if command -v git >/dev/null 2>&1; then \
		echo "Creating $(PKG_DIR)/$$base.zip and .tar.gz via git archive"; \
		git archive --format=zip -o "$(PKG_DIR)/$$base.zip" HEAD || true; \
		git archive --format=tar HEAD | gzip > "$(PKG_DIR)/$$base.tar.gz" || true; \
	else \
		staging="$(PKG_DIR)/$$base"; mkdir -p $$staging; cp -r . $$staging; rm -rf $$staging/$(PKG_DIR) $$staging/$(DIST_DIR) $$staging/.git || true; (cd $(PKG_DIR) && zip -r "$$base.zip" "$$base" ) > /dev/null 2>&1 || echo "zip not available"; (cd $(PKG_DIR) && tar -czf "$$base.tar.gz" "$$base" ) > /dev/null 2>&1 || echo "tar not available"; rm -rf $$staging; \
	fi; \
	ls -1 "$(PKG_DIR)" || true
endif

# Top-level package: produce both artifacts and source bundles
package: package-artifacts package-source
	@echo All packages created in '$(PKG_DIR)'

# Clean the build
clean:
	dotnet clean
ifeq ($(OS), Windows)
	@if exist $(DIST_DIR) rmdir /S /Q $(DIST_DIR)
else
	@rm -rf $(DIST_DIR)
endif

.PHONY: all build clean check-dotnet install-dotnet package prepare-releases package-artifacts package-source