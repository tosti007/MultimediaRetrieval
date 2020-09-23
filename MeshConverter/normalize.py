#!/usr/bin/python

from main import Options
import trimesh as tm
import numpy as np

def translating(m):
    # Put the barycenter of the mesh onto (0, 0, 0)
    translate = tm.transformations.translation_matrix(-m.centroid)
    m.apply_transform(translate)
    return m

def orienting(m):
    covariance = np.cov(m.vertices, rowvar=False)
    eigval, eigvec = np.linalg.eig(covariance)
    # Sort the eigenvectors by length and remove the smallest
    order = eigval.argsort()[::-1]
    eigval = eigval[order]
    eigvec = eigvec[order]

    # TODO: Make from these eigenvalues and eigenvectors a rotation matrix that moves
    # eigvec[0] to the x axis and eigvec[1] to the y axis.

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
