namespace Soe.Business.Tests.Business.OrganizationStructure
{
    public class AttestRoleUserMockSettings
    {
        public int NbrOfUserAttestRoles { get; private set; }
        public int NbrOfAccounts { get; private set; }
        public bool DoSetFirstAttestRoleAsShowAll { get; private set; }
        public EAccountStructureStartLevelOption StartLevelOption { get; private set; }
        public EAccountStructureSubLevelOption SubLevelOption { get; private set; }

        public static AttestRoleUserMockSettings Create(
            int nbrOfUserAttestRoles = 1,
            int nbrOfAccounts = 1,
            bool doSetFirstAttestRoleAsShowAll = false,
            EAccountStructureStartLevelOption startLevelOption = EAccountStructureStartLevelOption.Standard,
            EAccountStructureSubLevelOption subLevelOption = EAccountStructureSubLevelOption.NoSubLevel
        )
        {
            return new AttestRoleUserMockSettings
            {
                NbrOfUserAttestRoles = nbrOfUserAttestRoles,
                NbrOfAccounts = nbrOfAccounts,
                DoSetFirstAttestRoleAsShowAll = doSetFirstAttestRoleAsShowAll,
                StartLevelOption = startLevelOption,
                SubLevelOption = subLevelOption,
            };
        }
    }
}
