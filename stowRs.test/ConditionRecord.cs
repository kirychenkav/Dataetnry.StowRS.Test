using System;

namespace stowRs.test
{
    public abstract class BaseRecord
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public DateTime Published { get; set; }
        public DateTime Updated { get; set; }
        public string Identifier { get; set; }
    }

    public class Reference
    {
        public string Id { get; set; }
    }

    public class Coding
    {
        public string System { get; set; }
        public string Code { get; set; }
    }

    public class ConditionRecord : BaseRecord
    {
        public int AgeOnset { get; set; }
        public Reference Encounter { get; set; }
        public Reference Patient { get; set; }
        public Coding CaseDefinition { get; set; }
        public SocialData SocialData { get; set; }
        public Anamnesis Anamnesis { get; set; }
        public DateTime RegistrationDate { get; set; }
    }

    public class SocialData
    {
        public Coding[] RiskFactors { get; set; }
        public Coding Education { get; set; }
        public Coding Employment { get; set; }
        public int? TotalContacts { get; set; }
        public int? TotalChildren { get; set; }
    }

    public class Anamnesis
    {
        public Coding Localization { get; set; }
        public Coding[] Comorbidity { get; set; }
        public Coding Diagnosis { get; set; }
        public Coding DstProfile { get; set; }
        public decimal? Weigth { get; set; }
        public decimal? Height { get; set; }
        public string Comments { get; set; }
    }
}