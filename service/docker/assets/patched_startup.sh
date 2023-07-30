#!/bin/bash

# python bin
PYTHON_BIN=python3

# packages path
PYTHON_PACKAGES=$(su-exec kallithea:kallithea $PYTHON_BIN -m site --user-site)

# kallithea installation directory
KALLITEHA_INSTALL_DIR=$PYTHON_PACKAGES/kallithea

# overwrite files
if [ -d "$KALLITHEA_OVERRIDE_DIR/kallithea" ]; then
    cp -RT "$KALLITHEA_PATCH_DIR/kallithea"  "$KALLITEHA_INSTALL_DIR"
fi

# patch files
if [ -d "$KALLITHEA_PATCH_DIR" ]; then
    git -C "$KALLITEHA_INSTALL_DIR" apply --reject --whitespace=fix -p2 $KALLITHEA_PATCH_DIR/*
fi

# normal startup
exec bash /kallithea/startup.sh

