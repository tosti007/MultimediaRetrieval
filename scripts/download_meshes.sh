#!/usr/bin/bash

outdir="../tmp"
use_Princeton=true
use_LPSB=false


cd "$(dirname $0)"

mkdir -p "$outdir"

url_Princeton="https://shape.cs.princeton.edu/benchmark/download.cgi?file=download/psb_db0-3.zip https://shape.cs.princeton.edu/benchmark/download.cgi?file=download/psb_db4-9.zip https://shape.cs.princeton.edu/benchmark/download.cgi?file=download/psb_db10-13.zip https://shape.cs.princeton.edu/benchmark/download.cgi?file=download/psb_db14-18.zip"
url_LPSB="https://people.cs.umass.edu/~kalo/papers/LabelMeshes/labeledDb.7z"

if [ $use_Princeton = true ]; then
    echo "Downloading Princeton dataset"
    wget -nc -q -P "$outdir/Princeton" $url_Princeton

    for i in $(ls $outdir/Princeton/*.zip); do
        unzip -n -q -d "$outdir/Princeton" $i &
    done
fi

if [ $use_LPSB = true ]; then
    echo "Downloading LPSB dataset"
    wget -nc -q -P "$outdir/LPSB" $url_LPSB

    for i in $(ls $outdir/LPSB/*.7z); do
        # THIS IS UNTESTED
        7z -q $i &
    done
fi
