#!/bin/bash

# python bin
PYTHON_BIN=python3

# packages path
PYTHON_PACKAGES=$(su-exec kallithea:kallithea $PYTHON_BIN -m site --user-site)

# kallithea installation directory
KALLITEHA_INSTALL_DIR=$PYTHON_PACKAGES/kallithea

# overwrite patch files
KALLITHEA_PATCH_DIR=/kallithea/assets/patch/kallithea
if [ -d "$KALLITHEA_PATCH_DIR" ]; then
    cp -RT "$KALLITHEA_PATCH_DIR"  "$KALLITEHA_INSTALL_DIR"
fi

# normal startup
exec bash /kallithea/startup.sh

