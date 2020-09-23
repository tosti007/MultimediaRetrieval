#!/usr/bin/python

from main import Options, getId
import trimesh as tm
import numpy as np
import os
from multiprocessing import Pool, freeze_support, cpu_count

opts = Options('../database/step1/', '../database/step2/')

# Taken from https://github.com/mikedh/trimesh/blob/fef8efb5ac7e56aae795da9333a58b26061001c6/trimesh/base.py#L730
def principal_inertia_transform(m):
    order = np.argsort(m.principal_inertia_components)[1:][::-1]
    vectors = m.principal_inertia_vectors[order]
    vectors = np.vstack((vectors, np.cross(*vectors)))

    transform = np.eye(4)
    transform[:3, :3] = vectors
    transform = tm.transformations.transform_around(
        matrix=transform,
        point=np.zeros(3))

    m.apply_transform(transform)

def translating(m):
    translate = (m.bounds[0] + m.bounds[1]) / 2
    translate = tm.transformations.translation_matrix(-translate)
    m.apply_transform(translate)
    return m

def orienting(m):
    # TODO
    principal_inertia_transform(m)
    return m

def flipping(m):
    # TODO
    return m

def scaling(m):
    scale = tm.transformations.scale_matrix(1 / m.scale)
    m.apply_transform(scale)
    return m

def handle_mesh(m):
    # Following the lectures the order is:
    m = translating(m)
    m = orienting(m)
    m = flipping(m)
    m = scaling(m)
    return m

def handle_file(filename):
    print("Handling: ", getId(filename))
    m = tm.load_mesh(opts.inputdir + filename)
    m = handle_mesh(m)
    tm.exchange.export.export_mesh(m, opts.outputdir + getId(filename) + ".off")

if __name__ == "__main__":
    opts.execute(handle_file)
