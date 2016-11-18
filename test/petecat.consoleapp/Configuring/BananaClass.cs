﻿using Petecat.Configuring;
using Petecat.Formatter.Attribute;
using Petecat.Configuring.Attribute;

namespace Petecat.ConsoleApp.Configuring
{
    [StaticFileConfigElement(
        Key = "banana", 
        Path = "./banana.json", 
        FileFormat = "json", 
        Inference = typeof(IBananaInterface))]
    public class BananaClass : StaticFileConfigInstanceBase, IBananaInterface
    {
        [JsonProperty(Alias = "name")]
        public string Name { get; set; }

        [JsonProperty(Alias = "age")]
        public int Age { get; set; }
    }
}