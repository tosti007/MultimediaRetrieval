#!/usr/bin/bash

#cd $(dirname $0)
#cd ..

script=$(realpath --relative-to="$(dirname $0)" "$1")
shift

docker run -it --rm \
    -v $(realpath database):/database:rw \
    -v $(realpath MeshConverter):/root:ro \
    pymesh/pymesh python "$script" "$@"
