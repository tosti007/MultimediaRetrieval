#!/usr/bin/python

from main import Options
import trimesh as tm
from meshparty import trimesh_vtk as tmvtk
from meshparty.trimesh_io import Mesh

def handle_mesh_refine(m):
    v, f = tm.remesh.subdivide(m.vertices, m.faces)
    return Mesh(v, f, process=False)

def handle_mesh_coarse(m):
    red = 1 - (1500/len(m.faces))
    print("Reduction: ", red)
    v, f = tmvtk.decimate_trimesh(m, reduction=red)
    return Mesh(v, f, process=False)

def handle_mesh(opts, mid, m):
    while len(m.vertices) < 1000 or len(m.faces) < 1000:
        print("Refine: ", mid)
        m = handle_mesh_refine(m)
        return m

    if len(m.vertices) > 2000 or len(m.faces) > 2000:
        print("Coarse: ", mid)
        m = handle_mesh_coarse(m)
        return m
    return m

if __name__ == "__main__":
    opts = Options('../database/step1/', '../database/step2/')
    opts.execute(handle_mesh)
