namespace ANut.Core.Save
{
    public interface ISaveModule
    {
        string Key { get; }

        int Version { get; }

        bool IsDirty { get; }

        void ResetDirty();

        string Serialize();

        /// Deserializes data from a JSON payload.
        /// Runs migration if <paramref name="fromVersion"/> is less than <see cref="Version"/>.
        void Deserialize(string payload, int fromVersion);
    }
}