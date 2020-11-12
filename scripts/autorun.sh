#!/usr/bin/bash

call="mono MultimediaRetrieval/bin/Debug/MultimediaRetrieval.exe "

# Stop the script if any command fails
set -e

show () {
    RESET='\033[0m'
    COLOR='\033[0;30m\033[47m'
    FORMAT=$(($(tput cols) - ${#1}))
    printf "$COLOR%s%*s$RESET\n" "$1" $FORMAT
}

cd "$(dirname $0)"
cd ..

# Download meshfiles if they don't exists yet.
if [ ! -d "database/step1" ]; then
    show "Downloading meshfiles"
    scripts/download_meshes.sh
fi

if [ ! -d "database/step4" ]; then
    show "Normalizing meshfiles"
    MeshConverter/autorun.sh
fi

show "Making features"
$call feature

show "Normalizing features"
$call normalize

show "Done autorunning"
show "Ready for query"
