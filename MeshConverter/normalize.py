#!/usr/bin/python

from main import Options, Mesh
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
    eigvec = eigvec[eigval.argsort()[::-1]]
    eigvec[2] = np.cross(eigvec[0], eigvec[1])

    m.vertices = [np.dot(v, eigvec) for v in m.vertices]

    return m

def flipping(m):
    c = m.triangles_center
    t = np.diag(np.append(np.sign(np.sum(np.sign(c) * c ** 2, axis=0)), 1))
    m.apply_transform(t)
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
    opts = Options('../database/step3/', '../database/step4/')
    opts.execute(handle_mesh)
