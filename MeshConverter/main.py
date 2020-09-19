#!/usr/bin/python

import os, sys, getopt

class Options:
    inputdir = 'database/step1/'
    outputdir = 'database/step2/'
    classes = ''
    arguments = []

    def __init__(self, defaultinputdir, defaultoutputdir):
        self.inputdir = defaultinputdir
        self.outputdir = defaultoutputdir
        helpline = 'main.py -i <inputdir> -o <outputdir> -f <classfile, default [inputdir]/output.mr>' 
        try:
            opts, args = getopt.getopt(sys.argv[1:], "hi:o:f:",["--help", "input=","output=", "file="])
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

        if self.classes == '':
            self.classes = self.inputdir + "/output.mr"

if __name__ == "__main__":
    opts = Options()
    print("inputdir", opts.inputdir)
    print("outputdir", opts.outputdir)
    print("classes", opts.classes)
    print("arguments", opts.arguments)
