using System.Collections.Generic;
using ScreenConnection;

namespace MasterModuleCommon
{
    public abstract class TargetProvider
    {
        public abstract void Init(IList<KeyValuePair<string, string>> parameters);

        public abstract IEnumerator<Screen> GetTargets();

    }
}
