//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace zxing.Properties
{
    
    internal partial class Resources
    {
        private static System.Resources.ResourceManager manager;
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if ((Resources.manager == null))
                {
                    Resources.manager = new System.Resources.ResourceManager("zxing.Properties.Resources", typeof(Resources).Assembly);
                }
                return Resources.manager;
            }
        }
        internal static System.Drawing.Font GetFont(Resources.FontResources id)
        {
            return ((System.Drawing.Font)(ResourceManager.GetObject(((short)(id)))));
        }
        [System.SerializableAttribute()]
        internal enum FontResources : short
        {
            small = 13070,
        }
    }
}
