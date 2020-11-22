using NaughtyAttributes;

namespace Hairibar.NaughtyExtensions
{
    public class InlineAttribute : DrawerAttribute
    {
        public bool ShowHeaderAndBox { get; private set; } = true;

        public InlineAttribute() { }

        public InlineAttribute(bool showHeaderAndBox)
        {
            ShowHeaderAndBox = showHeaderAndBox;
        }
    }
}
