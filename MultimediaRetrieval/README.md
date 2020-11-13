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
`-i`,`--input`    | file path | `database/step4/output.mr`   | File to read the feature list from.
`-o`,`--output`   | file path | `database/output.mr`         | File path to write the normalized feature list to.
`-v`,`--vector`   | `<none>`  | `off`                        | Print the feature vectors for meshes with bad feature-vectors after normalizing.
`-m`,`--method`   | string    | `Euclidian Earthmovers`      | The distance function to use. This argument is further explained in the `Distance functions` section below. This does not work for tSNE.
`--medoids`       | int       | `<none>` or `|C|` if `""`    | Generate a K-Mediods cluster tree with `[ARG]` clusters and safe it to `[OUTPUT]kmed` file. If k is an empty string (`""`), it is set to the number of classes.
`--tsne`          | int,float | `<none>` or `80,0.5` if `""` | Use the tSNE algorithm to reduce the feature vector dimensionallity and safe it to `[OUTPUT]tsne` file. The parameters are seperated by a `,`. The first should be the Perplexity (int) and the second the Theta (float).

To execute this step use:
```bash
% mr.exe normalize [ARGUMENTS]
```

## Step 3 - Querying a mesh
More on this soon.

The arguments for this command are:

Argument             | Type             | Default                             | Description
---------------------|------------------|-------------------------------------|------------
`[FILE]`             | file path or int | `<none>`                            | File to read the mesh to query on the feature list. If a mesh id is given instead of file path, the feature list is searched for that id and it is used instead. If no input mesh is given, it will be read from `stdin`.
`-d`,`--database`    | directory        | `database/step1/`                   | Directory to filter the list of mesh features with, meaning if no such file exists with the corresponding id, the item is removed from the list.
`-i`,`--input`       | file path        | `database/step4/output.mr`          | File to read the feature list from.
`-v`,`--vector`      | `<none>`         | `off`                               | Print the feature vectors for meshes with bad feature-vectors after normalizing.
`-m`,`--method`      | string           | `Euclidian Earthmovers`             | The distance function to use. This argument is further explained in the `Distance functions` section below. This does not work for tSNE.
`-k`,`--k_parameter` | int              | if no `t` is given 5, else `<none>` | The number of meshes to return. K and T cannot both be set.
`-t`,`--t_parameter` | float            | `<none>`                            | Return the meshes with a distance less than or equal to t. `-k` and `-t` cannot both be set.
`--csv`              | `<none>`         | `off`                               | Output the results as csv instead of text.
`--medoids`          | `<none>`         | `off`                               | Use the K-Medoids tree created with the `normalize` command for searching. This is compatible with `-k` and `-t`.

To execute this step use:
```bash
% mr.exe query [FILE] [ARGUMENTS]
```

## Step 4 - Evaluating performance
More on this soon.

The arguments for this command are shown below. If no `-k` or `-t` option is set, then the `-k` value is chosen automatically. This value will vary per mesh, where it will be the number meshes with the same class.

Argument             | Type             | Default                     | Description
---------------------|------------------|-----------------------------|------------
`-d`,`--database`    | directory        | `database/step1/`           | Directory to filter the list of mesh features with, meaning if no such file exists with the corresponding id, the item is removed from the list.
`-i`,`--input`       | file path        | `database/step4/output.mr`  | File to read the feature list from.
`-v`,`--vector`      | `<none>`         | `off`                       | Print the feature vectors for meshes with bad feature-vectors after normalizing.
`-m`,`--method`      | string           | `Euclidian Earthmovers`     | The distance function to use. This argument is further explained in the `Distance functions` section below. This does not work for tSNE.
`-k`,`--k_parameter` | int              | `<none>`                    | The number of meshes to return. K and T cannot both be set.
`-t`,`--t_parameter` | float            | `<none>`                    | Return the meshes with a distance less than or equal to t. `-k` and `-t` cannot both be set.
`--csv`              | `<none>`         | `off`                       | Output the results as csv instead of text.
`--medoids`          | `<none>`         | `off`                       | Use the K-Medoids tree created with the `normalize` command for searching. This is compatible with `-k` and `-t`.


To execute this step use:
```bash
% mr.exe evaluate [ARGUMENTS]
```

## Step N - Viewing a mesh
More on this soon.

The arguments for this command are:

Argument | Type      | Default  | Description
---------|-----------|----------|------------
`[FILE]` | file path | `<none>` | File to read the mesh to query on the feature list. If no input mesh is given, it will be read from `stdin`.

To execute this step use:
```bash
% mr.exe view [FILE]
```

#### Distance functions
More on this soon.
