# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/roles_permissions/v1/role_permission.proto](#com_empyreanmed_heracles_roles_permissions_v1_role_permission-proto)
    - [RolesPermissions](#com-empyreanmed-heracles-roles_permissions-v1-RolesPermissions)
  
- [com/empyreanmed/heracles/roles_permissions/v1/roles_permission_service.proto](#com_empyreanmed_heracles_roles_permissions_v1_roles_permission_service-proto)
    - [CreateRolesPermissionsRequest](#com-empyreanmed-heracles-roles_permissions-v1-CreateRolesPermissionsRequest)
    - [CreateRolesPermissionsResponse](#com-empyreanmed-heracles-roles_permissions-v1-CreateRolesPermissionsResponse)
    - [DeleteRolesPermissionsRequest](#com-empyreanmed-heracles-roles_permissions-v1-DeleteRolesPermissionsRequest)
    - [DeleteRolesPermissionsResponse](#com-empyreanmed-heracles-roles_permissions-v1-DeleteRolesPermissionsResponse)
    - [GetRolesPermissionsRequest](#com-empyreanmed-heracles-roles_permissions-v1-GetRolesPermissionsRequest)
    - [GetRolesPermissionsResponse](#com-empyreanmed-heracles-roles_permissions-v1-GetRolesPermissionsResponse)
    - [ListRolesPermissionsRequest](#com-empyreanmed-heracles-roles_permissions-v1-ListRolesPermissionsRequest)
    - [ListRolesPermissionsResponse](#com-empyreanmed-heracles-roles_permissions-v1-ListRolesPermissionsResponse)
    - [UpdateRolesPermissionsRequest](#com-empyreanmed-heracles-roles_permissions-v1-UpdateRolesPermissionsRequest)
    - [UpdateRolesPermissionsResponse](#com-empyreanmed-heracles-roles_permissions-v1-UpdateRolesPermissionsResponse)
  
    - [RolesPermissionsService](#com-empyreanmed-heracles-roles_permissions-v1-RolesPermissionsService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_roles_permissions_v1_role_permission-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/roles_permissions/v1/role_permission.proto



<a name="com-empyreanmed-heracles-roles_permissions-v1-RolesPermissions"></a>

### RolesPermissions
Represents a link between a role and a permission in the system.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for this role (auto-increment). |
| role_id | [int64](#int64) | optional | The ID of the role that is assigned the permission. |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | The time when this link was created in the database. |
| permission | [com.empyreanmed.heracles.enums.v1.PERMISSION](#com-empyreanmed-heracles-enums-v1-PERMISSION) | optional | role permission |





 

 

 

 



<a name="com_empyreanmed_heracles_roles_permissions_v1_roles_permission_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/roles_permissions/v1/roles_permission_service.proto



<a name="com-empyreanmed-heracles-roles_permissions-v1-CreateRolesPermissionsRequest"></a>

### CreateRolesPermissionsRequest
Request for creating a role-permission link.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| roles_permissions | [RolesPermissions](#com-empyreanmed-heracles-roles_permissions-v1-RolesPermissions) | optional | The pivot record for associating a role with a permission. |






<a name="com-empyreanmed-heracles-roles_permissions-v1-CreateRolesPermissionsResponse"></a>

### CreateRolesPermissionsResponse
Response containing the newly created role-permission link.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| roles_permissions | [RolesPermissions](#com-empyreanmed-heracles-roles_permissions-v1-RolesPermissions) |  | The roles_permissions pivot record that was created. |






<a name="com-empyreanmed-heracles-roles_permissions-v1-DeleteRolesPermissionsRequest"></a>

### DeleteRolesPermissionsRequest
Request for deleting a role-permission link by composite key.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the role permission in the pivot. |






<a name="com-empyreanmed-heracles-roles_permissions-v1-DeleteRolesPermissionsResponse"></a>

### DeleteRolesPermissionsResponse
Response confirming deletion of the pivot record (empty).






<a name="com-empyreanmed-heracles-roles_permissions-v1-GetRolesPermissionsRequest"></a>

### GetRolesPermissionsRequest
Request for retrieving a single role-permission link.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of role permission in the pivot record. |
| permission_id | [int64](#int64) | optional | The ID of the permission in the pivot record. |






<a name="com-empyreanmed-heracles-roles_permissions-v1-GetRolesPermissionsResponse"></a>

### GetRolesPermissionsResponse
Response containing the requested role-permission link.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| roles_permissions | [RolesPermissions](#com-empyreanmed-heracles-roles_permissions-v1-RolesPermissions) |  | The roles_permissions pivot object matching the request. |






<a name="com-empyreanmed-heracles-roles_permissions-v1-ListRolesPermissionsRequest"></a>

### ListRolesPermissionsRequest
Request for listing role-permission links.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| role_id | [int64](#int64) | optional | Filter results by parent&#39;s ID. |






<a name="com-empyreanmed-heracles-roles_permissions-v1-ListRolesPermissionsResponse"></a>

### ListRolesPermissionsResponse
Response containing an array of roles-permissions links.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| roles_permissions | [RolesPermissions](#com-empyreanmed-heracles-roles_permissions-v1-RolesPermissions) | repeated | The collection of roles_permissions pivot records. |
| next_page_token | [string](#string) | optional | A token for retrieving the next page of results. |






<a name="com-empyreanmed-heracles-roles_permissions-v1-UpdateRolesPermissionsRequest"></a>

### UpdateRolesPermissionsRequest
Request for updating an existing role-permission link.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| roles_permissions | [RolesPermissions](#com-empyreanmed-heracles-roles_permissions-v1-RolesPermissions) | optional | The pivot record with updated fields. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | The field mask specifying which fields to update. |






<a name="com-empyreanmed-heracles-roles_permissions-v1-UpdateRolesPermissionsResponse"></a>

### UpdateRolesPermissionsResponse
Response after updating the role-permission link.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| roles_permissions | [RolesPermissions](#com-empyreanmed-heracles-roles_permissions-v1-RolesPermissions) |  | The updated roles_permissions pivot object. |





 

 

 


<a name="com-empyreanmed-heracles-roles_permissions-v1-RolesPermissionsService"></a>

### RolesPermissionsService
RolesPermissionsService handles pivot records linking roles and permissions.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListRolesPermissions | [ListRolesPermissionsRequest](#com-empyreanmed-heracles-roles_permissions-v1-ListRolesPermissionsRequest) | [ListRolesPermissionsResponse](#com-empyreanmed-heracles-roles_permissions-v1-ListRolesPermissionsResponse) | Lists role-permission links in the system. |
| GetRolesPermissions | [GetRolesPermissionsRequest](#com-empyreanmed-heracles-roles_permissions-v1-GetRolesPermissionsRequest) | [GetRolesPermissionsResponse](#com-empyreanmed-heracles-roles_permissions-v1-GetRolesPermissionsResponse) | Retrieves a single role-permission link by role_id and permission_id. |
| CreateRolesPermissions | [CreateRolesPermissionsRequest](#com-empyreanmed-heracles-roles_permissions-v1-CreateRolesPermissionsRequest) | [CreateRolesPermissionsResponse](#com-empyreanmed-heracles-roles_permissions-v1-CreateRolesPermissionsResponse) | Creates a new role-permission link. |
| UpdateRolesPermissions | [UpdateRolesPermissionsRequest](#com-empyreanmed-heracles-roles_permissions-v1-UpdateRolesPermissionsRequest) | [UpdateRolesPermissionsResponse](#com-empyreanmed-heracles-roles_permissions-v1-UpdateRolesPermissionsResponse) | Updates an existing role-permission link. |
| DeleteRolesPermissions | [DeleteRolesPermissionsRequest](#com-empyreanmed-heracles-roles_permissions-v1-DeleteRolesPermissionsRequest) | [DeleteRolesPermissionsResponse](#com-empyreanmed-heracles-roles_permissions-v1-DeleteRolesPermissionsResponse) | Deletes a role-permission link by composite key. |

 



## Scalar Value Types

| .proto Type | Notes | C++ | Java | Python | Go | C# | PHP | Ruby |
| ----------- | ----- | --- | ---- | ------ | -- | -- | --- | ---- |
| <a name="double" /> double |  | double | double | float | float64 | double | float | Float |
| <a name="float" /> float |  | float | float | float | float32 | float | float | Float |
| <a name="int32" /> int32 | Uses variable-length encoding. Inefficient for encoding negative numbers – if your field is likely to have negative values, use sint32 instead. | int32 | int | int | int32 | int | integer | Bignum or Fixnum (as required) |
| <a name="int64" /> int64 | Uses variable-length encoding. Inefficient for encoding negative numbers – if your field is likely to have negative values, use sint64 instead. | int64 | long | int/long | int64 | long | integer/string | Bignum |
| <a name="uint32" /> uint32 | Uses variable-length encoding. | uint32 | int | int/long | uint32 | uint | integer | Bignum or Fixnum (as required) |
| <a name="uint64" /> uint64 | Uses variable-length encoding. | uint64 | long | int/long | uint64 | ulong | integer/string | Bignum or Fixnum (as required) |
| <a name="sint32" /> sint32 | Uses variable-length encoding. Signed int value. These more efficiently encode negative numbers than regular int32s. | int32 | int | int | int32 | int | integer | Bignum or Fixnum (as required) |
| <a name="sint64" /> sint64 | Uses variable-length encoding. Signed int value. These more efficiently encode negative numbers than regular int64s. | int64 | long | int/long | int64 | long | integer/string | Bignum |
| <a name="fixed32" /> fixed32 | Always four bytes. More efficient than uint32 if values are often greater than 2^28. | uint32 | int | int | uint32 | uint | integer | Bignum or Fixnum (as required) |
| <a name="fixed64" /> fixed64 | Always eight bytes. More efficient than uint64 if values are often greater than 2^56. | uint64 | long | int/long | uint64 | ulong | integer/string | Bignum |
| <a name="sfixed32" /> sfixed32 | Always four bytes. | int32 | int | int | int32 | int | integer | Bignum or Fixnum (as required) |
| <a name="sfixed64" /> sfixed64 | Always eight bytes. | int64 | long | int/long | int64 | long | integer/string | Bignum |
| <a name="bool" /> bool |  | bool | boolean | boolean | bool | bool | boolean | TrueClass/FalseClass |
| <a name="string" /> string | A string must always contain UTF-8 encoded or 7-bit ASCII text. | string | String | str/unicode | string | string | string | String (UTF-8) |
| <a name="bytes" /> bytes | May contain any arbitrary sequence of bytes. | string | ByteString | str | []byte | ByteString | string | String (ASCII-8BIT) |

