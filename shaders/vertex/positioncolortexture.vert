#version 450

layout(location = 0) in vec4 inPosition;
layout(location = 1) in vec2 inCoords;
layout(location = 2) in vec4 inColor;

layout(location = 0) out vec2 outCoords;
layout(location = 1) out vec4 outColor;

layout(set = 0, binding = 0) uniform Transforms {
    mat4 World;
    mat4 View;
    mat4 Projection;
};

void main() {
    vec4 worldPos = World * inPosition;
    vec4 viewPos = View * worldPos;
    gl_Position = Projection * viewPos;

    outCoords = inCoords;
    outColor = inColor;
}