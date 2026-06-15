using System.Collections;
using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Xcc.Test.Xcc.Core.Domain.DataManagement.Common.Users
{
    public class UserPermissionsTests
    {
        [Test]
        public void UserPermissions_Defaults()
        {
            var sut  = new UserPermissions();
            var permissions = sut.Permissions.Select(x => x.Type).ToList();

            var expectedAll = Enum.GetValues<PermissionType>();
            
            Assert.That(permissions, Is.EqualTo(expectedAll).AsCollection);
            
            Assert.That(sut.ClinicalData, Is.False);
            Assert.That(sut.Treatment, Is.False);
            Assert.That(sut.SystemCalibration, Is.False);
            Assert.That(sut.QualityAssurance, Is.False);
            Assert.That(sut.SystemSettings, Is.False);
            Assert.That(sut.UserManagement, Is.False);
            Assert.That(sut.Services, Is.False);
        }
        
        [Test]
        public void UserPermissions_GettersSetters(
            [Values(false, true)] bool isClinicalData,
            [Values(false, true)] bool isTreatment,
            [Values(false, true)] bool isSystemCalibration,
            [Values(false, true)] bool isQualityAssurance,
            [Values(false, true)] bool isSystemSettings,
            [Values(false, true)] bool isUserManagement,
            [Values(false, true)] bool isServices)
        {
            var sut = new UserPermissions
            {
                ClinicalData = isClinicalData,
                Treatment = isTreatment,
                SystemCalibration = isSystemCalibration,
                QualityAssurance = isQualityAssurance,
                SystemSettings = isSystemSettings,
                UserManagement = isUserManagement,
                Services = isServices
            };
            
            var permissions = sut.Permissions.Select(x => x.Type).ToList();
            var expectedAll = Enum.GetValues<PermissionType>();
            
            Assert.That(permissions, Is.EqualTo(expectedAll).AsCollection);
            
            Assert.That(sut.ClinicalData, Is.EqualTo(isClinicalData));
            Assert.That(sut.Treatment, Is.EqualTo(isTreatment));
            Assert.That(sut.SystemCalibration, Is.EqualTo(isSystemCalibration));
            Assert.That(sut.QualityAssurance, Is.EqualTo(isQualityAssurance));
            Assert.That(sut.SystemSettings, Is.EqualTo(isSystemSettings));
            Assert.That(sut.UserManagement, Is.EqualTo(isUserManagement));
            Assert.That(sut.Services, Is.EqualTo(isServices));
        }
        
        [Test]
        public void UserPermissions_CopyCtor(
            [Values(false, true)] bool isClinicalData,
            [Values(false, true)] bool isTreatment,
            [Values(false, true)] bool isSystemCalibration,
            [Values(false, true)] bool isQualityAssurance,
            [Values(false, true)] bool isSystemSettings,
            [Values(false, true)] bool isUserManagement,
            [Values(false, true)] bool isServices)
        {
            var copyPermissions = new UserPermissions
            {
                ClinicalData = isClinicalData,
                Treatment = isTreatment,
                SystemCalibration = isSystemCalibration,
                QualityAssurance = isQualityAssurance,
                SystemSettings = isSystemSettings,
                UserManagement = isUserManagement,
                Services = isServices
            };
            var sut  = new UserPermissions(copyPermissions);
            
            var permissions = sut.Permissions.Select(x => x.Type).ToList();
            var expectedAll = Enum.GetValues<PermissionType>();
            
            Assert.That(permissions, Is.EqualTo(expectedAll).AsCollection);
            
            Assert.That(sut.ClinicalData, Is.EqualTo(isClinicalData));
            Assert.That(sut.Treatment, Is.EqualTo(isTreatment));
            Assert.That(sut.SystemCalibration, Is.EqualTo(isSystemCalibration));
            Assert.That(sut.QualityAssurance, Is.EqualTo(isQualityAssurance));
            Assert.That(sut.SystemSettings, Is.EqualTo(isSystemSettings));
            Assert.That(sut.UserManagement, Is.EqualTo(isUserManagement));
            Assert.That(sut.Services, Is.EqualTo(isServices));
        }
        
        private static bool GetPermissionValue(UserPermissions sut, PermissionType type) =>
            type switch
            {
                PermissionType.ClinicalData => sut.ClinicalData,
                PermissionType.Treatment => sut.Treatment,
                PermissionType.SystemCalibration => sut.SystemCalibration,
                PermissionType.QualityAssurance => sut.QualityAssurance,
                PermissionType.SystemSettings => sut.SystemSettings,
                PermissionType.UserManagement => sut.UserManagement,
                PermissionType.Services => sut.Services,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        
        private static IEnumerable<TestCaseData> PermissionCombinations()
        {
            var allTypes = Enum.GetValues<PermissionType>();

            foreach (int bitmask in Enumerable.Range(1, (1 << allTypes.Length) - 1))
            {
                var currentTypes = allTypes
                    .Where((_, i) => (bitmask & (1 << i)) != 0)
                    .ToList();

                yield return new TestCaseData(currentTypes)
                    .SetName($"UpdatePermissions_{string.Join("_", currentTypes)}");
            }
        }
        
        [TestCaseSource(nameof(PermissionCombinations))]
        public void UserPermissions_UpdatePermission_ToEnable(List<PermissionType> enabledPermissions)
        {
            var sut = new UserPermissions();
            
            foreach (var type in enabledPermissions)
            {
                var updatedPermission = new UserPermission(type) { Value = true };
                sut.UpdatePermission(updatedPermission);
            }
            
            foreach (var type in Enum.GetValues<PermissionType>())
            {
                bool expected = enabledPermissions.Contains(type);
                bool actual = GetPermissionValue(sut, type);
                Assert.That(actual, Is.EqualTo(expected), $"Permission {type}");
            }
        }
        
        [TestCaseSource(nameof(PermissionCombinations))]
        public void UserPermissions_UpdatePermission_ToDisable(List<PermissionType> disabledPermissions)
        {
            var sut = new UserPermissions
            {
                ClinicalData = true,
                Treatment = true,
                SystemCalibration = true,
                QualityAssurance = true,
                SystemSettings = true,
                UserManagement = true,
                Services = true
            };
            
            foreach (var type in disabledPermissions)
            {
                var updatedPermission = new UserPermission(type) { Value = false };
                sut.UpdatePermission(updatedPermission);
            }
            
            foreach (var type in Enum.GetValues<PermissionType>())
            {
                bool expected = !disabledPermissions.Contains(type);
                bool actual = GetPermissionValue(sut, type);
                Assert.That(actual, Is.EqualTo(expected), $"Permission {type}");
            }
        }
        
        [Test]
        public void UserPermissions_GetEnumerator()
        {
            var sut = new UserPermissions();

            var result = new List<UserPermission>();
            foreach (UserPermission permission in sut)
                result.Add(permission);

            var expectedTypes = Enum.GetValues<PermissionType>();
            var actualTypes = result.Select(p => p.Type);

            Assert.That(actualTypes, Is.EqualTo(expectedTypes).AsCollection);
        }
        
        [Test]
        public void UserPermissions_GetEnumerator2()
        {
            var permissions = new UserPermissions();
            IEnumerable sut = permissions;

            var result = new List<PermissionType>();
            foreach (var item in sut)
            {
                Assert.That(item, Is.InstanceOf<UserPermission>());
                result.Add(((UserPermission)item).Type);
            }

            var expectedTypes = Enum.GetValues<PermissionType>();
            Assert.That(result, Is.EqualTo(expectedTypes).AsCollection);
        }
    }
}