#!/usr/bin/python

from main import Options
from normalize import handle_mesh as step_normalize
from clean import handle_mesh as step_clean
from refine import handle_mesh as step_refine

def handle_mesh(opts, mid, m):
    m = step_normalize(m)
    m = step_clean(m)
    m = step_refine(m)
    return m

if __name__ == "__main__":
    opts = Options('', '../database/pipeline/')
    opts.inputdir = ''
    opts.execute(handle_mesh, opts.arguments)
