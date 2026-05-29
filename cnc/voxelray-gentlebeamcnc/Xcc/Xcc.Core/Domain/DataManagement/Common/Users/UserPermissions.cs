using Prism.Mvvm;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Xcc.Core.Domain.DataManagement.Common.Users
{
    public class UserPermission : PermissionRecord
    {
        public UserPermission(PermissionType type)
        {
            Id = BaseEntry.NEW_ENTRY_ID;
            Type = type;
        }

        public UserPermission(PermissionRecord record)
        {
            Id = record.Id;
            Type= record.Type;
            Value = true;
            RoleId = record.RoleId;
        }

        public UserPermission(UserPermission userPermission)
        {
            Id = userPermission.Id;
            Type = userPermission.Type;
            Value = userPermission.Value;
            RoleId = userPermission.RoleId;
        }

        public bool Value { set; get; }
    }

    public class UserPermissions : BindableBase, IEnumerable<UserPermission>
    {
        public UserPermissions(UserPermissions userPermissions)
        {
            Permissions = userPermissions.Permissions.Select(x => new UserPermission(x)).ToList();
        }

        public UserPermissions()
        {
            Permissions = Enum.GetValues<PermissionType>().Select(type => new UserPermission(type)).ToList();
        }

        public IList<UserPermission> Permissions { get; }

        public void UpdatePermission(UserPermission permissionToUpdate)
        {
            var updatedPermissionIndex =
                Permissions.IndexOf(
                Permissions.Single(p => p.Type == permissionToUpdate.Type));

            Permissions[updatedPermissionIndex] = permissionToUpdate;

            //will work if property name and enum value as string are same
            RaisePropertyChanged(permissionToUpdate.Type.ToString());
        }

        private UserPermission FindPermission(PermissionType type)
        {
            return Permissions.Single(x => x.Type == type);
        }

        private void SetPermission(PermissionType type, bool value, [CallerMemberName] string? propertyName = null)
        {
            FindPermission(type).Value = value;
            RaisePropertyChanged(propertyName);
        }

        public bool ClinicalData
        {
            get => FindPermission(PermissionType.ClinicalData).Value;
            set => SetPermission(PermissionType.ClinicalData, value);
        }

        public bool Treatment
        {
            get => FindPermission(PermissionType.Treatment).Value;
            set => SetPermission(PermissionType.Treatment, value);
        }

        public bool SystemCalibration
        {
            get => FindPermission(PermissionType.SystemCalibration).Value;
            set => SetPermission(PermissionType.SystemCalibration, value);
        }

        public bool QualityAssurance
        {
            get => FindPermission(PermissionType.QualityAssurance).Value;
            set => SetPermission(PermissionType.QualityAssurance, value);
        }

        public bool SystemSettings
        {
            get => FindPermission(PermissionType.SystemSettings).Value;
            set => SetPermission(PermissionType.SystemSettings, value);
        }

        public bool UserManagement
        {
            get => FindPermission(PermissionType.UserManagement).Value;
            set => SetPermission(PermissionType.UserManagement, value);
        }

        public bool Services
        {
            get => FindPermission(PermissionType.Services).Value;
            set => SetPermission(PermissionType.Services, value);
        }


        #region IEnumerable
        public IEnumerator<UserPermission> GetEnumerator() => Permissions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion IEnumerable
    }
}
