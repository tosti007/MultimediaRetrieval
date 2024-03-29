#! /bin/bash

cd "$(dirname $0)"
cd ..

outdir="plots/tsne"
mkdir -p "$outdir"
call="mono MultimediaRetrieval/bin/Debug/MultimediaRetrieval.exe "

theta="$(seq 0.1 0.2 0.8 | sed 's/,/\./g')"
perplexity="$(seq 31 10 81)"

echo "Using theta:"
echo "$theta"

echo "Using perplexity:"
echo "$perplexity"

for t in $theta; do
	for p in $perplexity; do
		echo "Handling: $t and $p"
		$call normalize --tsne "$p,$t"
		python scripts/plot_tsne.py f database/output.mrtsne "$outdir/tsne_$(echo $p)_$t.jpg"
	done
done

exit 0
