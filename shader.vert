#version 330 core
in vec3 position;
in vec3 normal;

out vec3 color;

uniform mat4 model;
uniform mat4 camera;

uniform vec3 ambientColor;
uniform vec3 lightColor;
uniform vec3 lightPosition;
uniform vec3 objectColor;

void main()
{
    // This is reversed from the expected order, this is due OpenTK and OpenGL
    // having transposed matrix coordinates
    gl_Position = vec4(position, 1.0) * model * camera;

    vec3 lightDirection = normalize(lightPosition - vec3(model * vec4(position, 1.0)));
    vec3 diffuse = max(dot(normalize(normal), lightDirection), 0) * lightColor;

    color = (ambientColor + diffuse) * objectColor;
}