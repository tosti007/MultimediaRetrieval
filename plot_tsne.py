#! /usr/bin/python

import sys
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt

f = sys.argv[1] if len(sys.argv) > 1 else "database/output.mrtsne"

df = pd.read_csv(f, sep=";")

classes = df.groupby("Class").count()["X"].sort_values()[:-6:-1].index

sns.scatterplot(data=df, x="X", y="Y", hue="Class", legend=False)
plt.savefig("plots/step4_tsne_all.jpg", dpi=300, transparent=True, bbox_inches=None)

sns.scatterplot(data=df[df["Class"].isin(classes)], x="X", y="Y", hue="Class")
plt.savefig("plots/step4_tsne_top5.jpg", dpi=300, transparent=True, bbox_inches=None)

