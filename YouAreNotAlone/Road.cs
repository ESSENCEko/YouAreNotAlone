using GTA.Math;

namespace YouAreNotAlone
{
    public class Road
    {
        public Vector3 Position { get; }
        public float Heading { get; }

        public Road(Vector3 position, float heading)
        {
            this.Position = position;
            this.Heading = heading;
        }
    }
}