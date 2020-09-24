#!/usr/bin/python

import os, sys, getopt
from multiprocessing import Pool, freeze_support, cpu_count
from trimesh import load_mesh
from trimesh.exchange.export import export_mesh
from meshparty.trimesh_io import Mesh

def getId(filename):
    return os.path.splitext(os.path.basename(filename))[0]

def run_in_batch(files, func):
    print("Handling", len(files), "files")

    # call freeze_support() if in Windows
    if os.name == "nt":
        freeze_support()

    pool = Pool(cpu_count())
    results = pool.map(func, files)
    return [x for x in results if x is not None]

def list_unmodified_files(opts):
    # Get the most recent edittime of the called file
    recentedit = os.path.getmtime(sys.argv[0])
    isdone = lambda f: f.endswith('.mr') or (os.path.isfile(f) and os.path.getmtime(f) > recentedit)
    return [f for f in os.listdir(opts.inputdir) if not isdone(f)]

def load_and_handle(args):
    opts, filename = args
    mid = getId(filename)
    print("Handling: ", mid)
    m = load_mesh(opts.inputdir + filename)
    m = opts.handlefunction(opts, mid, Mesh(m.vertices, m.faces, process=False))
    export_mesh(m, opts.outputdir + mid + ".off")

def mkdir(path):
    if not os.path.exists(path):
        os.mkdir(path)

class Options:
    handlefunction = None
    inputdir = 'database/step1/'
    outputdir = 'database/step2/'
    classes = ''
    parallel = True
    runs = None
    arguments = []

    def __init__(self, defaultinputdir, defaultoutputdir, useinputdir=True):
        self.inputdir = defaultinputdir
        self.outputdir = defaultoutputdir
        helpline = os.path.basename(getId(sys.argv[0])) + '''
        -h,--help <show this message>
        -i,--input [inputdir]
        -o,--output [outputdir]
        -f,--file [classfile] <default [inputdir]/output.mr>
        -n,--not-parallel <if this flag is given, handle files sequential instead of parallel>
        -r,--runs [number of refinement runs] <otherwise minimal 100 vertices/faces>'''
        try:
            opts, args = getopt.getopt(sys.argv[1:], "hni:o:f:r:", ["--help", "--not-parallel", "input=","output=", "file=", "runs="])
            self.arguments = args
        except getopt.GetoptError:
            print(helpline)
            sys.exit(2)
        for opt, arg in opts:
            if opt in ('-h', "--help"):
                print(helpline)
                sys.exit()
            elif opt in ("-i", "--input"):
                self.inputdir = os.path.join(arg, '')
            elif opt in ("-o", "--output"):
                self.outputdir = os.path.join(arg, '')
            elif opt in ("-f", "--file"):
                self.classes = arg
            elif opt in ("-n", "--not-parallel"):
                self.parallel = False
            elif opt in ("-r", "--runs"):
                self.runs = arg

        if self.classes == '':
            if useinputdir:
                self.classes = self.inputdir
            else:
                self.classes = self.outputdir
            self.classes += "output.mr"

    def execute(self, func, files=None):
        mkdir(self.inputdir)
        mkdir(self.outputdir)
        self.handlefunction = func
        if files is None:
            files = list_unmodified_files(self)
        if self.parallel:
            results = run_in_batch([(self, f) for f in files], load_and_handle)
            if len(results) > 0:
                print("Errors:", results)
        else:
            for f in files:
                load_and_handle((self, f))

if __name__ == "__main__":
    opts = Options(lambda o, i, m: print(i), 'input', 'output')
    print("inputdir", opts.inputdir)
    print("outputdir", opts.outputdir)
    print("classes", opts.classes)
    print("parallel", opts.parallel)
    print("runs", opts.runs)
    print("arguments", opts.arguments)
