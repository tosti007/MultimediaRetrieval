#version 330 core
in vec3 position;

uniform mat4 model;
uniform mat4 camera;

void main()
{
    // This is reversed from the expected order, this is due OpenTK and OpenGL
    // having transposed matrix coordinates
    gl_Position = vec4(position, 1.0) * model * camera;
}