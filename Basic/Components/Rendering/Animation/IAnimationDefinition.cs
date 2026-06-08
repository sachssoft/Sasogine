namespace Sachssoft.Sasogine.Components.Rendering.Animation
{
    public class AnimationDefinition : IComponentDefinition
    {
        public string? Id { get; set; }

        public string? Class { get; set; }

        public float Speed { get; set; }

        public int Duration { get; set; }

        public bool Infinite { get; set; }

        public int Delay { get; set; }

    }
}
