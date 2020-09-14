#version 330 core
in vec3 outNormal;
in vec3 outPosition;

uniform vec3 lightPosition;
uniform vec3 ambientColor;
uniform vec3 lightColor;
uniform vec3 objectColor;

out vec4 FragColor;

void main()
{
    vec3 lightDirection = normalize(lightPosition - outPosition);  
    vec3 diffuse = max(dot(outNormal, lightDirection), 0.0) * lightColor;
    FragColor = vec4((ambientColor + diffuse) * objectColor, 1.0);
}
