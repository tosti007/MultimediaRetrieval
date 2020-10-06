#!/usr/bin/bash

# Stop the script if any command fails
set -e

show () {
    RESET='\033[0m'
    COLOR='\033[0;30m\033[47m'
    FORMAT=$(($(tput cols) - ${#1}))
    printf "$COLOR%s%*s$RESET\n" "$1" $FORMAT
}

cd "$(dirname $0)"

# Make pip use local environment instead of global install
show "Using local environment"
[[ -d env ]] || python -m venv env 
[[ -f env/bin/activate ]] && source env/bin/activate
[[ -f env/scripts/activate ]] && source env/scripts/activate

if ping -q -c 1 -W 1 8.8.8.8 >/dev/null 2>&1; then
    show "Installing packages"
    pip install -q --upgrade pip
    pip install -q -r requirements.pip
fi

# From step0 to step1
show "Parse step"
python parse.py

# From step1 to step2
show "Clean step"
python clean.py

# From step2 to step3
show "Refine step"
python refine.py

# From step3 to step4
show "Normalize step"
python normalize.py

show "Done autorunning"
