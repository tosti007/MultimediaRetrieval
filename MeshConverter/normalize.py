#!/usr/bin/python

from main import Options
import trimesh as tm
import os
from multiprocessing import Pool, freeze_support, cpu_count

def getId(filename):
    return os.path.splitext(filename)[0]

def normalize(m):
    translate = (m.bounds[0] + m.bounds[1]) / 2
    translate = tm.transformations.translation_matrix(-translate)
    scale = tm.transformations.scale_matrix(1 / m.scale)
    transform = tm.transformations.concatenate_matrices(scale, translate)
    m.apply_transform(transform)

def handle_file(filename):
    print("Handling: ", getId(filename))
    m = tm.load_mesh(opts.inputdir + filename)
    normalize(m)
    tm.exchange.export.export_mesh(m, opts.outputdir + getId(filename) + ".off")

if __name__ == "__main__":
    opts = Options()
    files = [f for f in os.listdir(opts.inputdir) if not f.endswith('.mr')]

    # call freeze_support() if in Windows
    if os.name == "nt":
        freeze_support()

    pool = Pool(cpu_count()) 
    results = pool.map(handle_file, files)
    print("Errors:", [x for x in results if x is not None])