#version 330 core
in vec3 inPosition;
in vec3 inNormal;

out vec3 outNormal;
out vec3 outPosition;

uniform mat4 model;
uniform mat4 camera;

void main()
{
    // This is reversed from the expected order, this is due OpenTK and OpenGL
    // having transposed matrix coordinates
    vec4 position = vec4(inPosition, 1.0) * model;
    gl_Position = position * camera;
    outPosition = vec3(position);

    outNormal = vec3(vec4(inNormal, 1.0) * model);
}