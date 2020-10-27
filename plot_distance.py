#! /usr/bin/python

NR_SELECTED = 3
SHOW_STD = False

import sys
import os
import pandas as pd
import numpy as np
import seaborn as sns
import matplotlib.pyplot as plt

sns.set_theme(style="darkgrid")

# Read the files
dfs = [f for f in sys.argv[1:]]

for i, f in enumerate(dfs):
    dfs[i] = pd.read_csv(f)
    dfs[i]["Method"] = os.path.splitext(os.path.basename(f))[0]
dfs = [df.sort_values(by=["Distance"]) for df in dfs]


# Find the mean distance for each class and sort these values
means = [df.groupby('Class').agg([np.mean, np.std])["Distance"] for df in dfs]
means = [df.sort_values(by=['mean']) for df in means]
# We only want the top n classes for our plot
# But some files may have a different top n, so select them all
selected = [df.index[:NR_SELECTED] for df in means]
selected += [df["Class"][:NR_SELECTED] for df in dfs]
selected = np.unique(selected)

print(selected)

# Filter all data with the selected classes
for i in range(len(dfs)):
    dfs[i]   = dfs[i]  [dfs[i]["Class"].isin(selected)]
    means[i] = means[i][means[i].index .isin(selected)]
    means[i]["std_min"] = means[i]["mean"] - means[i]["std"]
    means[i]["std_max"] = means[i]["mean"] + means[i]["std"]

# Plot the data as a dist plot, with a subplot for each file
colors = sns.color_palette("Set1", len(selected))
plots = sns.displot(data=pd.concat(dfs), palette=colors, kind="kde", x="Distance", hue="Class", row="Method", facet_kws={"sharex":False, "sharey":False, "legend_out":False}, height=3, aspect=3)
plots.legend.loc = "upper right"

# Draw lines of the means on each subplot
for i, df in enumerate(means):
    plots.axes[i, 0].set_xlim(left=0)
    lims = plots.axes[i, 0].viewLim
    plots.axes[i, 0].vlines(df["mean"], lims.y0, lims.y1, colors=colors)
    if SHOW_STD:
        plots.axes[i, 0].vlines(df["std_min"], lims.y0, lims.y1 * 0.75, colors=colors)
        plots.axes[i, 0].vlines(df["std_max"], lims.y0, lims.y1 * 0.75, colors=colors)
plt.savefig("plots/step4_distances.jpg", dpi=300, bbox_inches="tight")

