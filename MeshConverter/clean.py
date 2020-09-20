#!/usr/bin/python

from main import Options, getId
import trimesh as tm
import os
from multiprocessing import Pool, freeze_support, cpu_count

def handle_mesh(m, filename):
    m.remove_unreferenced_vertices()
    m.euler_number # This should be called after remove_unreferenced_vertices
    try:
        m.process(validate=True, digits_vertex=7)
    except:
        print("Cleaning failed on " + filename)
    
def handle_file(filename):
    print("Handling: ", getId(filename))
    m = tm.load_mesh(opts.inputdir + filename)
    handle_mesh(m, filename)
    tm.exchange.export.export_mesh(m, opts.outputdir + filename)

if __name__ == "__main__":
    opts = Options('../database/step2/', '../database/step3/')
    files = [f for f in os.listdir(opts.inputdir) if not f.endswith('.mr')]

    for file in files:
        handle_file(file)

    exit()
    # call freeze_support() if in Windows
    if os.name == "nt":
        freeze_support()

    pool = Pool(cpu_count()) 
    results = pool.map(handle_file, files)
    print("Errors:", [x for x in results if x is not None])