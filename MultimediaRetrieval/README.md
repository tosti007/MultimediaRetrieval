# Multimedia Retrieval Project - Feature Extraction, Querying and Evalutation.
This sub-project handles the feature extraction and lets you query on the features and evaluate them.

## Requirements
C\# is used for the feature extraction and handling.

**For Windows**
The easiest way to build the project on Windows is to use [Visual Studio](https://visualstudio.microsoft.com/). 

**For Linux**
On Linux the project requires [MSBuild](https://github.com/dotnet/msbuild) for building the project and [Mono](https://www.mono-project.com/) for executing the executable. 

## Building the project
**For Windows**
Before you can build the actual project you will ned to build the ANN library. There is a solution file `MultimediaRetrieval/ANN/MS_Win32/ANN.sln` for the ANN C++ project. Build this solution, using the wrapper subproject as the build target. This will yield two DLL files: `MultimediaRetrieval/ANN/MS_Win32/Debug/wrapper.dll` and `MultimediaRetrieval/ANN/MS_Win32/bin/ANN.dll`. These are dependencies for the MultimediaRetrieval C\# project. The `wrapper.dll` should be found in this location by the Visual Studio, but the `ANN.dll` file should be copied to where the executable is run: Copy it to the `bin\Debug` folder.

Now simply open the solution file `MultimediaRetrieval.sln` and build the project as you would any other Visual Studio solution.

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
After building, the executable can be located at `MultimediaRetrieval/bin/Debug/MultimediaRetrieval.exe`. If the ANN DLL is located in the same folder, the executable can be run via the command line as any other executable.

**For Linux**
After building, the executable can be located at `MultimediaRetrieval/bin/Debug/MultimediaRetrieval.exe`. In order to run it we will use [Mono](https://www.mono-project.com/) for easy compatability, this can be done as shown below, where we assume it is executed from the root folder.
```bash
$ mono MultimediaRetrieval/bin/Debug/MultimediaRetrieval.exe
```

## Step 1 - Extracting features
This step reads the mesh files from a database folder. It assumes that the meshes in this database are fully normalised! (There will be errors if they are not normalized.) In addition, it reads the classes of the meshes from a class list (or a feature list). The features are then calculated per mesh and combined with the class list in a so-called feature list: This is the output file. This is a CSV document (albeit the `.mr` file extension) that contains the features for each mesh in the database.

The arguments for this command are:

Argument           | Type      | Default                    | Description
-------------------|-----------|----------------------------|------------
`-d`,`--directory` | directory | `database/step4`           | Folder to read the meshes parsed and pre-processed meshes from.
`-i`,`--input`     | file path | `database/step1/output.mr` | File path to read the class list from. This may also be a feature list.
`-o`,`--output`    | file path | `[directory]/output.mr`    | File path to write the feature list to.

To execute this step use:
```bash
$ mr.exe feature [ARGUMENTS]
```

## Step 2 - Normalizing features
This command normalizes the feature vectors contained in the feature files produced by the previous step. It has two additional features: It is possible to also generate a k-medoids clustering for the database or perform dimensionality reduction on the database using tSNE.

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
$ mr.exe normalize [ARGUMENTS]
```

## Step 3 - Querying a mesh
This command queries a mesh. This means that it takes the input mesh, compares it to the meshes in the given feature database and returns either the top k meshes, or the meshes closer than a specified distance. If the feature database is not yet normalized, it will normalize it in memory and give a warning. The distance function can be chosen, as well as the query method: normal, k-medoids or ANN.

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
`--ann`              | `<none>`         | `off`                               | Only on Windows: Use ANN to query the database. This will use the tree located at `kdtree.tree` if the `--newtree` parameter is set to off.
`--newtree`          | `<none>`         | `off`                               | Only on Windows: Generate a new tree for this ANN query and save it in `kdtree.tree`. Only works in combination with ANN.

To execute this step use:
```bash
$ mr.exe query [FILE] [ARGUMENTS]
```

## Step 4 - Evaluating performance
This command evaluates the performance of the system as seen in the report, by combining the results of each mesh in a given feature database. The output consists of various quality metrics (Precision, Recall, Accuracy, F1Score, Specificity) both for the total database and for each specific class in the database.

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
$ mr.exe evaluate [ARGUMENTS]
```

## Step N - Viewing a mesh
This step opens an OpenGL viewer window for a mesh that allows the user to inspect this mesh. The controls in this window are straightforward: The position of the camera is controlled using the WASD keys, and SPACE and SHIFT to move up and down respectively. The rotation is controlled using the arrow keys. Q and E can be used to zoom. The axes can be toggled using the Z button. The viewmode (edges, mesh, both edges and mesh) can be toggled using TAB.

The arguments for this command are:

Argument | Type      | Default  | Description
---------|-----------|----------|------------
`[FILE]` | file path | `<none>` | File to read the mesh to query on the feature list. If no input mesh is given, it will be read from `stdin`.

To execute this step use:
```bash
$ mr.exe view [FILE]
```

## Distance functions
Some commands may accept a `-m` or `--method` argument that denotes the functions used for computing distances between feature vectors. This argument accepts one of three patterns which, and their resulting behavior, are shown in the table below. In the patterns the `[F]` denotes a value of distance measure.

Pattern   | Description   | Result
----------|---------------|-------
`<none>`  | No function   | The default distance measure is used, which is `Euclidian Earthmovers` unless mentioned otherwise.
`[F]`     | One function  | The feature vector is interpreted as a single vector where the function is used once.
`[F] [F]` | Two functions | The histograms are seen as single features. The first function is used on the global features and the second function is used per histogram where the distance value is defided by the number of bins in that histogram.

There are multiple options to choose as functions, these are:

- **Euclidan**, which is the same as the L2 metric.
- **Cosine**
- **Earthmovers**, this only makes sense to use on histograms.

> Note: Distance functions are case-sensitive.

> The possible distance functions can also be found by calling the `distancemethods` command on the executable, i.e.:
```bash
$ mr.exe distancmethods
```
