#!/usr/bin/python

from main import Options, getId
import trimesh as tm
import os
from multiprocessing import Pool, freeze_support, cpu_count
from meshparty import trimesh_vtk as tmvtk

opts = Options('../database/step3/', '../database/step4/')

def handle_mesh_refine(m):
    v, f = tm.remesh.subdivide(m.vertices, m.faces)
    return tm.base.Trimesh(v, f, process=False)

def handle_mesh_coarse(m):
    red = 1 - (1500/len(m.faces))
    print("Reduction: ", red)
    v, f = tmvtk.decimate_trimesh(m, reduction=red)
    return tm.base.Trimesh(v, f, process=False)

def handle_mesh(m):
    while len(m.vertices) < 1000 or len(m.faces) < 1000:
        print("Refine: ", getId(filename))
        m = handle_mesh_refine(m)
        return m

    if len(m.vertices) > 2000 or len(m.faces) > 2000:
        print("Coarse: ", getId(filename))
        m = handle_mesh_coarse(m)
        return m
    return m

def handle_file(filename):
    print("Handling: ", getId(filename))
    m = tm.load_mesh(opts.inputdir + filename)
    m = handle_mesh(m)
    tm.exchange.export.export_mesh(m, opts.outputdir + filename)

if __name__ == "__main__":
    opts.execute(handle_file)
