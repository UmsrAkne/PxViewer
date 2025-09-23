using System.Collections.Generic;

namespace PxViewer.Models
{
    public class PngGenerationMetadata
    {
        public List<string> PositivePrompts { get; set; } = new ();

        public List<string> NegativePrompts { get; set; } = new ();

        public int Steps { get; set; }

        public string Sampler { get; set; }

        public long Seed { get; set; }

        public string ModelName { get; set; }

        public string VaeName { get; set; }

        public string Version { get; set; }
    }
}