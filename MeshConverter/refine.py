#!/usr/bin/python

from main import Options
from utils import fill_holes
import trimesh as tm
from meshparty import trimesh_vtk as tmvtk
from meshparty.trimesh_io import Mesh
import pyvista
import pyacvd

NUMBER_OF_SAMPLES=2000

def mesh_refine(m, mid, min_vertices=None, min_faces=None):
    v, f = (m.vertices, m.faces)
    changed = False
    
    while (min_vertices is not None and len(v) < min_vertices) or (min_faces is not None and len(f) < min_faces):
        print("Refine: ", mid)
        changed = True
        v, f = tm.remesh.subdivide(v, f)
    
    return (Mesh(v, f, process=False), changed)

def mesh_coarse(m, mid, min_vertices=None, min_faces=None):
    changed = False
    
    while (min_vertices is not None and len(m.vertices) > max_vertices) or (min_faces is not None and len(m.faces) > max_faces):
        print("Coarse: ", mid)
        red = 1 - ((max_vertices * 0.75)/len(m.faces))
        print("Reduction: ", red)
        changed = True
        v, f = tmvtk.decimate_trimesh(m, reduction=red)
        m = Mesh(v, f, process=False)
    
    return (m, changed)

def mesh_resize(opts, mid, m):
    m, changed = mesh_refine(m, mid, 1000, 1000)

    if not changed:
        m, changed = mesh_coarse(m, mid, 2000, 2000)

    return m

def mesh_resample(opts, mid, m):
    # mesh is not dense enough for uniform remeshing, as nr_vertices >> nr_samples
    # NUMBER_OF_SAMPLES * 5 seems to be a good enough amount to be dense enough
    mesh, _ = mesh_refine(m, mid, NUMBER_OF_SAMPLES * 5)
    mesh = tmvtk.trimesh_to_vtk(mesh.vertices, mesh.faces)
    mesh = pyvista.PolyData(mesh)
    clus = pyacvd.Clustering(mesh)
    clus.cluster(NUMBER_OF_SAMPLES)
    mesh = clus.create_mesh(flipnorm=False)
    mesh.compute_normals(point_normals=False, auto_orient_normals=True, inplace=True)
    points, tris, _ = tmvtk.poly_to_mesh_components(mesh)
    # TODO: tris might be none, we should do something about this.
    if tris is None:
        return None
    m = fill_holes(points, tris, mid)
    return m

def handle_mesh(opts, mid, m):
    # Since resampling our mesh is enough refining, we do not need to subdivide or coarse
    m = mesh_resample(opts, mid, m)
    return m

if __name__ == "__main__":
    opts = Options('../database/step2/', '../database/step3/')
    opts.parallel = False
    opts.execute(handle_mesh)
