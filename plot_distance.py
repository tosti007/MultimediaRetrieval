#! /usr/bin/python

NR_SELECTED = 5
SHOW_STD = False

import sys
import pandas as pd
import numpy as np
import seaborn as sns
import matplotlib.pyplot as plt

sns.set_theme(style="darkgrid")

# Read the files
dfs = [f for f in sys.argv[1:]]

for i, f in enumerate(dfs):
    dfs[i] = pd.read_csv(f)
    dfs[i]["File"] = f
dfs = [df.sort_values(by=["Distance"]) for df in dfs]


# Find the mean distance for each class and sort these values
means = [df.groupby('Class').agg([np.mean, np.std])["Distance"] for df in dfs]
means = [df.sort_values(by=['mean']) for df in means]
# We only want the top 5 classes for our plot
# But some files may have a different top 5, so select them all
selected = [df.index[:NR_SELECTED] for df in means]
selected += [df["Class"][:5] for df in dfs]
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
plots = sns.displot(data=pd.concat(dfs), palette=colors, kind="kde", x="Distance", hue="Class", row="File")

# Draw lines of the means on each subplot
for i, df in enumerate(means):
    lims = plots.axes[i, 0].viewLim
    plots.axes[i, 0].vlines(df["mean"], lims.y0, lims.y1, colors=colors)
    if SHOW_STD:
        plots.axes[i, 0].vlines(df["std_min"], lims.y0, lims.y1 * 0.75, colors=colors)
        plots.axes[i, 0].vlines(df["std_max"], lims.y0, lims.y1 * 0.75, colors=colors)
plt.xlim(left=0)
plt.show()

