# Protocol Documentation
<a name="top"></a>

## Table of Contents

- [com/empyreanmed/heracles/roles/v1/role.proto](#com_empyreanmed_heracles_roles_v1_role-proto)
    - [Role](#com-empyreanmed-heracles-roles-v1-Role)
  
- [com/empyreanmed/heracles/roles/v1/role_service.proto](#com_empyreanmed_heracles_roles_v1_role_service-proto)
    - [CreateRoleRequest](#com-empyreanmed-heracles-roles-v1-CreateRoleRequest)
    - [CreateRoleResponse](#com-empyreanmed-heracles-roles-v1-CreateRoleResponse)
    - [DeleteRoleRequest](#com-empyreanmed-heracles-roles-v1-DeleteRoleRequest)
    - [DeleteRoleResponse](#com-empyreanmed-heracles-roles-v1-DeleteRoleResponse)
    - [GetRoleRequest](#com-empyreanmed-heracles-roles-v1-GetRoleRequest)
    - [GetRoleResponse](#com-empyreanmed-heracles-roles-v1-GetRoleResponse)
    - [ListRolesRequest](#com-empyreanmed-heracles-roles-v1-ListRolesRequest)
    - [ListRolesResponse](#com-empyreanmed-heracles-roles-v1-ListRolesResponse)
    - [UpdateRoleRequest](#com-empyreanmed-heracles-roles-v1-UpdateRoleRequest)
    - [UpdateRoleResponse](#com-empyreanmed-heracles-roles-v1-UpdateRoleResponse)
  
    - [RoleService](#com-empyreanmed-heracles-roles-v1-RoleService)
  
- [Scalar Value Types](#scalar-value-types)



<a name="com_empyreanmed_heracles_roles_v1_role-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/roles/v1/role.proto



<a name="com-empyreanmed-heracles-roles-v1-Role"></a>

### Role
Represents a named role in the system with certain permissions.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | Unique identifier for this role (auto-increment). |
| creation_date | [google.protobuf.Timestamp](#google-protobuf-Timestamp) | optional | Timestamp when the role was created in the database. |
| role_name | [string](#string) | optional | The name assigned to this role, cannot be null. |
| description | [string](#string) | optional | An optional description of what this role signifies. |





 

 

 

 



<a name="com_empyreanmed_heracles_roles_v1_role_service-proto"></a>
<p align="right"><a href="#top">Top</a></p>

## com/empyreanmed/heracles/roles/v1/role_service.proto



<a name="com-empyreanmed-heracles-roles-v1-CreateRoleRequest"></a>

### CreateRoleRequest
Request message for creating a new role.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| role | [Role](#com-empyreanmed-heracles-roles-v1-Role) | optional | The role record to be created. |






<a name="com-empyreanmed-heracles-roles-v1-CreateRoleResponse"></a>

### CreateRoleResponse
Response after creating a new role.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| role | [Role](#com-empyreanmed-heracles-roles-v1-Role) |  | The newly created role object. |






<a name="com-empyreanmed-heracles-roles-v1-DeleteRoleRequest"></a>

### DeleteRoleRequest
Request message for deleting a role by ID.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The unique ID of the role to be deleted. |






<a name="com-empyreanmed-heracles-roles-v1-DeleteRoleResponse"></a>

### DeleteRoleResponse
Response confirming deletion of the specified role (empty).






<a name="com-empyreanmed-heracles-roles-v1-GetRoleRequest"></a>

### GetRoleRequest
Request message for retrieving a single role by ID.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| id | [int64](#int64) | optional | The ID of the role to be retrieved. |






<a name="com-empyreanmed-heracles-roles-v1-GetRoleResponse"></a>

### GetRoleResponse
Response containing the single role record.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| role | [Role](#com-empyreanmed-heracles-roles-v1-Role) |  | The role object matching the requested ID. |






<a name="com-empyreanmed-heracles-roles-v1-ListRolesRequest"></a>

### ListRolesRequest
Request message for listing roles.






<a name="com-empyreanmed-heracles-roles-v1-ListRolesResponse"></a>

### ListRolesResponse
Response containing the list of roles.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| roles | [Role](#com-empyreanmed-heracles-roles-v1-Role) | repeated | The roles matching the list request. |
| next_page_token | [string](#string) | optional | A token that can be used for pagination to retrieve next page of results. |






<a name="com-empyreanmed-heracles-roles-v1-UpdateRoleRequest"></a>

### UpdateRoleRequest
Request message for updating an existing role.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| role | [Role](#com-empyreanmed-heracles-roles-v1-Role) | optional | The role object with updated fields. |
| update_mask | [google.protobuf.FieldMask](#google-protobuf-FieldMask) | optional | Field mask specifying which fields to update. |






<a name="com-empyreanmed-heracles-roles-v1-UpdateRoleResponse"></a>

### UpdateRoleResponse
Response containing the updated role object.


| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| role | [Role](#com-empyreanmed-heracles-roles-v1-Role) |  | The updated role record after applying changes. |





 

 

 


<a name="com-empyreanmed-heracles-roles-v1-RoleService"></a>

### RoleService
RoleService handles CRUD operations for roles in the system.

| Method Name | Request Type | Response Type | Description |
| ----------- | ------------ | ------------- | ------------|
| ListRoles | [ListRolesRequest](#com-empyreanmed-heracles-roles-v1-ListRolesRequest) | [ListRolesResponse](#com-empyreanmed-heracles-roles-v1-ListRolesResponse) | Lists all roles available. |
| GetRole | [GetRoleRequest](#com-empyreanmed-heracles-roles-v1-GetRoleRequest) | [GetRoleResponse](#com-empyreanmed-heracles-roles-v1-GetRoleResponse) | Retrieves a single role by its ID. |
| CreateRole | [CreateRoleRequest](#com-empyreanmed-heracles-roles-v1-CreateRoleRequest) | [CreateRoleResponse](#com-empyreanmed-heracles-roles-v1-CreateRoleResponse) | Creates a new role record. |
| UpdateRole | [UpdateRoleRequest](#com-empyreanmed-heracles-roles-v1-UpdateRoleRequest) | [UpdateRoleResponse](#com-empyreanmed-heracles-roles-v1-UpdateRoleResponse) | Updates an existing role record by ID. |
| DeleteRole | [DeleteRoleRequest](#com-empyreanmed-heracles-roles-v1-DeleteRoleRequest) | [DeleteRoleResponse](#com-empyreanmed-heracles-roles-v1-DeleteRoleResponse) | Deletes a role record by ID. |

 



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

