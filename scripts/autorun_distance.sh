#! /bin/bash

outdir="plots/"
call="mono MultimediaRetrieval/bin/Debug/MultimediaRetrieval.exe "

if [[ -z "$1" ]]; then
	echo "No meshfile set!"
	exit 1
fi

cd "$(dirname $0)"
cd ..
mkdir -p "$outdir"
methods="$($call distancemethods)"

echo "Using queryfile $1"
echo "Using methods:"
echo "$methods"

for i in $methods; do
	echo "Handling: $i"
	$call query "$1" -m "$i" --csv -k 0 > "$outdir$i.csv"
	for j in $methods; do
		echo "Handling: $i and $j"
		$call query "$1" -m "$i" "$j" --csv -k 0 > "$outdir$i$j.csv"
	done
done

exit 0
