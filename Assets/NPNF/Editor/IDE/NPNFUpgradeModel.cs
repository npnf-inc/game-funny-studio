using System;
using System.Collections.Generic;
using System.Linq;

namespace NPNF.Upgrade
{
    public class NPNFUpgreadeModel
    {
        public List<NPNFUpgradeVersionModel> releases { get; set; }

        public NPNFUpgreadeModel() { }

        public NPNFUpgradeVersionModel GetLatestVersion()
        {
            if (releases.Count > 0)
            {
                releases.Sort((v1, v2) =>
                {
                    return new Version(v2.version).CompareTo(new Version(v1.version));
                });
                return releases[0];
            }
            else
            {
                return null;
            }
        }

        public class NPNFUpgradeVersionModel
        {
            public string path { get; set; }
            public string version { get; set; }
            public string release_date { get; set; }
            public string sdk_size { get; set; }
            public string sample_size { get; set; }

            public NPNFUpgradeVersionModel() { }
        }
    }
}
