using System.Collections.Generic;
using System.Drawing;
using NLog;
using ScreenConnection;

namespace MasterModuleCommon
{
    public abstract class GraphicProvider
    {
        public abstract void Init(Logger logger, IList<KeyValuePair<string, string>> parameters);

        public abstract Bitmap GetNextGraphic(Screen target);
    }
}
