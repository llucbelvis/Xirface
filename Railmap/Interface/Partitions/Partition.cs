using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Hjson;
using System.ComponentModel.Design.Serialization;


using Silk.NET.OpenGL;
using Silk.NET.Input;
using System.Numerics;

namespace Xirface
{
    public class RootConverter : JsonConverter<Root>
    {
        public override Root Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("type", out JsonElement typeElement))
                throw new JsonException("Missing 'type' property");

            string typeName = typeElement.GetString()!;

            return typeName switch
            {
                "frame" => JsonSerializer.Deserialize<Frame>(root.GetRawText(), options)!,
                "texture" => JsonSerializer.Deserialize<Texture>(root.GetRawText(), options)!,
                "text" => JsonSerializer.Deserialize<Text>(root.GetRawText(), options)!,
                "textbox" => JsonSerializer.Deserialize<TextBox>(root.GetRawText(), options)!,
                _ => throw new JsonException($"Unknown type: {typeName}")
            };
        }

        public override void Write(Utf8JsonWriter writer, Root value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader jsonReader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (jsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Vector2 must be an array[x,y]");

            jsonReader.Read();

            float x = jsonReader.GetSingle();
            jsonReader.Read();

            float y = jsonReader.GetSingle();

            jsonReader.Read();

            return new Vector2(x, y);
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

    }

    public class ColorConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader jsonReader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (jsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Color must be an array[x,y,z,w]");

            jsonReader.Read();

            float r = jsonReader.GetSingle();
            jsonReader.Read();

            float g = jsonReader.GetSingle();
            jsonReader.Read();

            float b = jsonReader.GetSingle();
            jsonReader.Read();

            float a = jsonReader.GetSingle();
            jsonReader.Read();

            return new Color((byte)r,(byte)g,(byte)b,(byte)a);
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

    }

    public class TextureConverter : JsonConverter<Texture2D>
    {
        AssetManager AssetManager;

        public TextureConverter(AssetManager ContentManager)
        {
            this.AssetManager = ContentManager;
        }

        public override Texture2D Read(ref Utf8JsonReader jsonReader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (jsonReader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            string path = jsonReader.GetString()!;
            Debug.WriteLine($"Loading texture: '{path}'");
            if (!string.IsNullOrEmpty(path))
            {
                return AssetManager.Load<Texture2D>(path);
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, Texture2D value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

    }

    public class AlignmentConverter : JsonConverter<String.Alignment>
    {
        public override String.Alignment Read(ref Utf8JsonReader jsonReader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (jsonReader.TokenType == JsonTokenType.Null)
            {
                return String.Alignment.BottomLeft;
            }

            string alignment = jsonReader.GetString()!;

            if (!string.IsNullOrEmpty(alignment))
            {
                switch (alignment)
                {
                    case "bottom-left":
                        return String.Alignment.BottomLeft;

                    case "bottom-center":
                        return String.Alignment.BottomCenter;

                    case "bottom-right":
                        return String.Alignment.BottomRight;

                    case "center-left":
                        return String.Alignment.CenterLeft;

                    case "center-center":
                        return String.Alignment.CenterCenter;

                    case "center-right":
                        return String.Alignment.CenterRight;

                    case "top-left":
                        return String.Alignment.TopLeft;

                    case "top-center":
                        return String.Alignment.TopCenter;

                    case "top-right":
                        return String.Alignment.TopRight;
                }

            }

            return String.Alignment.BottomLeft;
        }
        public override void Write(Utf8JsonWriter writer, String.Alignment value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class PositioningConverter : JsonConverter<Root.Positioning>
    {
        public override Root.Positioning Read(ref Utf8JsonReader jsonReader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (jsonReader.TokenType == JsonTokenType.Null)
            {
                return Root.Positioning.Zero;
            }

            string alignment = jsonReader.GetString()!;

            if (!string.IsNullOrEmpty(alignment))
            {
                switch (alignment)
                {
                    case "hierarchical":
                        return Root.Positioning.Hierarchical;

                    case "absolute":
                        return Root.Positioning.Absolute;
                    case "zero":
                        return Root.Positioning.Zero;



                }

            }

            return Root.Positioning.Hierarchical;
        }
        public override void Write(Utf8JsonWriter writer, Root.Positioning value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }

    public class FontConverter : JsonConverter<Font>
    {
        AssetManager AssetManager;
        public FontConverter(AssetManager assetManager)
        {
            this.AssetManager = assetManager;
        }

        public override Font Read(ref Utf8JsonReader jsonReader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (jsonReader.TokenType == JsonTokenType.Null)
            {
                return null!;
            }

            string font = jsonReader.GetString()!;

            if (!string.IsNullOrEmpty(font))
            {
                switch (font)
                {
                    case "Light":
                        return AssetManager.Load<Font>("Fonts/Light.ttf");
                    case "Regular":
                        return AssetManager.Load<Font>("Fonts/Regular.ttf");
                    case "Medium":
                        return AssetManager.Load<Font>("Fonts/Medium.ttf");
                    case "SemiBold":
                        return AssetManager.Load<Font>("Fonts/SemiBold.ttf");
                    case "Bold":
                        return AssetManager.Load<Font>("Fonts/Bold.ttf");
                }
            }

            throw new Exception($"Uknown font : {font}");
        }

        public override void Write(Utf8JsonWriter writer, Font value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

    }
  
    public class Partition
    {
        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        public Root Root;
        public Partition(AssetManager assetManager, Interface @interface,string path)
        {
            options.Converters.Add(new RootConverter());
            options.Converters.Add(new Vector2Converter());
            options.Converters.Add(new ColorConverter());
            options.Converters.Add(new AlignmentConverter());
            options.Converters.Add(new TextureConverter(assetManager));
            options.Converters.Add(new FontConverter(assetManager));

            string hjson = File.ReadAllText(path);
            string json = HjsonValue.Parse(hjson).ToString();

            Debug.WriteLine(json);

            Root Root = JsonSerializer.Deserialize<Root>(json, options)!;
            this.Root = Root;
        }
    }
}
