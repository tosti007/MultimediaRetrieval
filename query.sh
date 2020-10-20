#! /bin/bash

call="mono bin/Debug/MultimediaRetrieval.exe "
methods="$($call distancemethods)"

files=""

echo "Using queryfile $1"
echo "Using methods:"
echo "$methods"

for i in $methods; do
	echo "Handling: $i"
	$call query "$1" -m "$i" --csv -k 0 > "$i.csv"
	for j in $methods; do
		echo "Handling: $i and $j"
		outfile="$i$j.csv"
		$call query "$1" -m "$i" "$j" --csv -k 0 > "$outfile"
		files="$files $outfile"
	done
done

echo "$files" | xargs python plot_distance.py

exit 0
