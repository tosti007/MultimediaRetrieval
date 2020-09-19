#!/usr/bin/python

from main import Options, install_package
import pymesh

#install_package("pandas"); import pandas as pd

if __name__ == "__main__":
    opts = Options()
    print("Handling: ", opts.arguments[0][1:])
    mesh = pymesh.load_mesh(opts.arguments[0]);
    print(mesh.num_vertices)
    
