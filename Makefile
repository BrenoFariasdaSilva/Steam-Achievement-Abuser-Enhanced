# Makefile for Steam Achievement Abuser

# Detect OS
OS := $(shell uname 2>NUL || echo Windows)

# Output directory for final executables
DIST_DIR := dist

# Default target
all: build

# Build the .NET project
build:
	cd src && dotnet build

# Clean the build
clean:
	dotnet clean
ifeq ($(OS), Windows)
	@if exist $(DIST_DIR) rmdir /S /Q $(DIST_DIR)
else
	@rm -rf $(DIST_DIR)
endif

.PHONY: all build clean
