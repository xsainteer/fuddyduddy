namespace FuddyDuddy.Core.Infrastructure.Configuration;

public class AiModels
{
    public ModelOptions Ollama { get; set; }
    public ModelOptions Gemini { get; set; }
    public ModelOptions Gemini2 { get; set; }
    public Dictionary<string, Mapping> ResponseMappings { get; set; }
    public class ModelOptions
    {
        public string ApiKey { get; set; } // Gemini specific
        public string Url { get; set; }
        public List<Characteristic> Models { get; set; }
        public double Temperature { get; set; } // Ollama specific
        public class Characteristic
        {
            public Type Type { get; set; }
            public string Name { get; set; }
            public int MaxTokens { get; set; }
            public double Temperature { get; set; }
            public string KeepAlive { get; set; } = "5m";
            public int RatePerMinute { get; set; } = 0; // Gemini specific
        }
    }

    public class Mapping
    {
        public string Model { get; set; }
        public Type ModelType { get; set; }
    }

    public enum Type
    {
        SuperLight,
        Light,
        Pro,
        Embedding
    }
}