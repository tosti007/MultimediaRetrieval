# Multimedia Retrieval Project - Root
This project is made by Brian Janssen (5661145) and Wink van Zon (5651387). 

This project consists of two sub-projects. These can be found in `MeshConverter` and `MultimediaRetrieval`. Both folder contain their own `README.md` file which lists their requirements. Please read these two and this `README.md` files prior to using this project.

> We all know that nearly nobody actually reads the guide in full before starting to execute the commands. This is fine, we do it too, but do read the `Requirements` and `Building` sections of both `README.md` files first. This reduces any let-downs. After doing that feel free to follow along whilst reading or reading everything first.

## Requirements
This project relies on a few programming tools to complete the entire pipeline. These are listed in the `README.md` files in the folder `MeshConverter` and `MultimediaRetrieval`.

## Building the project
This project only requires one sub-project to be build in advance (as the rest is either python or bash). More information can be found in `MultimediaRetrieval/README.md`

## Running the project
The project should be run in a certain order, where each step can be re-run at will. To chain the changes upwards the chain of steps, all steps starting at the redone step will have to be executed. The steps are described below and are in order.

> If the user is running on Linux a script can be used to automate the entire process of collecting the meshes and creating features with the command below.
> Note: This create a new Python Virtual Environment in `MeshConverter/env`.
> Note: This requires all requirements.
```bash
$ scripts/autorun.sh
```

## Step 1 - Preparing the database
First we will need the required meshes. To create the database in full we need to do a few substeps.

### Step 1.1 - Downloading the meshes
The user can choose which database to use (or both), the supported options are:
 - [The Princeton Shape Benchmark](https://shape.cs.princeton.edu/benchmark/)
 - [Labeled PSB Dataset](https://people.cs.umass.edu/~kalo/papers/LabelMeshes/)

All archive files should be downloaded and extracted in `database/step0/[DATABASE]`, where database refers to the selected database above. The extraction folders are already present in the `database/step0` folder. 

> If the user is running on Linux a script can be used to automate the entire process of downloading databases with the command below.
> Note: This only downloads the The Princeton Shape Benchmark database, as this was the main database chosen in the report.
```bash
$ scripts/download_meshes.sh
```

### Step 1.2 - Parsing the databases and normalizing the meshes
With the selected databases extracted in the correct `database/step0` folder, we can start processing the meshes. This is further explained in the `Running the project` section of the `MeshConverter/README.md` file.

## Step 2 - Using the database
The next step is to extract the features of the meshes and start using these features. This is further explained in the `Running the project` section of the `MultimediaRetrieval/README.md` file.

## Step N - Additional scripts
Although not part of the official assignment, some additional scripts were written to generate output (in batches). These are mostly used for evaluating small parts of the system and/or creating images for the report.

### Plotting query distances
There are multiple distance functions available for querying. In order to evaluate the differences between them a query has to be done with a mesh for all possibility. The `scripts/autorun_distance.sh` script that autmates this process of iterating all combinations and the `scripts/plot_disnance.py` python file generates a plot from the selected output. Both files can be executed from any directory, but for now we will assume the root folder.

**``autorun_distance.sh``**

This file takes one arguments, which should be a meshfile acceptable by the input of the query command of the C\# executable. This means it can be either a file path to a normalized meshfile or a mesh identifier. It can then be called, with `MESH` replaced with the correct argument, as:
```bash
$ scripts/autorun_distance.sh MESH
```

This will create a number of files in the `plots` directory in the format `Function.csv` and `FunctionFunction.csv`, where `Function` denotes a function used as distance measure.

**``plot_distance.py``**

This file takes any number of output files created with either the `autorun_distance.sh` script, as described above, or manually with the `query` command of the C\# executable and written to a file. Do not these files need to be in `csv` format, hence if creating manually the `--csv` should be used. Executing this script creates/overwrites the `plots/step4_distances.jpg` file. It can be run with the command below, where `FILE_i` can be any number of the `csv` files.

```bash
$ python scripts/plot_distance.py FILE_1 FILE_2 ... FILE_N
```

### Plotting tSNE
The tSNE implementation allows two hyper parameters to be tuned. In order to visualize the output of tSNE a `scripts/plot_tsne.py` python scripts was created. Additionally, the `scripts/autorun_tsne.sh` script was created to batch-process many possible parameters on a single feature file, which outputs all plotted images.
Both files can be executed from any directory, but for now we will assume the root folder.

> To create the tSNE output, please read the `normalize` function of the C\# executable.

**``plot_tsne.py``**

This scripts plots the output of a list of feature vectors that has been processed by tSNE. It takes up to 3 arguments:

Position | Type      | Optional | Default | Description
---------|-----------|----------|---------|------------
1        | boolean   | False    | `<none>` | This denotes if the output should contain all possible classes, or only the top 5 in number of occurances.
2        | file path | True     | `database/output.mrtsne` | The input `*.mrtsne` (csv) file that contains the featurevectors after tSNE.
3        | file path | True     | `plots/step5_tsne_top5.jpg` or `plots/step5_tsne_all.jpg` | The output file to create/overwrite. The default filename depends wether argument 1 was true or false (respectively).

The script can be executed as shown below, where ARGUMENTS is replaced with the arguments as described above.
```bash
$ python scripts/plot_tsne.py ARGUMENTS
```

**`autorun_tsne.sh`**

This file runs multiple hyper parameters for tSNE and plots the results with `plot_tsne.py` containing all classes. This script does not take any arguments. It writes the output images to `plots/tsne/tsne_P_T.jpg` where P and T are replaced with the hyper parameters Perplexity and Theta respectively.
