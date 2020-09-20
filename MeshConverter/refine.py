#!/usr/bin/python

from main import Options, getId
import trimesh as tm
import os
from multiprocessing import Pool, freeze_support, cpu_count

opts = Options('../database/step3/', '../database/step4/')

def handle_mesh(m):
    v, f = tm.remesh.subdivide(m.vertices, m.faces)
    return tm.base.Trimesh(v, f)

def handle_file(filename):
    print("Handling: ", getId(filename))
    m = tm.load_mesh(opts.inputdir + filename)
    if opts.runs is None:
        while len(m.vertices) < 100 or len(m.faces) < 100:
            m = handle_mesh(m)
    else:
        for n in range(opts.runs):
            m = handle_mesh(m)
    
    tm.exchange.export.export_mesh(m, opts.outputdir + filename)

if __name__ == "__main__":
    opts = Options('../database/step3/', '../database/step4/')
    files = [os.path.splitext(f)[0] + ".off" for f in opts.arguments]

    # call freeze_support() if in Windows
    if os.name == "nt":
        freeze_support()

    pool = Pool(cpu_count()) 
    results = pool.map(handle_file, files)
    print("Errors:", [x for x in results if x is not None])