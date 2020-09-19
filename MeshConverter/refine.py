#!/usr/bin/python

from main import Options, getId
import trimesh as tm
import os
from multiprocessing import Pool, freeze_support, cpu_count

def handle_mesh(m):
    # TODO: do something
    return m

def handle_file(filename):
    print("Handling: ", getId(filename))
    m = tm.load_mesh(opts.inputdir + filename)
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