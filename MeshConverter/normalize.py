#!/usr/bin/python

from main import Options
import trimesh as tm

def translating(m):
    # Put the barycenter of the mesh onto (0, 0, 0)
    translate = tm.transformations.translation_matrix(-m.centroid)
    m.apply_transform(translate)
    return m

def orienting(m):
    # TODO
    transform = m.principal_inertia_transform
    m.apply_transform(transform)
    return m

def flipping(m):
    # TODO
    return m

def scaling(m):
    scale = tm.transformations.scale_matrix(1 / m.scale)
    m.apply_transform(scale)
    return m

def handle_mesh(opts, mid, m):
    # Following the lectures the order is:
    m = translating(m)
    m = orienting(m)
    m = flipping(m)
    m = scaling(m)
    return m

if __name__ == "__main__":
    opts = Options('../database/step1/', '../database/step2/')
    opts.execute(handle_mesh)
