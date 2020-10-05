#!/usr/bin/python

import sys, os
from main import Options
from normalize import handle_mesh as step_normalize
from clean import handle_mesh as step_clean
from refine import handle_mesh as step_refine

def handle_mesh(opts, mid, m):
    print("Clean step", mid)
    m = step_clean(opts, mid, m)
    print("Refine step", mid)
    m = step_refine(opts, mid, m)
    print("Normalize step", mid)
    m = step_normalize(opts, mid, m)
    return m

if __name__ == "__main__":
    # Write all messages output to devnull
    sys.stdout = sys.__stderr__
    opts = Options('', '')
    opts.inputdir = ''
    opts.parallel = False
    opts.execute(handle_mesh, opts.arguments)
