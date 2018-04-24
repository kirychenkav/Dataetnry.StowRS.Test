using System.ComponentModel;

namespace stowRs.test
{
    public enum BatchType
    {
        [Description("Separate requests for each patient")]
        RequestPerPatient,
        [Description("Sinlge requests for all patients")]
        RequestAllData
    }
}