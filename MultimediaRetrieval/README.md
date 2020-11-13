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

**For Windows**
More on this soon.

**For Linux**
After building, the executable can be located at `MultimediaRetrieval/bin/Debug/MultimediaRetrieval.exe`. In order to run it we will use [Mono](https://www.mono-project.com/) for easy compatability, this can be done as shown below, where we assume it is executed from the root folder.
```bash
$ mono MultimediaRetrieval/bin/Debug/MultimediaRetrieval.exe
```

## Step 1 - Extracting features
More on this soon.

## Step 2 - Normalizing features
More on this soon.

## Step 3 - Querying a mesh
More on this soon.

## Step 4 - Evaluating performance
More on this soon.

## Step N - Viewing a mesh
More on this soon.
