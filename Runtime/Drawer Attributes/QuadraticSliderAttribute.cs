using NaughtyAttributes;

namespace Hairibar.NaughtyExtensions
{
    public class QuadraticSliderAttribute : DrawerAttribute
    {
        public float Power { get; private set; }
        public float Min { get; private set; }
        public float Max { get; private set; }

        public QuadraticSliderAttribute(float min, float max, float power)
        {
            Min = min;
            Max = max;
            Power = power;
        }
    }
}
