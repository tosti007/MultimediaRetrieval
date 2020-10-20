#! /usr/bin/python

TOP=True
TOPN=5

import sys
import numpy as np
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt

def str2bool(v):
    if isinstance(v, bool):
       return v
    if v.lower() in ('yes', 'true', 't', 'y', '1'):
        return True
    elif v.lower() in ('no', 'false', 'f', 'n', '0'):
        return False
    else:
        raise argparse.ArgumentTypeError('Boolean value expected.')

TOP=str2bool(sys.argv[1])

f = sys.argv[2] if len(sys.argv) > 2 else "database/output.mrtsne"

df = pd.read_csv(f, sep=";")

if TOP:
    classes = df.groupby("Class").count()["X"].sort_values(ascending=False)
    df = df[df["Class"].isin(classes.index[:TOPN])]

sns.scatterplot(data=df, x="X", y="Y", hue="Class", legend=TOP,
        palette=sns.hls_palette(len(np.unique(df["Class"]))),
        estimator=None, alpha=1 - (not TOP) * 0.6)

filename = "plots/step5_tsne_" + ("top" + str(TOPN) if TOP else "all") + ".jpg"
plt.savefig(filename, dpi=300, transparent=True, bbox_inches=None)

