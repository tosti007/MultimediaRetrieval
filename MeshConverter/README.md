# Multimedia Retrieval Project - Database Pre-Processing
This sub-project handles the preprocessing of the databases.

## Requirements
For mesh pre-processing and plotting figures `Python 3` is used. It is possible to use [Python Virtual Environments](https://docs.python.org/3/tutorial/venv.html) to keep the libraries contained to this project.

The required packages are listed below, but can also be found in `scripts/requirements.pip` and `MeshConverter/requirements.pip`. These requirement files can be used to install all packages at once with the following command:
```bash
$ pip install -r requirements.pip
```

 - numpy 1.19.2
 - seaborn 0.11.0
 - pandas 1.1.3
 - matplotlib 3.3.2
 - annotationframeworkclient 0.3.0
 - decorator 4.4.2
 - meshparty 1.8.0
 - networkx 2.5
 - numpy 1.19.2
 - pyacvd 0.2.5
 - pyvista 0.26.0
 - scipy 1.5.2
 - trimesh 3.8.8


## Running the project
This sub-project consists of 4 steps and 2 utility scripts. In this sub-project all steps should be executed in order, as described in the root `README.md`.

> **Python Virtual Environment** 
> If you are using a virtual environment for your packages don't forget to activate it!

> If the user is running on Linux a script can be used to automate the entire process of the steps below the command:
> Note: This create a new Python Virtual Environment in `MeshConverter/env`.
```bash
$ autorun.sh
```

For all steps 1 to 4 a few common arguments are possible, to eliminate repetition these arguments are listed here once. Note that in `step{n}` the `n` refers to the step number in the sections below (and thus the order of execution).

Argument              | Type      | Optional | Default | Description
----------------------|-----------|----------|---------|------------
`-i`,`--input`        | directory | True     | `../database/step{n-1}/` | This is the output directory to read the meshfiles from.
`-o`,`--output`       | directory | True     | `../database/step{n}/`   | This is the output directory to read the meshfiles to.
`-n`,`--not-parallel` | `<none>`  | True     | False | By default all meshes are processed in parallel to increase speed. However, this is not always wanted and thus can be turned off.


## Step 1 - Parsing the databases
To reorder the extracted archives from the databases into our own folder and file format, parsing of the files is required. It copies all meshfiles into the output directory and creates a file containing the mesh ids and classes per mesh.

> Note: For the LPSB dataset, to all mesh ids 1814 is added to avoid numbering-clashes.

This step takes additional arguments as shown below:

Argument      | Type      | Optional | Default | Description
--------------|-----------|----------|---------|------------
`-f`,`--file` | file path | True     | `[inputdir]/output.mr` | This is the output file to write the ids and classes to.

This step can be executed with:
```bash
$ python parse.py [ARGUMENTS]
```

## Step 2 - Cleaning the mesh files
As we do not know the quality of the meshes, or the type of meshfiles, we need to do some rudimentary cleaning of the dataset. This is handled in the clean process. 

This step can be executed with:
```bash
$ python clean.py [ARGUMENTS]
```

## Step 3 - Refining the meshes
Now all files are cleaned, uniform sampling of the mesh is required. This is done in the refine step. The refinement will take 2000 new vertices.

> Note: Due to resampling a mesh can fail to be rebuild from the sample points. In this case the mesh is not placed in the output folder and it's id is printed to the screen. If `--input` and `--output` are equal the mesh is left as-is in the folder and will cause issues in following steps.

This step can be executed with:
```bash
$ python refine.py [ARGUMENTS]
```

## Step 4 - Normalizing the meshes
Next up is normalizing the meshes. This is done in the following order:
1. Move the barycenter to (0, 0, 0)
2. Use PCA to rotate its longest two axis to X and Y.
3. Flip the mesh so most mass holding half of the mesh in on the positive side of the axis.
4. Scale the mesh so the AABB diagonal equals 1 

This step can be executed with:
```bash
$ python normalize.py [ARGUMENTS]
```

## Using the whole pre-processing pipeline
As described before we can run this whole pipeline on all meshes with a single command (`autorun.sh`), but we may want to apply this pipeline to just a single meshfile as well. This can come in handy for querying or viewing the results later on with a mesh that does not exist in the database just yet.

This command executes all previously described steps on a single mesh in memory, so it will not be written to disk in between. The script takes one single mesh filepath as an argument to run the pipeline on. The script writes any normal output to `stderr` and writes the resulting meshfile to `stdout`, this allows the user to pipe the resulting mesh to either a file or other commands (such as the C\# executable's view command).

To run the script use:
```bash
$ python pipeline.py [FILEPATH]
```
