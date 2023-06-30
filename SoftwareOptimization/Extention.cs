using SoftwareOptimization.Models.Entities;

namespace SoftwareOptimization {
    public static class Extention {
        public static int GetUserId(this System.Security.Principal.IIdentity identity) { 
            return int.Parse(identity.Name.Split('|')[0]);
        }
    }
}
