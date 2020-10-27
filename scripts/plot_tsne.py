#! /usr/bin/python

TOP=False
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

TOP = str2boolsys.argv[1]) if len(sys.argv) > 1 else False
f_i = sys.argv[2] if len(sys.argv) > 2 else "../database/output.mrtsne"
f_o = sys.argv[3] if len(sys.argv) > 3 else "../plots/step5_tsne_" + ("top" + str(TOPN) if TOP else "all") + ".jpg"

df = pd.read_csv(f_i, sep=";")

print(df.describe())

if TOP:
    classes = df.groupby("Class").count()["X"].sort_values(ascending=False)
    classes = set(classes.index[:TOPN])
    #classes.remove("handheld")
    #classes.remove("plant")
    df = df[df["Class"].isin(classes)]

sns.scatterplot(data=df, x="X", y="Y", hue="Class", legend=TOP,
        palette=sns.hls_palette(len(np.unique(df["Class"]))),
        estimator=None, alpha=1 - (not TOP) * 0.6)

plt.savefig(f_o, dpi=300, transparent=True, bbox_inches=None)
