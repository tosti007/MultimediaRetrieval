#!/usr/bin/python

from main import Options
import trimesh as tm
from meshparty import trimesh_vtk as tmvtk
from meshparty.trimesh_io import Mesh


def mesh_refine(m, mid, min_vertices=None, min_faces=None):
    v, f = (m.vertices, m.faces)
    changed = False
    
    while (min_vertices is not None and len(v) < min_vertices) or (min_faces is not None and len(f) < min_faces):
        print("Refine: ", mid)
        changed = True
        v, f = tm.remesh.subdivide(v, f)
    
    return (Mesh(v, f, process=False), changed)

def mesh_coarse(m, mid, min_vertices=None, min_faces=None):
    v, f = (m.vertices, m.faces)
    changed = False
    
    while (min_vertices is not None and len(v) > max_vertices) or (min_faces is not None and len(f) > max_faces):
        print("Coarse: ", mid)
        red = 1 - ((max_vertices * 0.75)/len(m.faces))
        print("Reduction: ", red)
        changed = True
        v, f = tmvtk.decimate_trimesh(m, reduction=red)
    
    return (Mesh(v, f, process=False), changed)

def mesh_resize(opts, mid, m):
    m, changed = mesh_refine(m, mid, 1000, 1000)

    if not changed:
        m, changed = mesh_coarse(m, mid, 2000, 2000)

    return m

def handle_mesh(opts, mid, m):
    m = mesh_resize(opts, mid, m)
    return m

if __name__ == "__main__":
    opts = Options('../database/step2/', '../database/step3/')
    opts.execute(handle_mesh)
