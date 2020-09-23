#!/usr/bin/python

from main import Options, getId
import trimesh as tm
from meshparty import trimesh_vtk as tmvtk
from meshparty.trimesh_io import Mesh

opts = Options('../database/step2/', '../database/step3/')

def remove_unused_vertices(m):
    v, f = tmvtk.remove_unused_verts(m.vertices, m.faces)
    return Mesh(v, f)

def handle_mesh(m, filename):
    m = remove_unused_vertices(m)
    m.process(validate=True, digits_vertex=7)
    m.fix_mesh()
    return m
    
def handle_file(filename):
    print("Handling: ", getId(filename))
    m = tm.load_mesh(opts.inputdir + filename)
    m = handle_mesh(m, filename)
    tm.exchange.export.export_mesh(m, opts.outputdir + filename)

if __name__ == "__main__":
    opts.execute(handle_file)
