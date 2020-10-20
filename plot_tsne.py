#! /usr/bin/python

TOP5=True

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

TOP5=str2bool(sys.argv[1])

f = sys.argv[2] if len(sys.argv) > 2 else "database/output.mrtsne"

df = pd.read_csv(f, sep=";")

if TOP5:
    classes = df.groupby("Class").count()["X"].sort_values(ascending=False).index
    df = df[df["Class"].isin(classes[-5:])]

sns.scatterplot(data=df, x="X", y="Y", hue="Class", legend=TOP5, 
        palette=sns.hls_palette(len(np.unique(df["Class"]))),
        estimator=None, alpha=1 - (not TOP5) * 0.6)

filename = "plots/step5_tsne_" + ("top5" if TOP5 else "all") + ".jpg"
plt.savefig(filename, dpi=300, transparent=True, bbox_inches=None)

