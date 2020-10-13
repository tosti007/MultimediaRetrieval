#! /usr/bin/python

import sys
import pandas as pd
import numpy as np

df = pd.read_csv(sys.argv[1])
df = df.groupby('Class').agg([np.mean, np.std])["Distance"]
print(df.to_csv(), end='')
