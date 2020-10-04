#!/usr/bin/python

from main import Options
import trimesh as tm
from meshparty import trimesh_vtk as tmvtk
from meshparty.trimesh_io import Mesh

def remove_unused_vertices(m):
    v, f = tmvtk.remove_unused_verts(m.vertices, m.faces)
    return Mesh(v, f, process=False)

def handle_mesh(opts, mid, m):
    m = remove_unused_vertices(m)
    m.process(validate=True, digits_vertex=7)
    tm.repair.fill_holes(m)
    return m
    
if __name__ == "__main__":
    opts = Options('../database/step1/', '../database/step2/')
    opts.execute(handle_mesh)
