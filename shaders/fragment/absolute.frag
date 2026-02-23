#version 450

layout(location = 0) out vec4 outColor;

layout(set = 0, binding = 1) uniform Absolute {
    vec4 Color;
};

void main() {
    outColor = Color;
}