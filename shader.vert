#version 330 core
in vec3 aPosition;

uniform mat4 model;
uniform mat4 camera;

void main()
{
    // This is reversed from the expected order, this is due OpenTK and OpenGL
    // having transposed matrix coordinates
    gl_Position = vec4(aPosition, 1.0) * model * camera;
}