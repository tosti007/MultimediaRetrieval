#!/usr/bin/python

from main import Options, getExt, getId, mkdir
import os
import glob
import shutil
import numpy as np
from trimesh.exchange.load import mesh_formats

IGNORE_LIST = np.loadtxt("ignore.list", dtype=np.int)
SUPPORTED_EXTENSIONS = ["." + f for f in mesh_formats()]
NR_PRINCETON_MESHES = 1814

def listMeshes(dirpath):
    for f in glob.iglob(dirpath + "**", recursive=True):
        if os.path.isdir(f):
            continue
        if f.endswith("~"):
            continue
        if getExt(f) not in SUPPORTED_EXTENSIONS:
            continue
        yield f

def moveFile(filename, outputdir, fid):
    if fid in IGNORE_LIST:
        return
    destination = outputdir + fid + getExt(filename)
    if not os.path.isfile(destination):
        shutil.copyfile(filename, destination)

def parseLPSB(total, inputdir, outputdir):
    print("Parsing LPSB dataset", inputdir)
    for f in listMeshes(inputdir):
        cla = os.path.basename(os.path.dirname(f)).lower()
        fid = str(int(getId(f)) + NR_PRINCETON_MESHES)
        total[fid] = cla
        moveFile(f, outputdir, fid)

def parsePrinceton(total, inputdir, filepath, outputdir):
    print("Parsing Princeton dataset", filepath)
    tmp = dict()
    with open(filepath, 'r') as file:
        for line in file:
            if not line:
                continue
            if line.startswith("PSB"):
                continue
            line = line.split()
            if len(line) != 3:
                continue
            cla = line[0].lower()
            for i in range(int(line[2])):
                line = file.readline()[:-1]
                tmp[line] = cla

    for f in listMeshes(inputdir):
        fid = getId(f)[1:]
        if not fid in tmp:
            continue
        total[fid] = tmp[fid]
        moveFile(f, outputdir, fid)

def parseUnkown(inputdir, outputdir, total):
    if len(glob.glob(inputdir + "*.mr")) > 0:
        raise Exception("This folder contains already a statistic file")

    files = glob.glob(inputdir + "*.cla")
    if len(files) > 0:
        for f in files:
            parsePrinceton(total, inputdir, f, outputdir)
        return total

    # Assuming it's LPSB since we could not find any class file
    parseLPSB(total, inputdir, outputdir);
    return total

def parse(inputdir, outputdir):
    total = dict()
    for folder in glob.iglob(inputdir + "*"):
        total = parseUnkown(folder+"/", outputdir, total)
    return total

def writeToFile(total, filepath):
    with open(filepath, 'w') as file:
        file.write("ID;Class\n")
        file.writelines("%s;%s\n"%v for v in total.items())

if __name__ == "__main__":
    opts = Options('../database/step0/', '../database/step1/', False)
    mkdir(opts.outputdir)
    total = parse(opts.inputdir, opts.outputdir)
    writeToFile(total, opts.classes)
