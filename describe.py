#! /usr/bin/python

import sys
import pandas as pd
import numpy as np

df = pd.read_csv(sys.argv[1])
df = df.groupby('Class').agg([np.mean, np.std])["Distance"]
for c in df.columns:
    print("Column:", c)
    print(df[c].describe())
