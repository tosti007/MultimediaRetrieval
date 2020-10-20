#! /bin/bash

outdir="plots/"
call="mono bin/Debug/MultimediaRetrieval.exe "
methods="$($call distancemethods)"

echo "Using queryfile $1"
echo "Using methods:"
echo "$methods"

for i in $methods; do
	echo "Handling: $i"
	$call query "$1" -m "$i" --csv -k 0 > "$outdir$i.csv"
	for j in $methods; do
		echo "Handling: $i and $j"
		outfile=
		$call query "$1" -m "$i" "$j" --csv -k 0 > "$outdir$i$j.csv"
	done
done

exit 0
