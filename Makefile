# Makefile for Steam Achievement Abuser

# Detect OS
OS := $(shell uname 2>NUL || echo Windows)

# Output directory for final executables
DIST_DIR := dist

# Default target
all: build

# Check if .NET SDK is installed
check-dotnet:
ifeq ($(OS), Windows)
	@where dotnet >NUL 2>&1 || (echo Error: .NET SDK is not installed. && exit 1)
else
	@dotnet --version > /dev/null 2>&1 || (echo Error: .NET SDK is not installed. && exit 1)
endif

# Build the .NET project
build: check-dotnet
	cd src && dotnet build
	@echo Copying build artifacts to $(DIST_DIR)...
ifeq ($(OS), Windows)
	@if not exist $(DIST_DIR) mkdir $(DIST_DIR)
	@for /r src\bin\Debug %%f in (*.exe) do @copy "%%f" $(DIST_DIR)\ >nul 2>&1
	@copy src\bin\Debug\net10.0\SAM.API.dll $(DIST_DIR)\ >nul 2>&1
else
	@mkdir -p $(DIST_DIR)
	@find src/bin/Debug -type f -name "*.exe" -exec cp {} $(DIST_DIR) \;
	@cp src/bin/Debug/net10.0/SAM.API.dll $(DIST_DIR)/
endif

# Clean the build
clean:
	dotnet clean
ifeq ($(OS), Windows)
	@if exist $(DIST_DIR) rmdir /S /Q $(DIST_DIR)
else
	@rm -rf $(DIST_DIR)
endif

.PHONY: all build clean check-dotnet
