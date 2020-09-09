using OpenTK.Graphics.ES11;
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MultimediaRetrieval
{
    public class Mesh
    {
        public float[][] vertices;
        public int[][] faces;

        public Mesh(float[][] vertices, int[][] faces)
        {
            this.vertices = vertices;
            this.faces = faces;
        }

        public static Mesh ReadMesh(string filepath)
        {
            //Check if the file is an .off file:
            string[] getext = filepath.Split('.');
            string ext = getext[getext.Length - 1];
            if (ext == "off")
            {
                return ReadOffMesh(filepath);
            }
            else
            {
                throw new Exception("Unknown filetype: " + ext);
            }
        }

        //OFF mesh reader, based on work by Philip Shilane, Patrick Min, Michael Kazhdan, and Thomas Funkhouser (Princeton Shape Benchmark)
        private static Mesh ReadOffMesh(string filepath)
        {
            // Open file
            string[] lines = File.ReadAllLines(filepath);

            // Read file
            float[][] vertices = new float[0][];
            int[][] faces = new int[0][];
            int readverts = 0;
            int readfaces = 0;
            int line_count = -1;
            while (line_count < lines.Length - 1)
            {
                // Increment line counter
                line_count++;

                // Get the current line:
                string line = lines[line_count];

                // Skip blank lines and comments
                if (line[0] == '#') 
                    continue;
                if (String.IsNullOrEmpty(line)) 
                    continue;

                // Read the header first:
                if (vertices.Length == 0)
                {
                    // Read header 
                    if (!(line == "OFF"))
                    {
                        // Read mesh counts
                        string[] counts = line.Split(' ');
                        if (counts.Length != 3)
                        {
                            throw new Exception($"Syntax error reading header on line {line_count} in file {filepath}");
                        }

                        // Allocate memory for mesh
                        vertices = new float[int.Parse(counts[0])][];
                        faces = new int[int.Parse(counts[1])][];
                    }
                }
                else if (readverts < vertices.Length)
                {
                    // Read vertex coordinates
                    string[] vertex = line.Split(' ');
                    if (vertex.Length != 3)
                    {
                        throw new Exception($"Syntax error reading vertex on line {line_count} in file {filepath}");
                    }
                    vertices[readverts] = new float[3] { float.Parse(vertex[0]), float.Parse(vertex[1]), float.Parse(vertex[2]) };
                    readverts++;
                }
                else if (readfaces < faces.Length)
                {
                    // Read face:
                    string[] face = line.Split(' ');
                    int nverts = int.Parse(face[0]);
                    if (face.Length < nverts + 1)
                    {
                        throw new Exception($"Syntax error reading face on line {line_count} in file {filepath}");
                    }

                    faces[readfaces] = new int[nverts];
                    for (int i = 0; i < nverts; i++)
                        faces[readfaces][i] = int.Parse(face[i + 1]);
                    /* 
                    // Compute normal for face
                    face.normal[0] = face.normal[1] = face.normal[2] = 0;
                    Vertex* v1 = face.verts[face.nverts - 1];
                    for (i = 0; i < face.nverts; i++)
                    {
                        Vertex* v2 = face.verts[i];
                        face.normal[0] += (v1->y - v2->y) * (v1->z + v2->z);
                        face.normal[1] += (v1->z - v2->z) * (v1->x + v2->x);
                        face.normal[2] += (v1->x - v2->x) * (v1->y + v2->y);
                        v1 = v2;
                    }

                    // Normalize normal for face
                    float squared_normal_length = 0.0;
                    squared_normal_length += face.normal[0] * face.normal[0];
                    squared_normal_length += face.normal[1] * face.normal[1];
                    squared_normal_length += face.normal[2] * face.normal[2];
                    float normal_length = sqrt(squared_normal_length);
                    if (normal_length > 1.0E-6)
                    {
                        face.normal[0] /= normal_length;
                        face.normal[1] /= normal_length;
                        face.normal[2] /= normal_length;
                    }
                    */

				readfaces++;
                }
                else
                {
                    // Should never get here
                    throw new Exception($"Found extra text starting at line {line_count} in file {filepath}");
                }
            }

            // Check whether read all faces
            if (faces.Length != readfaces)
            {
                throw new Exception($"Expected {faces.Length} faces, but read only {readfaces} faces in file {filepath}");
            }

            // Return mesh 
            return new Mesh(vertices, faces);
        }
    }

}
