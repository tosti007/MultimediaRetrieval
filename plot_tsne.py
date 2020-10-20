#! /usr/bin/python

TOP5=True

import sys
import numpy as np
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt

f = sys.argv[1] if len(sys.argv) > 1 else "database/output.mrtsne"


df = pd.read_csv(f, sep=";")

if TOP5:
    classes = df.groupby("Class").count()["X"].sort_values(ascending=False).index
    df = df[df["Class"].isin(classes[-5:])]

sns.scatterplot(data=df, x="X", y="Y", hue="Class", legend=TOP5, 
        palette=sns.hls_palette(len(np.unique(df["Class"]))),
        estimator=None, alpha=1 - (not TOP5) * 0.6)

filename = "plots/step5_tsne_" + ("top5" if TOP5 else "all") + ".jpg"
plt.savefig(filename, dpi=300, transparent=True, bbox_inches=None)

