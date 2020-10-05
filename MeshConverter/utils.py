from meshparty import trimesh_vtk as tmvtk
from meshparty.trimesh_io import Mesh
import pyvista
import numpy as np
import networkx as nx

def show_open_edges(m):
    mesh = tmvtk.trimesh_to_vtk(m.vertices, m.faces)
    mesh = pyvista.PolyData(mesh)
    edges = mesh.extract_feature_edges(boundary_edges=True,
                            non_manifold_edges=False,
                            feature_edges=False,
                            manifold_edges=False)
    p = pyvista.Plotter()
    p.add_mesh(mesh, color=True)
    p.add_mesh(edges, color="red", line_width=5)
    p.show()

def plot_mesh(m):
    p = pyvista.Plotter()
    p.add_mesh(m, color=True)
    p.show()

def find_vertex_index(arr, item):
    result = np.where((arr == item).all(axis=1))
    if len(result) != 1:
        raise ValueError('The numer of vertex indices was '+str(len(result))+" instead of 1.")
    return result[0][0]

def fill_holes(v, f, id):
    if v is None or f is None:
        print("Empty mesh:", id)
        return Mesh(v, f)
    mesh = tmvtk.trimesh_to_vtk(v, f)
    mesh = pyvista.PolyData(mesh)
    mesh = mesh.extract_feature_edges(boundary_edges=True,
                            non_manifold_edges=False,
                            feature_edges=False,
                            manifold_edges=False)

    points, _, edges = tmvtk.poly_to_mesh_components(mesh)
    if edges is None:
        print("No edges for mesh:", id)
        return Mesh(v, f)

    edges = [p for p in edges if p[0] != p[1]]
    g = nx.Graph()
    g.add_edges_from(edges)

    for l in nx.cycle_basis(g):
        #print("Handling loop:", [find_vertex_index(v, points[i]) for i in l])
        back = 0
        left = 1
        right = -1
        flip = True
        while len(l) + right > left:
            back_id = find_vertex_index(v, points[l[back]])
            left_id = find_vertex_index(v, points[l[left]])
            right_id = find_vertex_index(v, points[l[right]])
            face = np.array([back_id, left_id, right_id])
            #print("Adding face:", face)
            f = np.append(f, [face], axis=0)
            if flip:
                back = left
                left += 1
            else:
                back = right
                right -= 1
            flip = not flip
    
    result = Mesh(v, f)
    # I dont really think this is needed as the windings should be correct,
    # but it doesnt hurt to do anyway
    result.fix_normals()
    return result
