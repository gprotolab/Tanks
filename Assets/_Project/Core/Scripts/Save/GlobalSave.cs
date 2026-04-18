using System.Collections.Generic;
using Newtonsoft.Json;

namespace ANut.Core.Save
{
    internal sealed class GlobalSave
    {
        [JsonProperty("slots")] public Dictionary<string, SaveSlot> Slots { get; set; } = new();
    }

    internal sealed class SaveSlot
    {
        [JsonProperty("version")] public int Version { get; set; }
        [JsonProperty("payload")] public string Payload { get; set; }
    }
}