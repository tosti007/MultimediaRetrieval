#!/usr/bin/bash

cd "$(dirname $0)"

# Make pip use local environment instead of global install
python -m venv env
source env/bin/activate
pip install -r requirements.pip

# From step1 to step2
mkdir -p ../database/step2
python refine.py

# From step2 to step3
mkdir -p ../database/step3
python clean.py

# From step3 to step4
mkdir -p ../database/step4
python normalize.py