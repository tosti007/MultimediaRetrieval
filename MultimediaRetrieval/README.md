# Multimedia Retrieval Project - Feature Extraction, Querying and Evalutation.
This sub-project handles the feature extraction and lets you query on the features and evaluate them.

## Requirements
C\# is used for the feature extraction and handling.

**For Windows**
> I don't use Windows, good luck. --- Brian Janssen

For using the ANN algortihm additional C++ is required. More on this soon.

**For Linux**
On Linux the project requires [MSBuild](https://github.com/dotnet/msbuild) for building the project and [Mono](https://www.mono-project.com/) for executing the executable. 


## Building the project
**For Windows**
More on this soon.

**For Linux**
This repository already contains a compiled executable as a kickstart. Note this is the Linux version of the executable, so misses the ANN options.
As the project contains a C\# solution file MSBuild handles everything from scratch. Simply call the command below from the root folder or without arguments in the `MultimediaRetrieval` folder.

```bash
$ msbuild MultimediaRetrieval
```

## Running the project
Executing the build project differs for each platform, hence it is described here once. In later sections doing this execution is abbriviated as `mr.exe`.

> Note: All commands listed in the following step accept the `--help` flag as an argument, which displays some helptext on how to use the application or command.

**For Windows**
More on this soon.

**For Linux**
After building, the executable can be located at `MultimediaRetrieval/bin/Debug/MultimediaRetrieval.exe`. In order to run it we will use [Mono](https://www.mono-project.com/) for easy compatability, this can be done as shown below, where we assume it is executed from the root folder.
```bash
$ mono MultimediaRetrieval/bin/Debug/MultimediaRetrieval.exe
```

## Step 1 - Extracting features
More on this soon.

The arguments for this command are:

Argument           | Type      | Default                    | Description
-------------------|-----------|----------------------------|------------
`-d`,`--directory` | directory | `database/step4`           | Folder to read the meshes parsed and pre-processed meshes from.
`-i`,`--input`     | file path | `database/step1/output.mr` | File path to read the class list from. This may also be a feature list.
`-o`,`--output`    | file path | `[directory]/output.mr`    | File path to write the feature list to.

To execute this step use:
```bash
% mr.exe feature [ARGUMENTS]
```

## Step 2 - Normalizing features
More on this soon.

The arguments for this command are:

Argument          | Type      | Default                      | Description
------------------|-----------|------------------------------|------------
`-d`,`--database` | directory | `database/step1/`            | Directory to filter the list of mesh features with, meaning if no such file exists with the corresponding id, the item is removed from the list.
`-i`, `--input`   | file path | `database/step4/output.mr`   | File to read the feature list from.
`-o`, `--output`  | file path | `database/output.mr`         | File path to write the normalized feature list to.
`-v`, `--vector`  | `<none>`  | `off`                        | Print the feature vectors for meshes with bad feature-vectors after normalizing.
`-m`, `--method`  | string    | `Euclidian Earthmovers`      | The distance function to use. This argument is further explained in the `Distance functions` section below. This does not work for tSNE.
`--medoids`       | int       | `<none>` or `|C|` if `""`    | Generate a K-Mediods cluster tree with `[ARG]` clusters and safe it to `[OUTPUT]kmed` file. If k is an empty string (`""`), it is set to the number of classes.
`--tsne`          | int,float | `<none>` or `80,0.5` if `""` | Use the tSNE algorithm to reduce the feature vector dimensionallity and safe it to `[OUTPUT]tsne` file. The parameters are seperated by a `,`. The first should be the Perplexity (int) and the second the Theta (float).

To execute this step use:
```bash
% mr.exe normalize [ARGUMENTS]
```

#### Distance functions
More on this soon.

## Step 3 - Querying a mesh
More on this soon.

To execute this step use:
```bash
% mr.exe query [ARGUMENTS]
```

## Step 4 - Evaluating performance
More on this soon.

To execute this step use:
```bash
% mr.exe evaluate [ARGUMENTS]
```

## Step N - Viewing a mesh
More on this soon.

> Note: If no input mesh is given, it will be read from `stdin`.

To execute this step use:
```bash
% mr.exe view [ARGUMENTS] [FILE]
```