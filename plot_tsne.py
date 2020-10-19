#! /usr/bin/python

import sys
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt

f = sys.argv[1] if len(sys.argv) > 1 else "database/output.mrtsne"

df = pd.read_csv(f, sep=";")

sns.scatterplot(data=df, x="X", y="Y", hue="Class", legend=False)

plt.show()

