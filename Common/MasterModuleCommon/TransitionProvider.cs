using System;
using System.Collections.Generic;
using System.Drawing;
using ScreenConnection;

namespace MasterModuleCommon
{
    public abstract class TransitionProvider
    {
        public abstract void Init(IList<KeyValuePair<string, string>> parameters);

        public abstract IEnumerator<Tuple<Bitmap, Screen>> GetTransitions(IList<Tuple<Bitmap, Screen>> data);
    }
}
